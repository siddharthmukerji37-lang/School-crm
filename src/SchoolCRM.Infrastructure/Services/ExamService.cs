using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.DTOs.Exam;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Exam;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class ExamService : IExamService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;

    private static readonly string[] AdminRoles =
        { nameof(RoleType.SuperAdmin), nameof(RoleType.SchoolAdmin), nameof(RoleType.Principal), nameof(RoleType.VicePrincipal) };

    public ExamService(
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

    public async Task<ApiResponse<PagedResult<ExamDto>>> GetExamsAsync(PaginationQuery query, Guid? classRoomId, Guid? sectionId)
    {
        try
        {
            Expression<Func<Exam, bool>>? filter = e => !e.IsDeleted;
            if (classRoomId.HasValue && sectionId.HasValue)
                filter = e => !e.IsDeleted && e.ClassRoomId == classRoomId.Value && e.SectionId == sectionId.Value;
            else if (classRoomId.HasValue)
                filter = e => !e.IsDeleted && e.ClassRoomId == classRoomId.Value;
            else if (sectionId.HasValue)
                filter = e => !e.IsDeleted && e.SectionId == sectionId.Value;

            if (IsTeacherOnly() && _currentUserService.UserId is not null)
            {
                var teacherId = await ResolveTeacherIdAsync();
                filter = e => !e.IsDeleted && e.TeacherId == teacherId;
            }
            else if (!IsAdmin() && !IsTeacherOnly() && _currentUserService.Roles.Any(r => r == nameof(RoleType.Student)))
            {
                var (studentClassRoomId, studentSectionId) = await ResolveStudentScopeAsync();
                if (studentClassRoomId == Guid.Empty)
                    filter = e => false;
                else
                    filter = e => !e.IsDeleted && e.ClassRoomId == studentClassRoomId &&
                                  (!e.SectionId.HasValue || e.SectionId == studentSectionId);
            }

            var (items, totalCount) = await _unitOfWork.Exams.GetPagedAsync(
                query.PageNumber, query.PageSize, filter,
                q => q.OrderByDescending(e => e.StartDate),
                q => q.Include(e => e.ExamType).Include(e => e.ClassRoom).Include(e => e.Section));

            var dtos = items.Select(MapToDto).ToList();

            var pagedResult = new PagedResult<ExamDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm
            };

            return ApiResponse<PagedResult<ExamDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<ExamDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExamDto>> GetExamByIdAsync(Guid id)
    {
        try
        {
            var exam = await _unitOfWork.Exams.GetExamWithDetailsAsync(id);
            if (exam is null)
                return ApiResponse<ExamDto>.NotFoundResponse(ApplicationMessages.NotFound);

            if (!IsAdmin() && !IsTeacherOnly() && _currentUserService.Roles.Any(r => r == nameof(RoleType.Student)))
            {
                var (studentClassRoomId, studentSectionId) = await ResolveStudentScopeAsync();
                var visibleToStudent =
                    studentClassRoomId != Guid.Empty &&
                    exam.ClassRoomId == studentClassRoomId &&
                    (!exam.SectionId.HasValue || exam.SectionId == studentSectionId);
                if (!visibleToStudent)
                    return ApiResponse<ExamDto>.NotFoundResponse(ApplicationMessages.NotFound);
            }
            else if (IsTeacherOnly() && exam.TeacherId != await ResolveTeacherIdAsync())
            {
                return ApiResponse<ExamDto>.NotFoundResponse(ApplicationMessages.NotFound);
            }

            return ApiResponse<ExamDto>.SuccessResponse(MapToDto(exam));
        }
        catch (Exception ex)
        {
            return ApiResponse<ExamDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExamDto>> CreateExamAsync(CreateExamDto dto)
    {
        try
        {
            var classRoom = await _unitOfWork.ClassRooms.GetByIdAsync(dto.ClassRoomId);
            if (classRoom is null)
                return ApiResponse<ExamDto>.FailResponse("Selected class not found.");

            if (dto.ExamType.Equals("Final", StringComparison.OrdinalIgnoreCase) && !IsAdmin())
                return ApiResponse<ExamDto>.FailResponse("Only administrators can create Final exams.");

            var teacherId = await ResolveTeacherIdAsync();

            Guid? examTypeId = null;
            if (!string.IsNullOrWhiteSpace(dto.ExamType))
            {
                var existingType = (await _unitOfWork.Repository<Domain.Entities.Exam.ExamType>()
                    .FindAsync(e => e.Name == dto.ExamType && e.SchoolId == classRoom.SchoolId))
                    .FirstOrDefault();

                if (existingType is null)
                {
                    var code = dto.ExamType.Length > 50
                        ? dto.ExamType[..50].ToUpperInvariant()
                        : dto.ExamType.ToUpperInvariant();
                    var newType = new Domain.Entities.Exam.ExamType
                    {
                        Name = dto.ExamType,
                        Code = code,
                        SchoolId = classRoom.SchoolId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<Domain.Entities.Exam.ExamType>().AddAsync(newType);
                    await _unitOfWork.SaveChangesAsync();
                    examTypeId = newType.Id;
                }
                else
                {
                    examTypeId = existingType.Id;
                }
            }

            var exam = new Domain.Entities.Exam.Exam
            {
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = ExamStatus.Scheduled,
                SchoolId = classRoom.SchoolId,
                ClassRoomId = dto.ClassRoomId,
                SectionId = dto.SectionId,
                ExamTypeId = examTypeId,
                TeacherId = teacherId,
                AcademicYearId = dto.AcademicYearId,
                ApprovalStatus = ApprovalStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Exams.AddAsync(exam);
            await _unitOfWork.SaveChangesAsync();

            await NotifyAndEmailStudentsOnExamCreatedAsync(exam);

            var created = await _unitOfWork.Exams.GetByIdAsync(exam.Id);
            return ApiResponse<ExamDto>.SuccessResponse(MapToDto(created!), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return ApiResponse<ExamDto>.FailResponse(message);
        }
    }

    public async Task<ApiResponse<ExamDto>> UpdateExamAsync(Guid id, CreateExamDto dto)
    {
        try
        {
            var exam = await _unitOfWork.Exams.GetByIdAsync(id);
            if (exam is null)
                return ApiResponse<ExamDto>.NotFoundResponse(ApplicationMessages.NotFound);

            var classRoom = await _unitOfWork.ClassRooms.GetByIdAsync(dto.ClassRoomId);
            if (classRoom is null)
                return ApiResponse<ExamDto>.FailResponse("Selected class not found.");

            Guid? examTypeId = exam.ExamTypeId;
            if (!string.IsNullOrWhiteSpace(dto.ExamType))
            {
                var existingType = (await _unitOfWork.Repository<Domain.Entities.Exam.ExamType>()
                    .FindAsync(e => e.Name == dto.ExamType && e.SchoolId == classRoom.SchoolId))
                    .FirstOrDefault();

                if (existingType is null)
                {
                    var newType = new Domain.Entities.Exam.ExamType
                    {
                        Name = dto.ExamType,
                        Code = dto.ExamType.ToUpperInvariant(),
                        SchoolId = classRoom.SchoolId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<Domain.Entities.Exam.ExamType>().AddAsync(newType);
                    await _unitOfWork.SaveChangesAsync();
                    examTypeId = newType.Id;
                }
                else
                {
                    examTypeId = existingType.Id;
                }
            }

            exam.Name = dto.Name;
            exam.Description = dto.Description;
            exam.StartDate = dto.StartDate;
            exam.EndDate = dto.EndDate;
            exam.ClassRoomId = dto.ClassRoomId;
            exam.SectionId = dto.SectionId;
            exam.ExamTypeId = examTypeId;
            exam.AcademicYearId = dto.AcademicYearId;
            exam.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Exams.UpdateAsync(exam);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Exams.GetByIdAsync(id);
            return ApiResponse<ExamDto>.SuccessResponse(MapToDto(updated!), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ExamDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteExamAsync(Guid id)
    {
        try
        {
            var exam = await _unitOfWork.Exams.GetByIdAsync(id);
            if (exam is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            exam.IsDeleted = true;
            exam.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Exams.UpdateAsync(exam);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<ExamScheduleDto>>> GetExamScheduleAsync(Guid examId)
    {
        try
        {
            var schedules = await _unitOfWork.ExamSchedules.GetByExamAsync(examId);
            var dtos = schedules.Select(s => new ExamScheduleDto
            {
                Id = s.Id,
                ExamId = s.ExamId,
                SubjectId = s.SubjectId,
                SubjectName = s.Subject?.Name ?? string.Empty,
                ExamDate = s.ExamDate,
                StartTime = TimeOnly.FromTimeSpan(s.StartTime),
                EndTime = TimeOnly.FromTimeSpan(s.EndTime),
                Room = s.HallName,
                MaxMarks = s.MaxMarks,
                PassingMarks = s.PassMarks,
                Instructions = s.Instructions
            }).ToList();

            return ApiResponse<List<ExamScheduleDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ExamScheduleDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> UpdateExamScheduleAsync(Guid examId, List<ExamScheduleDto> scheduleDtos)
    {
        try
        {
            var existing = await _unitOfWork.ExamSchedules.GetByExamAsync(examId);
            foreach (var s in existing)
                await _unitOfWork.ExamSchedules.DeleteAsync(s);

            foreach (var dto in scheduleDtos)
            {
                var schedule = new ExamSchedule
                {
                    ExamId = examId,
                    SubjectId = dto.SubjectId,
                    ExamDate = dto.ExamDate,
                    StartTime = dto.StartTime.ToTimeSpan(),
                    EndTime = dto.EndTime.ToTimeSpan(),
                    MaxMarks = dto.MaxMarks ?? 0,
                    PassMarks = dto.PassingMarks ?? 0,
                    HallName = dto.Room,
                    Instructions = dto.Instructions,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.ExamSchedules.AddAsync(schedule);
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse(ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<MarkDto>>> GetMarksAsync(Guid examId, Guid? sectionId, Guid? subjectId)
    {
        try
        {
            var schedules = await _unitOfWork.ExamSchedules.GetByExamAsync(examId);
            var scheduleIds = schedules.Where(s => !subjectId.HasValue || s.SubjectId == subjectId.Value)
                .Select(s => s.Id).ToList();

            var allMarks = new List<Mark>();
            foreach (var scheduleId in scheduleIds)
            {
                var marks = await _unitOfWork.Marks.GetByExamScheduleAsync(scheduleId);
                allMarks.AddRange(marks);
            }

            var dtos = allMarks.Select(m => new MarkDto
            {
                Id = m.Id,
                ExamId = m.ExamSchedule.ExamId,
                ExamName = m.ExamSchedule.Exam?.Name ?? string.Empty,
                StudentId = m.StudentId,
                StudentName = $"{m.Student?.User?.FirstName} {m.Student?.User?.LastName}",
                AdmissionNumber = m.Student?.AdmissionNumber ?? string.Empty,
                SubjectId = m.ExamSchedule.SubjectId,
                SubjectName = m.ExamSchedule.Subject?.Name ?? string.Empty,
                MarksObtained = m.MarksObtained,
                MaxMarks = m.ExamSchedule.MaxMarks,
                IsPass = m.MarksObtained >= m.ExamSchedule.PassMarks,
                Remarks = m.Remarks,
                GradedBy = m.EnteredBy,
                GradedDate = m.EnteredAt
            }).ToList();

            return ApiResponse<List<MarkDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<MarkDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> EnterMarksAsync(EnterMarksDto dto)
    {
        try
        {
            var schedule = (await _unitOfWork.ExamSchedules.GetByExamAsync(dto.ExamId))
                .FirstOrDefault(s => s.SubjectId == dto.SubjectId);

            if (schedule is null)
                return ApiResponse.FailResponse("Exam schedule not found for the specified subject.");

            foreach (var entry in dto.Marks)
            {
                var existing = await _unitOfWork.Marks.GetByStudentAndScheduleAsync(entry.StudentId, schedule.Id);
                if (existing is not null)
                {
                    existing.MarksObtained = entry.MarksObtained;
                    existing.Remarks = entry.Remarks;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Marks.UpdateAsync(existing);
                }
                else
                {
                    var mark = new Mark
                    {
                        ExamScheduleId = schedule.Id,
                        StudentId = entry.StudentId,
                        MarksObtained = entry.MarksObtained,
                        Remarks = entry.Remarks,
                        IsPublished = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Marks.AddAsync(mark);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse(ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ResultDto>> GetStudentResultAsync(Guid studentId, Guid examId)
    {
        try
        {
            var marks = await _unitOfWork.Marks.GetByStudentAsync(studentId, examId);
            if (!marks.Any())
                return ApiResponse<ResultDto>.NotFoundResponse("No marks found for this student.");

            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);

            var subjectResults = marks.Select(m => new SubjectResultDto
            {
                SubjectId = m.ExamSchedule.SubjectId,
                SubjectName = m.ExamSchedule.Subject?.Name ?? string.Empty,
                MarksObtained = m.MarksObtained,
                MaxMarks = m.ExamSchedule.MaxMarks,
                PassingMarks = m.ExamSchedule.PassMarks,
                IsPass = m.MarksObtained >= m.ExamSchedule.PassMarks,
                Remarks = m.Remarks
            }).ToList();

            var totalObtained = subjectResults.Sum(s => s.MarksObtained);
            var totalMax = subjectResults.Sum(s => s.MaxMarks);

            var result = new ResultDto
            {
                StudentId = studentId,
                StudentName = student is not null ? $"{student.User.FirstName} {student.User.LastName}" : string.Empty,
                AdmissionNumber = student?.AdmissionNumber ?? string.Empty,
                ClassName = student?.Section?.ClassRoom?.Name ?? string.Empty,
                SectionName = student?.Section?.Name ?? string.Empty,
                ExamId = examId,
                ExamName = marks.First().ExamSchedule.Exam?.Name ?? string.Empty,
                SubjectResults = subjectResults,
                TotalMarksObtained = totalObtained,
                TotalMaxMarks = totalMax,
                Percentage = totalMax > 0 ? Math.Round(totalObtained / totalMax * 100, 2) : 0,
                IsPassed = subjectResults.All(s => s.IsPass)
            };

            return ApiResponse<ResultDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<ResultDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<ResultDto>>> GetStudentResultsAsync(Guid studentId)
    {
        try
        {
            var allMarks = await _unitOfWork.Marks.GetByStudentAllAsync(studentId);
            var examIds = allMarks
                .Where(m => m.ExamSchedule?.ExamId != Guid.Empty)
                .Select(m => m.ExamSchedule!.ExamId)
                .Distinct()
                .ToList();

            var results = new List<ResultDto>();
            foreach (var examId in examIds)
            {
                var resultResponse = await GetStudentResultAsync(studentId, examId);
                if (resultResponse.Data is not null)
                    results.Add(resultResponse.Data);
            }

            return ApiResponse<List<ResultDto>>.SuccessResponse(results);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ResultDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<ResultDto>>> GetResultsAsync(
        PaginationQuery query, Guid examId, Guid? classRoomId, Guid? sectionId)
    {
        try
        {
            var exam = await _unitOfWork.Exams.GetByIdAsync(examId);
            if (exam is null)
                return ApiResponse<PagedResult<ResultDto>>.NotFoundResponse(ApplicationMessages.NotFound);

            var allMarks = await _unitOfWork.Marks.GetByExamScheduleAsync(Guid.Empty);
            var studentIds = allMarks.Where(m => m.ExamSchedule.ExamId == examId)
                .Select(m => m.StudentId).Distinct().ToList();

            var results = new List<ResultDto>();
            foreach (var studentId in studentIds)
            {
                var studentResult = await GetStudentResultAsync(studentId, examId);
                if (studentResult.Data is not null)
                    results.Add(studentResult.Data);
            }

            var pagedItems = results.Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize).ToList();

            var pagedResult = new PagedResult<ResultDto>
            {
                Items = pagedItems,
                TotalCount = results.Count,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return ApiResponse<PagedResult<ResultDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<ResultDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ReportCardDto>> GenerateReportCardAsync(Guid studentId, Guid examId)
    {
        try
        {
            var resultResponse = await GetStudentResultAsync(studentId, examId);
            if (resultResponse.Data is null)
                return ApiResponse<ReportCardDto>.FailResponse(resultResponse.Message);

            var result = resultResponse.Data;
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);

            var reportCard = new ReportCardDto
            {
                StudentId = studentId,
                StudentName = result.StudentName,
                AdmissionNumber = result.AdmissionNumber,
                ClassName = result.ClassName,
                SectionName = result.SectionName,
                RollNumber = student?.RollNumber ?? string.Empty,
                SchoolName = student?.School?.Name ?? string.Empty,
                ExamId = examId,
                ExamName = result.ExamName,
                SubjectResults = result.SubjectResults,
                TotalMarksObtained = result.TotalMarksObtained,
                TotalMaxMarks = result.TotalMaxMarks,
                Percentage = result.Percentage,
                Grade = result.Percentage >= 90 ? "A+" :
                        result.Percentage >= 80 ? "A" :
                        result.Percentage >= 70 ? "B+" :
                        result.Percentage >= 60 ? "B" :
                        result.Percentage >= 50 ? "C" :
                        result.Percentage >= 40 ? "D" : "F",
                IsPassed = result.IsPassed
            };

            return ApiResponse<ReportCardDto>.SuccessResponse(reportCard);
        }
        catch (Exception ex)
        {
            return ApiResponse<ReportCardDto>.FailResponse(ex.Message);
        }
    }

    private bool IsAdmin() => _currentUserService.Roles.Any(r => AdminRoles.Contains(r));

    private bool IsTeacherOnly() =>
        !IsAdmin() && _currentUserService.Roles.Any(r => r == nameof(RoleType.Teacher) || r == nameof(RoleType.ClassTeacher));

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

    private static async Task<bool> IsFinalExamAsync(IUnitOfWork unitOfWork, Guid examId)
    {
        var exam = await unitOfWork.Exams.GetByIdAsync(examId);
        return exam?.ExamType?.Name?.Equals("Final", StringComparison.OrdinalIgnoreCase) == true;
    }

    private async Task<Exam?> GetExamOrNullAsync(Guid examId) =>
        await _unitOfWork.Exams.GetExamWithDetailsAsync(examId);

    private static ExamQuestionDto MapQuestionToDto(ExamQuestion q) => new()
    {
        Id = q.Id,
        QuestionText = q.QuestionText,
        QuestionType = q.QuestionType.ToString(),
        OptionA = q.OptionA,
        OptionB = q.OptionB,
        OptionC = q.OptionC,
        OptionD = q.OptionD,
        CorrectAnswer = q.CorrectAnswer,
        Marks = q.Marks,
        ImageUrl = q.ImageUrl,
        ImageFileName = q.ImageFileName,
        OrderIndex = q.OrderIndex,
        SubjectId = q.SubjectId,
        SubjectName = q.Subject?.Name ?? string.Empty
    };

    private static ExamAnswerDto MapAnswerToDto(ExamAnswer a, ExamQuestion q, bool maskMarks = false)
    {
        var dto = new ExamAnswerDto
        {
            Id = a.Id,
            ExamQuestionId = a.ExamQuestionId,
            QuestionText = q.QuestionText,
            QuestionType = q.QuestionType.ToString(),
            SelectedOption = a.SelectedOption,
            AnswerText = a.AnswerText,
            ImageUrl = a.ImageUrl,
            CorrectAnswer = q.CorrectAnswer,
            OptionA = q.OptionA,
            OptionB = q.OptionB,
            OptionC = q.OptionC,
            OptionD = q.OptionD,
            Marks = q.Marks,
            IsCorrect = a.IsCorrect,
            MarksObtained = a.MarksObtained,
            Remarks = a.Remarks,
            OrderIndex = q.OrderIndex
        };

        if (maskMarks)
        {
            dto.CorrectAnswer = null;
            dto.IsCorrect = null;
            dto.MarksObtained = null;
            dto.Remarks = null;
        }

        return dto;
    }

    private static async Task<ExamSubmissionDto> MapSubmissionToDtoAsync(
        IUnitOfWork unitOfWork, ExamSubmission s, bool maskMarks = false)
    {
        var answers = await unitOfWork.ExamAnswers.GetBySubmissionAsync(s.Id);
        var gradingStatus = s.GradingStatus == 0 ? GradingStatus.Pending : s.GradingStatus;
        var mask = maskMarks && gradingStatus != GradingStatus.Approved;
        var dto = new ExamSubmissionDto
        {
            Id = s.Id,
            ExamId = s.ExamId,
            ExamName = s.Exam?.Name ?? string.Empty,
            StudentId = s.StudentId,
            StudentName = s.Student?.User is not null
                ? $"{s.Student.User.FirstName} {s.Student.User.LastName}"
                : string.Empty,
            AdmissionNumber = s.Student?.AdmissionNumber ?? string.Empty,
            SubmittedAt = s.SubmittedAt,
            TotalMarksObtained = s.TotalMarksObtained,
            TotalMaxMarks = s.TotalMaxMarks,
            IsGraded = s.IsGraded,
            GradedBy = s.GradedBy,
            GradedAt = s.GradedAt,
            GradingStatus = gradingStatus.ToString(),
            GradingApprovedBy = s.GradingApprovedBy,
            GradingApprovedAt = s.GradingApprovedAt,
            GradingRejectionReason = s.GradingRejectionReason,
            Answers = answers.Select(a => MapAnswerToDto(a, a.ExamQuestion, mask)).ToList()
        };

        if (mask)
            dto.TotalMarksObtained = null;

        return dto;
    }

    public async Task<ApiResponse<List<ExamQuestionDto>>> GetExamQuestionsAsync(Guid examId)
    {
        try
        {
            var questions = await _unitOfWork.ExamQuestions.GetByExamAsync(examId);
            var dtos = questions.Select(MapQuestionToDto).ToList();
            return ApiResponse<List<ExamQuestionDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ExamQuestionDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> AddExamQuestionsAsync(Guid examId, List<CreateExamQuestionDto> dtos)
    {
        try
        {
            var exam = await GetExamOrNullAsync(examId);
            if (exam is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            var isFinal = exam.ExamType?.Name?.Equals("Final", StringComparison.OrdinalIgnoreCase) == true;
            if (isFinal && !IsAdmin())
                return ApiResponse.FailResponse("Only administrators can add questions to Final exams.");

            if (!isFinal && IsTeacherOnly() && exam.TeacherId != await ResolveTeacherIdAsync())
                return ApiResponse.FailResponse("You can only manage questions for your own exams.");

            var existingMaxOrder = exam.Questions?.Any() == true
                ? exam.Questions.Max(q => q.OrderIndex)
                : 0;

            foreach (var dto in dtos)
            {
                var isMcq = dto.QuestionType.Equals(nameof(QuestionType.MCQ), StringComparison.OrdinalIgnoreCase);
                if (isMcq)
                {
                    if (string.IsNullOrWhiteSpace(dto.OptionA) || string.IsNullOrWhiteSpace(dto.OptionB)
                        || string.IsNullOrWhiteSpace(dto.CorrectAnswer))
                        return ApiResponse.FailResponse("MCQ questions require at least options A, B and a correct answer.");
                }
                else if (!isMcq && string.IsNullOrWhiteSpace(dto.QuestionText) && string.IsNullOrWhiteSpace(dto.ImageUrl))
                {
                    return ApiResponse.FailResponse("Descriptive questions require either question text or an image.");
                }

                var question = new ExamQuestion
                {
                    ExamId = examId,
                    QuestionText = dto.QuestionText,
                    QuestionType = isMcq ? QuestionType.MCQ : QuestionType.Descriptive,
                    OptionA = dto.OptionA,
                    OptionB = dto.OptionB,
                    OptionC = dto.OptionC,
                    OptionD = dto.OptionD,
                    CorrectAnswer = isMcq ? dto.CorrectAnswer?.ToUpperInvariant() : null,
                    Marks = dto.Marks,
                    ImageUrl = dto.ImageUrl,
                    ImageFileName = dto.ImageFileName,
                    OrderIndex = dto.OrderIndex == 0 ? ++existingMaxOrder : dto.OrderIndex,
                    SubjectId = dto.SubjectId,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.ExamQuestions.AddAsync(question);
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse(ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExamQuestionDto>> UpdateExamQuestionAsync(Guid examId, Guid questionId, CreateExamQuestionDto dto)
    {
        try
        {
            var exam = await GetExamOrNullAsync(examId);
            if (exam is null)
                return ApiResponse<ExamQuestionDto>.FailResponse(ApplicationMessages.NotFound);

            var isFinal = exam.ExamType?.Name?.Equals("Final", StringComparison.OrdinalIgnoreCase) == true;
            if (isFinal && !IsAdmin())
                return ApiResponse<ExamQuestionDto>.FailResponse("Only administrators can manage Final exam questions.");

            var question = (await _unitOfWork.ExamQuestions.GetByExamAsync(examId))
                .FirstOrDefault(q => q.Id == questionId);
            if (question is null)
                return ApiResponse<ExamQuestionDto>.NotFoundResponse(ApplicationMessages.NotFound);

            var isMcq = dto.QuestionType.Equals(nameof(QuestionType.MCQ), StringComparison.OrdinalIgnoreCase);
            question.QuestionText = dto.QuestionText;
            question.QuestionType = isMcq ? QuestionType.MCQ : QuestionType.Descriptive;
            question.OptionA = dto.OptionA;
            question.OptionB = dto.OptionB;
            question.OptionC = dto.OptionC;
            question.OptionD = dto.OptionD;
            question.CorrectAnswer = isMcq ? dto.CorrectAnswer?.ToUpperInvariant() : null;
            question.Marks = dto.Marks;
            question.ImageUrl = dto.ImageUrl;
            question.ImageFileName = dto.ImageFileName;
            question.SubjectId = dto.SubjectId;
            question.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ExamQuestions.UpdateAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ExamQuestionDto>.SuccessResponse(MapQuestionToDto(question), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ExamQuestionDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteExamQuestionAsync(Guid examId, Guid questionId)
    {
        try
        {
            var exam = await GetExamOrNullAsync(examId);
            if (exam is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            var isFinal = exam.ExamType?.Name?.Equals("Final", StringComparison.OrdinalIgnoreCase) == true;
            if (isFinal && !IsAdmin())
                return ApiResponse.FailResponse("Only administrators can manage Final exam questions.");

            var question = (await _unitOfWork.ExamQuestions.GetByExamAsync(examId))
                .FirstOrDefault(q => q.Id == questionId);
            if (question is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            await _unitOfWork.ExamQuestions.DeleteAsync(question);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExamDto>> ApproveExamAsync(Guid id, bool approved, string? reason)
    {
        try
        {
            if (!IsAdmin())
                return ApiResponse<ExamDto>.FailResponse("Only administrators can approve exams.");

            var exam = await GetExamOrNullAsync(id);
            if (exam is null)
                return ApiResponse<ExamDto>.NotFoundResponse(ApplicationMessages.NotFound);

            exam.ApprovalStatus = approved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
            exam.ApprovedBy = _currentUserService.FullName;
            exam.ApprovedAt = DateTime.UtcNow;
            exam.RejectionReason = approved ? null : reason;
            exam.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Exams.UpdateAsync(exam);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ExamDto>.SuccessResponse(MapToDto(exam), approved ? "Exam approved." : "Exam rejected.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ExamDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExamDto>> UploadQuestionPaperAsync(Guid examId, string? fileUrl, string? fileName)
    {
        try
        {
            var exam = await GetExamOrNullAsync(examId);
            if (exam is null)
                return ApiResponse<ExamDto>.NotFoundResponse(ApplicationMessages.NotFound);

            exam.QuestionPaperUrl = fileUrl;
            exam.QuestionPaperFileName = fileName;
            exam.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Exams.UpdateAsync(exam);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ExamDto>.SuccessResponse(MapToDto(exam), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ExamDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExamSubmissionDto>> GetSubmissionAsync(Guid examId, Guid studentId)
    {
        try
        {
            var submission = await _unitOfWork.ExamSubmissions.GetByExamAndStudentAsync(examId, studentId);
            if (submission is null)
                return ApiResponse<ExamSubmissionDto>.NotFoundResponse("No submission found for this student.");

            var dto = await MapSubmissionToDtoAsync(_unitOfWork, submission, maskMarks: true);
            return ApiResponse<ExamSubmissionDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ExamSubmissionDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExamSubmissionDto>> SubmitExamAsync(SubmitExamDto dto)
    {
        try
        {
            var exam = await GetExamOrNullAsync(dto.ExamId);
            if (exam is null)
                return ApiResponse<ExamSubmissionDto>.NotFoundResponse(ApplicationMessages.NotFound);

            if (exam.ApprovalStatus != ApprovalStatus.Approved)
                return ApiResponse<ExamSubmissionDto>.FailResponse("This exam is not approved yet and cannot be attempted.");

            if (DateTime.UtcNow < exam.StartDate.ToUniversalTime())
                return ApiResponse<ExamSubmissionDto>.FailResponse("This exam has not started yet.");

            var studentId = await ResolveStudentIdAsync();
            if (studentId == Guid.Empty)
                return ApiResponse<ExamSubmissionDto>.FailResponse("Unable to identify the current student.");

            if (await _unitOfWork.FeeInstallments.HasOutstandingFeesAsync(studentId))
                return ApiResponse<ExamSubmissionDto>.FailResponse(
                    "You have pending fees. Please clear your fee dues before accessing exams.");

            var existing = await _unitOfWork.ExamSubmissions.GetByExamAndStudentAsync(dto.ExamId, studentId);
            if (existing is not null)
                return ApiResponse<ExamSubmissionDto>.FailResponse("You have already submitted this exam.");

            var questions = (await _unitOfWork.ExamQuestions.GetByExamAsync(dto.ExamId))
                .OrderBy(q => q.OrderIndex).ToList();
            if (!questions.Any())
                return ApiResponse<ExamSubmissionDto>.FailResponse("This exam has no questions yet.");

            var submission = new ExamSubmission
            {
                ExamId = dto.ExamId,
                StudentId = studentId,
                SubmittedAt = DateTime.UtcNow,
                TotalMarksObtained = 0,
                TotalMaxMarks = questions.Sum(q => q.Marks),
                IsGraded = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.ExamSubmissions.AddAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            foreach (var question in questions)
            {
                var answer = dto.Answers.FirstOrDefault(a => a.ExamQuestionId == question.Id);
                var isMcq = question.QuestionType == QuestionType.MCQ;
                var selected = answer?.SelectedOption?.ToUpperInvariant();
                var isCorrect = isMcq && !string.IsNullOrWhiteSpace(selected)
                    && selected == question.CorrectAnswer?.ToUpperInvariant();

                var examAnswer = new ExamAnswer
                {
                    ExamSubmissionId = submission.Id,
                    ExamQuestionId = question.Id,
                    SelectedOption = isMcq ? selected : null,
                    AnswerText = isMcq ? null : answer?.AnswerText,
                    ImageUrl = isMcq ? null : answer?.ImageUrl,
                    IsCorrect = isCorrect,
                    MarksObtained = isCorrect ? question.Marks : 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.ExamAnswers.AddAsync(examAnswer);
            }

            var autoGradedMarks = 0m;
            foreach (var question in questions)
            {
                var answer = dto.Answers.FirstOrDefault(a => a.ExamQuestionId == question.Id);
                if (question.QuestionType == QuestionType.MCQ
                    && !string.IsNullOrWhiteSpace(answer?.SelectedOption)
                    && answer.SelectedOption.ToUpperInvariant() == question.CorrectAnswer?.ToUpperInvariant())
                {
                    autoGradedMarks += question.Marks;
                }
            }
            submission.TotalMarksObtained = autoGradedMarks;
            submission.IsGraded = questions.All(q => q.QuestionType == QuestionType.MCQ);
            submission.GradedBy = null;
            submission.GradedAt = null;
            submission.GradingStatus = GradingStatus.Pending;
            submission.GradingApprovedBy = null;
            submission.GradingApprovedAt = null;
            submission.GradingRejectionReason = null;

            await _unitOfWork.SaveChangesAsync();

            var result = await MapSubmissionToDtoAsync(_unitOfWork, submission);
            return ApiResponse<ExamSubmissionDto>.SuccessResponse(result, "Exam submitted successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ExamSubmissionDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<ExamSubmissionDto>>> GetSubmissionsByExamAsync(Guid examId)
    {
        try
        {
            var submissions = await _unitOfWork.ExamSubmissions.GetByExamAsync(examId);
            var dtos = new List<ExamSubmissionDto>();
            foreach (var s in submissions)
                dtos.Add(await MapSubmissionToDtoAsync(_unitOfWork, s));

            return ApiResponse<List<ExamSubmissionDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ExamSubmissionDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<ExamSubmissionDto>>> GetMySubmissionsAsync()
    {
        try
        {
            var studentId = await ResolveStudentIdAsync();
            if (studentId == Guid.Empty)
                return ApiResponse<List<ExamSubmissionDto>>.FailResponse("Unable to identify the current student.");

            var submissions = await _unitOfWork.ExamSubmissions.GetByStudentAsync(studentId);
            var dtos = new List<ExamSubmissionDto>();
            foreach (var s in submissions)
                dtos.Add(await MapSubmissionToDtoAsync(_unitOfWork, s, maskMarks: true));

            return ApiResponse<List<ExamSubmissionDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<ExamSubmissionDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExamSubmissionDto>> GradeSubmissionAsync(Guid submissionId, GradeSubmissionDto dto)
    {
        try
        {
            var target = await _unitOfWork.ExamSubmissions.GetByIdAsync(submissionId);
            if (target is null)
                return ApiResponse<ExamSubmissionDto>.NotFoundResponse(ApplicationMessages.NotFound);

            var answers = await _unitOfWork.ExamAnswers.GetBySubmissionAsync(target.Id);
            var gradeMap = dto.Answers.ToDictionary(a => a.AnswerId);

            foreach (var answer in answers)
            {
                if (answer.ExamQuestion.QuestionType == QuestionType.MCQ)
                    continue;

                if (gradeMap.TryGetValue(answer.Id, out var grade))
                {
                    var max = answer.ExamQuestion.Marks;
                    answer.MarksObtained = Math.Min(grade.MarksObtained, max);
                    answer.Remarks = grade.Remarks;
                    answer.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.ExamAnswers.UpdateAsync(answer);
                }
            }

            var gradedAnswers = await _unitOfWork.ExamAnswers.GetBySubmissionAsync(target.Id);
            target.TotalMarksObtained = gradedAnswers.Sum(a => a.MarksObtained);
            target.IsGraded = true;
            target.GradedBy = _currentUserService.FullName;
            target.GradedAt = DateTime.UtcNow;
            target.GradingStatus = GradingStatus.Pending;
            target.GradingApprovedBy = null;
            target.GradingApprovedAt = null;
            target.GradingRejectionReason = null;
            target.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ExamSubmissions.UpdateAsync(target);
            await _unitOfWork.SaveChangesAsync();

            var result = await MapSubmissionToDtoAsync(_unitOfWork, target);
            return ApiResponse<ExamSubmissionDto>.SuccessResponse(result, "Submission graded successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<ExamSubmissionDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExamSubmissionDto>> ApproveSubmissionGradingAsync(
        Guid submissionId, bool approved, string? reason)
    {
        try
        {
            if (!IsAdmin())
                return ApiResponse<ExamSubmissionDto>.FailResponse("Only administrators can approve exam results.");

            var target = await _unitOfWork.ExamSubmissions.GetByIdAsync(submissionId);
            if (target is null)
                return ApiResponse<ExamSubmissionDto>.NotFoundResponse(ApplicationMessages.NotFound);

            target.GradingStatus = approved ? GradingStatus.Approved : GradingStatus.Rejected;
            target.GradingApprovedBy = _currentUserService.FullName;
            target.GradingApprovedAt = DateTime.UtcNow;
            target.GradingRejectionReason = approved ? null : reason;
            target.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ExamSubmissions.UpdateAsync(target);
            await _unitOfWork.SaveChangesAsync();

            var result = await MapSubmissionToDtoAsync(_unitOfWork, target);
            var message = approved
                ? "Grading approved and marks published to student."
                : "Grading rejected.";
            return ApiResponse<ExamSubmissionDto>.SuccessResponse(result, message);
        }
        catch (Exception ex)
        {
            return ApiResponse<ExamSubmissionDto>.FailResponse(ex.Message);
        }
    }

    private static ExamDto MapToDto(Domain.Entities.Exam.Exam exam)
    {
        return new ExamDto
        {
            Id = exam.Id,
            Name = exam.Name,
            Description = exam.Description,
            ExamType = exam.ExamType?.Name ?? string.Empty,
            ClassRoomId = exam.ClassRoomId ?? Guid.Empty,
            ClassName = exam.ClassRoom?.Name ?? string.Empty,
            SectionId = exam.SectionId,
            SectionName = exam.Section?.Name ?? string.Empty,
            AcademicYearId = exam.AcademicYearId,
            AcademicYearName = exam.AcademicYear?.Name ?? string.Empty,
            StartDate = exam.StartDate,
            EndDate = exam.EndDate,
            MaxMarks = null,
            PassingMarks = null,
            IsPublished = exam.Status == ExamStatus.Completed,
            IsActive = exam.Status != ExamStatus.Cancelled,
            TeacherName = exam.Teacher?.User is not null
                ? $"{exam.Teacher.User.FirstName} {exam.Teacher.User.LastName}"
                : string.Empty,
            QuestionPaperUrl = exam.QuestionPaperUrl,
            QuestionPaperFileName = exam.QuestionPaperFileName,
            ApprovalStatus = (exam.ApprovalStatus == 0 ? ApprovalStatus.Pending : exam.ApprovalStatus).ToString(),
            ApprovedBy = exam.ApprovedBy,
            ApprovedAt = exam.ApprovedAt,
            RejectionReason = exam.RejectionReason,
            QuestionCount = exam.Questions?.Count ?? 0,
            TotalMarks = exam.Questions?.Sum(q => q.Marks) ?? 0
        };
    }

    private async Task NotifyAndEmailStudentsOnExamCreatedAsync(Domain.Entities.Exam.Exam exam)
    {
        try
        {
            if (!exam.ClassRoomId.HasValue)
                return;

            var title = "New exam scheduled";
            var message = $"New exam '{exam.Name}' has been scheduled from {exam.StartDate:dd MMM yyyy} to {exam.EndDate:dd MMM yyyy}.";
            var link = $"/exams/{exam.Id}";

            await _notificationService.NotifyStudentsOfClassAsync(
                exam.ClassRoomId.Value, title, message, NotificationType.Info, sectionId: exam.SectionId, link: link);

            var sections = (await _unitOfWork.Sections.FindAsync(s =>
                s.ClassRoomId == exam.ClassRoomId.Value && !s.IsDeleted)).ToList();

            if (exam.SectionId.HasValue)
                sections = sections.Where(s => s.Id == exam.SectionId.Value).ToList();

            var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var section in sections)
            {
                var students = await _unitOfWork.Students.GetBySectionAsync(section.Id);
                foreach (var student in students.Where(s => !s.IsDeleted && !string.IsNullOrWhiteSpace(s.ParentEmail)))
                    emails.Add(student.ParentEmail!);
            }

            var subject = $"New exam scheduled: {exam.Name}";
            var body = $@"
                <h3>New exam scheduled</h3>
                <p><strong>{exam.Name}</strong></p>
                <p>{exam.Description}</p>
                <p><strong>Start:</strong> {exam.StartDate:dd MMM yyyy}</p>
                <p><strong>End:</strong> {exam.EndDate:dd MMM yyyy}</p>
                <p>Please help your child prepare for the exam.</p>";

            foreach (var email in emails)
                await _emailService.SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to notify on exam created: {ex.Message}");
        }
    }
}
