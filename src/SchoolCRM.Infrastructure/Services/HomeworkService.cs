using System.Linq.Expressions;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IHomeworkService;

namespace SchoolCRM.Infrastructure.Services;

public class HomeworkService : IHomeworkService
{
    private readonly IUnitOfWork _unitOfWork;

    public HomeworkService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<HomeworkDto>>> GetHomeworkAsync(
        PaginationQuery query, Guid? classRoomId, Guid? sectionId, Guid? subjectId,
        DateOnly? fromDate, DateOnly? toDate)
    {
        try
        {
            var homeworks = await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().GetAllAsync();
            var filtered = homeworks.Where(h => !h.IsDeleted).ToList();

            if (classRoomId.HasValue)
                filtered = filtered.Where(h => h.ClassRoomId == classRoomId.Value).ToList();
            if (subjectId.HasValue)
                filtered = filtered.Where(h => h.SubjectId == subjectId.Value).ToList();

            var totalCount = filtered.Count;
            var pagedItems = filtered
                .OrderByDescending(h => h.AssignedDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(h => new HomeworkDto
                {
                    Id = h.Id,
                    Title = h.Title,
                    Description = h.Description,
                    SubjectId = h.SubjectId,
                    SubjectName = h.Subject?.Name ?? string.Empty,
                    ClassRoomId = h.ClassRoomId,
                    ClassName = h.ClassRoom?.Name ?? string.Empty,
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
            var hw = await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().GetByIdAsync(id);
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
            var hw = new Domain.Entities.Homework.Homework
            {
                Title = dto.Title,
                Description = dto.Description,
                SubjectId = dto.SubjectId,
                ClassRoomId = dto.ClassRoomId,
                TeacherId = Guid.Empty,
                SchoolId = Guid.Empty,
                AssignedDate = dto.AssignedDate.ToDateTime(TimeOnly.MinValue),
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

            hw.Title = dto.Title;
            hw.Description = dto.Description;
            hw.SubjectId = dto.SubjectId;
            hw.ClassRoomId = dto.ClassRoomId;
            hw.AssignedDate = dto.AssignedDate.ToDateTime(TimeOnly.MinValue);
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
