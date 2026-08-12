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
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    public HomeworkService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _emailService = emailService;
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

    private async Task<Guid> ResolveStudentIdAsync()
    {
        if (!string.IsNullOrEmpty(_currentUserService.UserId))
        {
            var student = await _unitOfWork.Students.GetStudentByUserIdAsync(Guid.Parse(_currentUserService.UserId));
            if (student is not null)
                return student.Id;
        }

        return (await _unitOfWork.Students.FindAsync(s => !s.IsDeleted))
            .FirstOrDefault()?.Id ?? Guid.Empty;
    }

    private async Task<Guid> ResolveTeacherIdAsync()
    {
        if (!string.IsNullOrEmpty(_currentUserService.UserId))
        {
            var teachers = await _unitOfWork.Teachers.FindAsync(t =>
                t.UserId == Guid.Parse(_currentUserService.UserId) && !t.IsDeleted);
            var teacherId = teachers.FirstOrDefault()?.Id;
            if (teacherId.HasValue)
                return teacherId.Value;
        }

        return (await _unitOfWork.Teachers.FindAsync(t => !t.IsDeleted))
            .FirstOrDefault()?.Id ?? Guid.Empty;
    }

    private async Task<(Guid ClassRoomId, Guid SectionId)> ResolveStudentScopeAsync()
    {
        var studentId = await ResolveStudentIdAsync();
        if (studentId == Guid.Empty)
            return (Guid.Empty, Guid.Empty);

        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student is null)
            return (Guid.Empty, Guid.Empty);

        var section = await _unitOfWork.Sections.GetByIdAsync(student.SectionId);
        if (section is null)
            return (Guid.Empty, Guid.Empty);

        return (section.ClassRoomId, student.SectionId);
    }

    private bool IsStudentRole() =>
        !_currentUserService.Roles.Contains(nameof(RoleType.SuperAdmin)) &&
        !_currentUserService.Roles.Contains(nameof(RoleType.SchoolAdmin)) &&
        !_currentUserService.Roles.Contains(nameof(RoleType.Principal)) &&
        !_currentUserService.Roles.Contains(nameof(RoleType.VicePrincipal)) &&
        _currentUserService.Roles.Contains(nameof(RoleType.Student));

    private bool IsAdminRole() =>
        _currentUserService.Roles.Contains(nameof(RoleType.SuperAdmin)) ||
        _currentUserService.Roles.Contains(nameof(RoleType.SchoolAdmin)) ||
        _currentUserService.Roles.Contains(nameof(RoleType.Principal)) ||
        _currentUserService.Roles.Contains(nameof(RoleType.VicePrincipal));

    private bool IsTeacherOnly() =>
        !IsAdminRole() &&
        (_currentUserService.Roles.Contains(nameof(RoleType.Teacher)) ||
         _currentUserService.Roles.Contains(nameof(RoleType.ClassTeacher)));

    public async Task<ApiResponse<PagedResult<HomeworkDto>>> GetHomeworkAsync(
        PaginationQuery query, Guid? classRoomId, Guid? sectionId, Guid? subjectId,
        DateOnly? fromDate, DateOnly? toDate)
    {
        try
        {
            var repo = _unitOfWork.Repository<Domain.Entities.Homework.Homework>();

            Expression<Func<Domain.Entities.Homework.Homework, bool>> filter =
                h => !h.IsDeleted &&
                      (!classRoomId.HasValue || h.ClassRoomId == classRoomId.Value) &&
                      (!sectionId.HasValue || h.SectionId == sectionId.Value) &&
                      (!subjectId.HasValue || h.SubjectId == subjectId.Value) &&
                      (!fromDate.HasValue || h.AssignedDate >= fromDate.Value.ToDateTime(TimeOnly.MinValue)) &&
                      (!toDate.HasValue || h.AssignedDate <= toDate.Value.ToDateTime(TimeOnly.MinValue));

            if (IsStudentRole())
            {
                var (studentClassRoomId, studentSectionId) = await ResolveStudentScopeAsync();
                if (studentClassRoomId == Guid.Empty)
                    filter = h => false;
                else
                    filter = h => !h.IsDeleted &&
                                  h.ClassRoomId == studentClassRoomId &&
                                  (!h.SectionId.HasValue || h.SectionId == studentSectionId);
            }
            else if (IsTeacherOnly() && !string.IsNullOrEmpty(_currentUserService.UserId))
            {
                var teacherId = await ResolveTeacherIdAsync();
                filter = h => !h.IsDeleted && h.TeacherId == teacherId;
            }

            var (items, totalCount) = await repo.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                filter: filter,
                orderBy: q => q.OrderByDescending(h => h.AssignedDate),
                include: q => q.Include(h => h.ClassRoom)
                               .Include(h => h.Section)
                               .Include(h => h.Subject)
                               .Include(h => h.Teacher)
                               .ThenInclude(t => t.User)
                               .Include(h => h.Submissions));

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
                    IsActive = !h.IsDeleted,
                    ApprovalStatus = (h.ApprovalStatus == 0 ? ApprovalStatus.Pending : h.ApprovalStatus).ToString(),
                    ApprovedBy = h.ApprovedBy,
                    ApprovedAt = h.ApprovedAt,
                    RejectionReason = h.RejectionReason,
                    SubmissionCount = h.Submissions.Count(s => !s.IsDeleted),
                    Status = h.Submissions.Any(s => !s.IsDeleted)
                        ? nameof(SchoolCRM.Domain.Enums.HomeworkStatus.Completed)
                        : nameof(SchoolCRM.Domain.Enums.HomeworkStatus.Assigned)
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
                               .ThenInclude(t => t.User)
                               .Include(h => h.Submissions)
                               .ThenInclude(s => s.Student)
                               .ThenInclude(st => st.User));

            var hw = items.FirstOrDefault();
            if (hw is null)
                return ApiResponse<HomeworkDto>.NotFoundResponse(ApplicationMessages.NotFound);

            if (IsStudentRole())
            {
                var (studentClassRoomId, studentSectionId) = await ResolveStudentScopeAsync();
                var visibleToStudent =
                    studentClassRoomId != Guid.Empty &&
                    hw.ClassRoomId == studentClassRoomId &&
                    (!hw.SectionId.HasValue || hw.SectionId == studentSectionId);
                if (!visibleToStudent)
                    return ApiResponse<HomeworkDto>.NotFoundResponse(ApplicationMessages.NotFound);
            }
            else if (IsTeacherOnly() && hw.TeacherId != await ResolveTeacherIdAsync())
            {
                return ApiResponse<HomeworkDto>.NotFoundResponse(ApplicationMessages.NotFound);
            }

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
                IsActive = !hw.IsDeleted,
                ApprovalStatus = (hw.ApprovalStatus == 0 ? ApprovalStatus.Pending : hw.ApprovalStatus).ToString(),
                ApprovedBy = hw.ApprovedBy,
                ApprovedAt = hw.ApprovedAt,
                RejectionReason = hw.RejectionReason,
                SubmissionCount = hw.Submissions.Count(s => !s.IsDeleted),
                Status = hw.Submissions.Any(s => !s.IsDeleted)
                    ? nameof(SchoolCRM.Domain.Enums.HomeworkStatus.Completed)
                    : nameof(SchoolCRM.Domain.Enums.HomeworkStatus.Assigned),
                Submissions = hw.Submissions
                    .Where(s => !s.IsDeleted)
                    .OrderByDescending(s => s.SubmittedAt)
                    .Select(s => new AssignmentDto
                    {
                        Id = s.Id,
                        HomeworkId = s.HomeworkId,
                        HomeworkTitle = hw.Title,
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
                    }).ToList()
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

            await NotifyAndEmailStudentsOnHomeworkCreatedAsync(hw);

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

            var effectiveStudentId = studentId;
            if (!effectiveStudentId.HasValue &&
                (_currentUserService.Roles?.Contains("Student") == true))
            {
                var selfId = await ResolveStudentIdAsync();
                if (selfId != Guid.Empty)
                    effectiveStudentId = selfId;
            }

            if (effectiveStudentId.HasValue)
                filtered = filtered.Where(s => s.StudentId == effectiveStudentId.Value).ToList();

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
            var studentId = dto.StudentId == Guid.Empty
                ? await ResolveStudentIdAsync()
                : dto.StudentId;
            if (studentId == Guid.Empty)
                return ApiResponse<AssignmentDto>.FailResponse("Unable to identify the current student.");

            var existing = (await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>()
                .FindAsync(s => s.HomeworkId == dto.HomeworkId && s.StudentId == studentId && !s.IsDeleted))
                .FirstOrDefault();

            var homework = (await _unitOfWork.Repository<Domain.Entities.Homework.Homework>()
                .FindAsync(h => h.Id == dto.HomeworkId && !h.IsDeleted))
                .FirstOrDefault();
            if (homework is null)
                return ApiResponse<AssignmentDto>.NotFoundResponse(ApplicationMessages.NotFound);

            Domain.Entities.Homework.HomeworkSubmission submission;

            if (existing is not null && existing.Status != HomeworkStatus.Rejected)
                return ApiResponse<AssignmentDto>.FailResponse("Assignment already submitted.");

            if (existing is not null)
            {
                existing.SubmittedText = dto.SubmissionText;
                existing.AttachmentUrl = dto.AttachmentUrl;
                existing.SubmittedAt = DateTime.UtcNow;
                existing.Status = HomeworkStatus.Submitted;
                existing.MarksObtained = null;
                existing.TeacherRemarks = null;
                existing.ReviewedAt = null;
                existing.UpdatedAt = DateTime.UtcNow;
                submission = existing;
                await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>().UpdateAsync(submission);
            }
            else
            {
                submission = new Domain.Entities.Homework.HomeworkSubmission
                {
                    HomeworkId = dto.HomeworkId,
                    StudentId = studentId,
                    SubmittedText = dto.SubmissionText,
                    AttachmentUrl = dto.AttachmentUrl,
                    SubmittedAt = DateTime.UtcNow,
                    Status = HomeworkStatus.Submitted,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>().AddAsync(submission);
            }

            await _unitOfWork.SaveChangesAsync();

            await NotifyTeacherAndAdminsOnSubmissionAsync(homework, studentId);

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

            var gradedStudent = await _unitOfWork.Students.GetByIdAsync(submission.StudentId);
            if (gradedStudent is not null)
            {
                await _notificationService.NotifyUsersAsync(
                    new[] { gradedStudent.UserId },
                    "Homework reviewed",
                    $"Your homework was marked as correct. Marks: {dto.Marks}{(string.IsNullOrWhiteSpace(dto.Remarks) ? "" : $". Remarks: {dto.Remarks}")}.",
                    NotificationType.Success,
                    link: $"/homework/{submission.HomeworkId}");
            }

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

    public async Task<ApiResponse<AssignmentDto>> RejectAssignmentAsync(Guid assignmentId, string? remarks)
    {
        try
        {
            var submission = await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>().GetByIdAsync(assignmentId);
            if (submission is null)
                return ApiResponse<AssignmentDto>.NotFoundResponse(ApplicationMessages.NotFound);

            submission.Status = HomeworkStatus.Rejected;
            submission.TeacherRemarks = remarks;
            submission.ReviewedAt = DateTime.UtcNow;
            submission.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Domain.Entities.Homework.HomeworkSubmission>().UpdateAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            var rejectedStudent = await _unitOfWork.Students.GetByIdAsync(submission.StudentId);
            if (rejectedStudent is not null)
            {
                await _notificationService.NotifyUsersAsync(
                    new[] { rejectedStudent.UserId },
                    "Homework returned",
                    $"Your homework was marked as not correct. {(string.IsNullOrWhiteSpace(remarks) ? "" : $"Reason: {remarks}. ")}Please resubmit.",
                    NotificationType.Warning,
                    link: $"/homework/{submission.HomeworkId}");
            }

            return ApiResponse<AssignmentDto>.SuccessResponse(new AssignmentDto
            {
                Id = submission.Id,
                HomeworkId = submission.HomeworkId,
                StudentId = submission.StudentId,
                Marks = submission.MarksObtained,
                Remarks = submission.TeacherRemarks,
                Status = submission.Status.ToString(),
                GradedDate = submission.ReviewedAt
            }, "Homework returned to student for resubmission.");
        }
        catch (Exception ex)
        {
            return ApiResponse<AssignmentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<HomeworkDto>> ApproveHomeworkAsync(Guid id, bool approved, string? reason)
    {
        try
        {
            var hw = await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().GetByIdAsync(id);
            if (hw is null)
                return ApiResponse<HomeworkDto>.NotFoundResponse(ApplicationMessages.NotFound);

            hw.ApprovalStatus = approved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
            hw.ApprovedBy = _currentUserService.FullName;
            hw.ApprovedAt = DateTime.UtcNow;
            hw.RejectionReason = approved ? null : reason;
            hw.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().UpdateAsync(hw);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<HomeworkDto>.SuccessResponse(new HomeworkDto
            {
                Id = hw.Id,
                Title = hw.Title,
                ApprovalStatus = (hw.ApprovalStatus == 0 ? ApprovalStatus.Pending : hw.ApprovalStatus).ToString(),
                ApprovedBy = hw.ApprovedBy,
                ApprovedAt = hw.ApprovedAt,
                RejectionReason = hw.RejectionReason
            }, approved ? "Homework approved." : "Homework rejected.");
        }
        catch (Exception ex)
        {
            return ApiResponse<HomeworkDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> RequestHomeworkApprovalAsync(Guid id)
    {
        try
        {
            var hw = await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().GetByIdAsync(id);
            if (hw is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            hw.ApprovalStatus = ApprovalStatus.Pending;
            hw.ApprovedBy = null;
            hw.ApprovedAt = null;
            hw.RejectionReason = null;
            hw.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Domain.Entities.Homework.Homework>().UpdateAsync(hw);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Homework submitted for approval.");
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    private async Task NotifyTeacherAndAdminsOnSubmissionAsync(
        Domain.Entities.Homework.Homework homework, Guid studentId)
    {
        try
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
            var studentName = student?.User is not null
                ? $"{student.User.FirstName} {student.User.LastName}"
                : "A student";

            var recipientIds = new HashSet<Guid>();

            if (homework.TeacherId != Guid.Empty)
            {
                var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(homework.TeacherId);
                if (teacher?.User is not null)
                    recipientIds.Add(teacher.User.Id);
            }

            var title = "New homework submission";
            var message = $"{studentName} submitted homework '{homework.Title}'.";

            if (recipientIds.Count > 0)
                await _notificationService.NotifyUsersAsync(recipientIds, title, message,
                    NotificationType.Info, link: $"/homework/{homework.Id}");

            await _notificationService.NotifyAdminsAsync(title, message,
                NotificationType.Info, link: $"/homework/{homework.Id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to notify on homework submission: {ex.Message}");
        }
    }

    private async Task NotifyAndEmailStudentsOnHomeworkCreatedAsync(
        Domain.Entities.Homework.Homework homework)
    {
        try
        {
            var title = "New homework assigned";
            var message = $"New homework '{homework.Title}' has been assigned. Due date: {homework.DueDate:dd MMM yyyy}.";
            var link = $"/homework/{homework.Id}";

            await _notificationService.NotifyStudentsOfClassAsync(
                homework.ClassRoomId, title, message, NotificationType.Info, homework.SectionId, link);

            var students = await _unitOfWork.Students.GetBySectionAsync(homework.SectionId ?? Guid.Empty);
            var emails = students
                .Where(s => !s.IsDeleted && !string.IsNullOrWhiteSpace(s.ParentEmail))
                .Select(s => s.ParentEmail!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var subject = $"New homework: {homework.Title}";
            var body = $@"
                <h3>New homework assigned</h3>
                <p><strong>{homework.Title}</strong></p>
                <p>{homework.Description}</p>
                <p><strong>Due date:</strong> {homework.DueDate:dd MMM yyyy}</p>
                <p>Please ensure your child completes and submits the homework on time.</p>";

            await SendEmailsAsync(emails, subject, body);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to notify on homework created: {ex.Message}");
        }
    }

    private async Task SendEmailsAsync(IEnumerable<string> emails, string subject, string htmlBody)
    {
        foreach (var email in emails)
            await _emailService.SendEmailAsync(email, subject, htmlBody);
    }
}
