using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse> MarkAttendanceAsync(MarkAttendanceDto dto)
    {
        try
        {
            foreach (var record in dto.Records)
            {
                var existing = (await _unitOfWork.Attendances.FindAsync(a =>
                    a.Date.Date == dto.Date.Date &&
                    a.StudentId == record.StudentId &&
                    !a.IsDeleted)).FirstOrDefault();

                if (existing is not null)
                {
                    existing.Status = Enum.Parse<AttendanceStatus>(record.Status);
                    existing.Remarks = record.Remarks;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Attendances.UpdateAsync(existing);
                }
                else
                {
                    var attendance = new Domain.Entities.Attendance.Attendance
                    {
                        Date = dto.Date,
                        Status = Enum.Parse<AttendanceStatus>(record.Status),
                        StudentId = record.StudentId,
                        Remarks = record.Remarks,
                        SchoolId = Guid.Empty,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Attendances.AddAsync(attendance);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse(ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<AttendanceDto>>> GetAttendanceAsync(
        PaginationQuery query, DateTime? date, Guid? classRoomId, Guid? sectionId, string? status)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.Attendances.GetPagedAttendanceAsync(
                query.PageNumber, query.PageSize, date, classRoomId, sectionId, status, null);

            var dtos = items.Select(a => new AttendanceDto
            {
                Id = a.Id,
                StudentId = a.StudentId ?? Guid.Empty,
                StudentName = a.Student?.User is not null
                    ? $"{a.Student.User.FirstName} {a.Student.User.LastName}"
                    : string.Empty,
                AdmissionNumber = a.Student?.AdmissionNumber ?? string.Empty,
                ClassName = a.Student?.Section?.ClassRoom?.Name ?? string.Empty,
                SectionName = a.Student?.Section?.Name ?? string.Empty,
                Date = a.Date,
                Status = a.Status.ToString(),
                Remarks = a.Remarks,
                CreatedAt = a.CreatedAt
            }).ToList();

            var pagedResult = new PagedResult<AttendanceDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm
            };

            return ApiResponse<PagedResult<AttendanceDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<AttendanceDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<AttendanceStatsDto>> GetAttendanceStatsAsync(
        DateTime date, Guid? classRoomId, Guid? sectionId)
    {
        try
        {
            var stats = await _unitOfWork.Attendances.GetAttendanceStatsAsync(date, classRoomId, sectionId);
            var totalStudents = stats.Values.Sum();

            var result = new AttendanceStatsDto
            {
                Date = date,
                ClassRoomId = classRoomId,
                SectionId = sectionId,
                TotalStudents = totalStudents,
                Present = stats.GetValueOrDefault(AttendanceStatus.Present),
                Absent = stats.GetValueOrDefault(AttendanceStatus.Absent),
                Late = stats.GetValueOrDefault(AttendanceStatus.Late),
                Excused = stats.GetValueOrDefault(AttendanceStatus.Excused),
                AttendancePercentage = totalStudents > 0
                    ? Math.Round((decimal)stats.GetValueOrDefault(AttendanceStatus.Present) / totalStudents * 100, 2)
                    : 0
            };

            return ApiResponse<AttendanceStatsDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<AttendanceStatsDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<AttendanceDto>>> GetStudentAttendanceAsync(
        Guid studentId, PaginationQuery query)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-6);

            var items = await _unitOfWork.Attendances.GetByStudentAsync(studentId, startDate, endDate);
            var totalCount = items.Count;

            var dtos = items.Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId ?? Guid.Empty,
                    StudentName = a.Student?.User is not null
                        ? $"{a.Student.User.FirstName} {a.Student.User.LastName}"
                        : string.Empty,
                    AdmissionNumber = a.Student?.AdmissionNumber ?? string.Empty,
                    ClassName = a.Student?.Section?.ClassRoom?.Name ?? string.Empty,
                    SectionName = a.Student?.Section?.Name ?? string.Empty,
                    Date = a.Date,
                    Status = a.Status.ToString(),
                    Remarks = a.Remarks,
                    CreatedAt = a.CreatedAt
                }).ToList();

            var pagedResult = new PagedResult<AttendanceDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return ApiResponse<PagedResult<AttendanceDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<AttendanceDto>>.FailResponse(ex.Message);
        }
    }
}
