using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.DTOs.Exam;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Exam;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class ExamService : IExamService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExamService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<ExamDto>>> GetExamsAsync(PaginationQuery query, Guid? classRoomId)
    {
        try
        {
            Expression<Func<Exam, bool>>? filter = e => !e.IsDeleted;
            if (classRoomId.HasValue)
                filter = e => !e.IsDeleted && e.SchoolId == classRoomId.Value;

            var (items, totalCount) = await _unitOfWork.Exams.GetPagedAsync(
                query.PageNumber, query.PageSize, filter,
                q => q.OrderByDescending(e => e.StartDate),
                q => q.Include(e => e.ExamType).Include(e => e.ClassRoom));

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
                ExamTypeId = examTypeId,
                AcademicYearId = dto.AcademicYearId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Exams.AddAsync(exam);
            await _unitOfWork.SaveChangesAsync();

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
            AcademicYearId = exam.AcademicYearId,
            AcademicYearName = exam.AcademicYear?.Name ?? string.Empty,
            StartDate = exam.StartDate,
            EndDate = exam.EndDate,
            MaxMarks = null,
            PassingMarks = null,
            IsPublished = exam.Status == ExamStatus.Completed,
            IsActive = exam.Status != ExamStatus.Cancelled
        };
    }
}
