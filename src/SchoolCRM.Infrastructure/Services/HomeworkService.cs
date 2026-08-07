using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IHomeworkService;

namespace SchoolCRM.Infrastructure.Services;

public class HomeworkService : IHomeworkService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public HomeworkService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    private async Task<(Guid SchoolId, Guid TeacherId)> ResolveSchoolAndTeacherAsync()
    {
        var schoolId = _currentUserService.SchoolId;
        if (schoolId is null || schoolId == Guid.Empty)
        {
            var schools = await _unitOfWork.Schools.GetAllAsync();
            schoolId = schools.FirstOrDefault()?.Id;
        }

        Guid? teacherId = null;
        if (!string.IsNullOrEmpty(_currentUserService.UserId))
        {
            var teachers = await _unitOfWork.Teachers.FindAsync(t =>
                t.UserId == Guid.Parse(_currentUserService.UserId) && !t.IsDeleted);
            teacherId = teachers.FirstOrDefault()?.Id;
        }

        teacherId ??= (await _unitOfWork.Teachers.FindAsync(t => !t.IsDeleted))
            .FirstOrDefault()?.Id;

        return (schoolId ?? Guid.Empty, teacherId ?? Guid.Empty);
    }

    public async Task<ApiResponse<PagedResult<HomeworkDto>>> GetHomeworkAsync(
        PaginationQuery query, Guid? classRoomId, Guid? sectionId, Guid? subjectId,
        DateOnly? fromDate, DateOnly? toDate)
    {
        try
        {
            var repo = _unitOfWork.Repository<Domain.Entities.Homework.Homework>();

            var (items, totalCount) = await repo.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                filter: h => !h.IsDeleted &&
                              (!classRoomId.HasValue || h.ClassRoomId == classRoomId.Value) &&
                              (!subjectId.HasValue || h.SubjectId == subjectId.Value),
                orderBy: q => q.OrderByDescending(h => h.AssignedDate),
                include: q => q.Include(h => h.ClassRoom)
                               .Include(h => h.Section)
                               .Include(h => h.Subject)
                               .Include(h => h.Teacher)
                               .ThenInclude(t => t.User));

            var pagedItems = items
                .Select(h => new HomeworkDto
                {
                    Id = h.Id,
                    Title = h.Title,
                    Description = h.Description,
                    SubjectId = h.SubjectId,
                    SubjectName = h.Subject?.Name ?? string.Empty,
                    ClassRoomId = h.ClassRoomId,
                    ClassName = h.ClassRoom?.Name ?? string.Empty,
                    SectionId = h.SectionId ?? Guid.Empty,
                    SectionName = h.Section?.Name ?? string.Empty,
                    TeacherId = h.TeacherId,
                    TeacherName = h.Teacher?.User is not null
                        ? $"{h.Teacher.User.FirstName} {h.Teacher.User.LastName}"
                        : string.Empty,
                    AssignedDate = DateOnly.FromDateTime(h.AssignedDate),
                    DueDate = DateOnly.FromDateTime(h.DueDate),
                    AttachmentUrl = h.AttachmentUrl,
                    IsActive = !h.IsDeleted
                }).ToList();

            return ApiResponse<PagedResult<HomeworkDto>>.SuccessResponse(new PagedResult<HomeworkDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<HomeworkDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkDto>> GetHomeworkByIdAsync(Guid id)
    {
        try
        {
            var repo = _unitOfWork.Repository<Domain.Entities.Homework.Homework>();

            var (items, _) = await repo.GetPagedAsync(
                1,
                1,
                filter: h => h.Id == id && !h.IsDeleted,
                include: q => q.Include(h => h.ClassRoom)
                               .Include(h => h.Section)
                               .Include(h => h.Subject)
                               .Include(h => h.Teacher)
                               .ThenInclude(t => t.User));

            var hw = items.FirstOrDefault();
            if (hw is null)
                return ApiResponse<HomeworkDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<HomeworkDto>.SuccessResponse(new HomeworkDto
            {
                Id = hw.Id,
                Title = hw.Title,
                Description = hw.Description,
                SubjectId = hw.SubjectId,
                SubjectName = hw.Subject?.Name ?? string.Empty,
                ClassRoomId = hw.ClassRoomId,
                ClassName = hw.ClassRoom?.Name ?? string.Empty,
                SectionId = hw.SectionId ?? Guid.Empty,
                SectionName = hw.Section?.Name ?? string.Empty,
                TeacherId = hw.TeacherId,
                TeacherName = hw.Teacher?.User is not null
                    ? $"{hw.Teacher.User.FirstName} {hw.Teacher.User.LastName}"
                    : string.Empty,
                AssignedDate = DateOnly.FromDateTime(hw.AssignedDate),
                DueDate = DateOnly.FromDateTime(hw.DueDate),
                AttachmentUrl = hw.AttachmentUrl,
                IsActive = !hw.IsDeleted
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<HomeworkDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkDto>> CreateHomeworkAsync(CreateHomeworkDto dto)
    {
        try
        {
            var (schoolId, teacherId) = await ResolveSchoolAndTeacherAsync();
            if (schoolId == Guid.Empty || teacherId == Guid.Empty)
                return ApiResponse<HomeworkDto>.FailResponse("Unable to determine the current school or teacher context. Please sign in again.");

            var assignedDate = dto.AssignedDate == default
                ? DateOnly.FromDateTime(DateTime.UtcNow)
                : dto.AssignedDate;

            var hw = new Domain.Entities.Homework.Homework
            {
                Title = dto.Title,
                Description = dto.Description,
                SubjectId = dto.SubjectId,
                ClassRoomId = dto.ClassRoomId,
                SectionId = dto.SectionId,
                TeacherId = teacherId,
                SchoolId = schoolId,
                AssignedDate = assignedDate.ToDateTime(TimeOnly.MinValue),
                DueDate = dto.DueDate.ToDateTime(TimeOnly.MinValue),
                AttachmentUrl = dto.AttachmentUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().AddAsync(hw);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<HomeworkDto>.SuccessResponse(new HomeworkDto
            {
                Id = hw.Id,
                Title = hw.Title,
                Description = hw.Description,
                SubjectId = hw.SubjectId,
                SubjectName = hw.Subject?.Name ?? string.Empty,
                ClassRoomId = hw.ClassRoomId,
                ClassName = hw.ClassRoom?.Name ?? string.Empty,
                AssignedDate = DateOnly.FromDateTime(hw.AssignedDate),
                DueDate = DateOnly.FromDateTime(hw.DueDate),
                AttachmentUrl = hw.AttachmentUrl,
                IsActive = true
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<HomeworkDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkDto>> UpdateHomeworkAsync(Guid id, CreateHomeworkDto dto)
    {
        try
        {
            var hw = await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().GetByIdAsync(id);
            if (hw is null)
                return ApiResponse<HomeworkDto>.NotFoundResponse(ApplicationMessages.NotFound);

            var (schoolId, teacherId) = await ResolveSchoolAndTeacherAsync();
            if (schoolId == Guid.Empty || teacherId == Guid.Empty)
                return ApiResponse<HomeworkDto>.FailResponse("Unable to determine the current school or teacher context. Please sign in again.");

            hw.Title = dto.Title;
            hw.Description = dto.Description;
            hw.SubjectId = dto.SubjectId;
            hw.ClassRoomId = dto.ClassRoomId;
            hw.SectionId = dto.SectionId;
            hw.TeacherId = teacherId;
            hw.SchoolId = schoolId;
            hw.AssignedDate = (dto.AssignedDate == default
                ? DateOnly.FromDateTime(DateTime.UtcNow)
                : dto.AssignedDate).ToDateTime(TimeOnly.MinValue);
            hw.DueDate = dto.DueDate.ToDateTime(TimeOnly.MinValue);
            hw.AttachmentUrl = dto.AttachmentUrl;
            hw.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().UpdateAsync(hw);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<HomeworkDto>.SuccessResponse(new HomeworkDto
            {
                Id = hw.Id,
                Title = hw.Title,
                Description = hw.Description,
                SubjectId = hw.SubjectId,
                ClassRoomId = hw.ClassRoomId,
                AssignedDate = DateOnly.FromDateTime(hw.AssignedDate),
                DueDate = DateOnly.FromDateTime(hw.DueDate),
                AttachmentUrl = hw.AttachmentUrl,
                IsActive = true
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<HomeworkDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteHomeworkAsync(Guid id)
    {
        try
        {
            var hw = await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().GetByIdAsync(id);
            if (hw is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            hw.IsDeleted = true;
            hw.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().UpdateAsync(hw);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<AssignmentDto>>> GetAssignmentsAsync(
        PaginationQuery query, Guid? studentId, string? status)
    {
        try
        {
            var submissions = await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>().GetAllAsync();
            var filtered = submissions.Where(s => !s.IsDeleted).ToList();

            if (studentId.HasValue)
                filtered = filtered.Where(s => s.StudentId == studentId.Value).ToList();

            var totalCount = filtered.Count;
            var pagedItems = filtered
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(s => new AssignmentDto
                {
                    Id = s.Id,
                    HomeworkId = s.HomeworkId,
                    HomeworkTitle = s.Homework?.Title ?? string.Empty,
                    StudentId = s.StudentId,
                    StudentName = s.Student?.User is not null
                        ? $"{s.Student.User.FirstName} {s.Student.User.LastName}"
                        : string.Empty,
                    SubmissionText = s.SubmittedText,
                    AttachmentUrl = s.AttachmentUrl,
                    SubmittedDate = s.SubmittedAt,
                    Marks = s.MarksObtained,
                    Remarks = s.TeacherRemarks,
                    Status = s.Status.ToString(),
                    GradedDate = s.ReviewedAt
                }).ToList();

            return ApiResponse<PagedResult<AssignmentDto>>.SuccessResponse(new PagedResult<AssignmentDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<AssignmentDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<AssignmentDto>> GetAssignmentByIdAsync(Guid id)
    {
        try
        {
            var submission = await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>().GetByIdAsync(id);
            if (submission is null)
                return ApiResponse<AssignmentDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<AssignmentDto>.SuccessResponse(new AssignmentDto
            {
                Id = submission.Id,
                HomeworkId = submission.HomeworkId,
                HomeworkTitle = submission.Homework?.Title ?? string.Empty,
                StudentId = submission.StudentId,
                StudentName = submission.Student?.User is not null
                    ? $"{submission.Student.User.FirstName} {submission.Student.User.LastName}"
                    : string.Empty,
                SubmissionText = submission.SubmittedText,
                AttachmentUrl = submission.AttachmentUrl,
                SubmittedDate = submission.SubmittedAt,
                Marks = submission.MarksObtained,
                Remarks = submission.TeacherRemarks,
                Status = submission.Status.ToString(),
                GradedDate = submission.ReviewedAt
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<AssignmentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<AssignmentDto>> SubmitAssignmentAsync(SubmitAssignmentDto dto)
    {
        try
        {
            var existing = (await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>()
                .FindAsync(s => s.HomeworkId == dto.HomeworkId && s.StudentId == dto.StudentId && !s.IsDeleted))
                .FirstOrDefault();

            if (existing is not null)
                return ApiResponse<AssignmentDto>.FailResponse("Assignment already submitted.");

            var submission = new Domain.Entities.Homework.HomeworkSubmission
            {
                HomeworkId = dto.HomeworkId,
                StudentId = dto.StudentId,
                SubmittedText = dto.SubmissionText,
                AttachmentUrl = dto.AttachmentUrl,
                SubmittedAt = DateTime.UtcNow,
                Status = HomeworkStatus.Submitted,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>().AddAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<AssignmentDto>.SuccessResponse(new AssignmentDto
            {
                Id = submission.Id,
                HomeworkId = submission.HomeworkId,
                StudentId = submission.StudentId,
                SubmissionText = submission.SubmittedText,
                AttachmentUrl = submission.AttachmentUrl,
                SubmittedDate = submission.SubmittedAt,
                Status = submission.Status.ToString()
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<AssignmentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<AssignmentDto>> GradeAssignmentAsync(Guid assignmentId, GradeAssignmentDto dto)
    {
        try
        {
            var submission = await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>().GetByIdAsync(assignmentId);
            if (submission is null)
                return ApiResponse<AssignmentDto>.NotFoundResponse(ApplicationMessages.NotFound);

            submission.MarksObtained = dto.Marks;
            submission.TeacherRemarks = dto.Remarks;
            submission.Status = HomeworkStatus.Reviewed;
            submission.ReviewedAt = DateTime.UtcNow;
            submission.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>().UpdateAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<AssignmentDto>.SuccessResponse(new AssignmentDto
            {
                Id = submission.Id,
                HomeworkId = submission.HomeworkId,
                HomeworkTitle = submission.Homework?.Title ?? string.Empty,
                StudentId = submission.StudentId,
                StudentName = submission.Student?.User is not null
                    ? $"{submission.Student.User.FirstName} {submission.Student.User.LastName}"
                    : string.Empty,
                Marks = submission.MarksObtained,
                Remarks = submission.TeacherRemarks,
                Status = submission.Status.ToString(),
                GradedDate = submission.ReviewedAt
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<AssignmentDto>.FailResponse(ex.Message);
        }
    }
}
