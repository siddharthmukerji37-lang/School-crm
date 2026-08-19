using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AttendanceService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse> MarkAttendanceAsync(MarkAttendanceDto dto)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null || schoolId == Guid.Empty)
                return ApiResponse.FailResponse("Unable to determine the current school context. Please sign in again.");

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
                        SchoolId = schoolId.Value,
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

    public async Task<ApiResponse> MarkTeacherAttendanceAsync(MarkStaffAttendanceDto dto)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null || schoolId == Guid.Empty)
                return ApiResponse.FailResponse("Unable to determine the current school context. Please sign in again.");

            foreach (var record in dto.Records.Where(r => r.TeacherId.HasValue))
            {
                var existing = (await _unitOfWork.Attendances.FindAsync(a =>
                    a.Date.Date == dto.Date.Date &&
                    a.TeacherId == record.TeacherId &&
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
                        TeacherId = record.TeacherId,
                        Remarks = record.Remarks,
                        SchoolId = schoolId.Value,
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

    public async Task<ApiResponse> MarkEmployeeAttendanceAsync(MarkStaffAttendanceDto dto)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null || schoolId == Guid.Empty)
                return ApiResponse.FailResponse("Unable to determine the current school context. Please sign in again.");

            foreach (var record in dto.Records.Where(r => r.EmployeeId.HasValue))
            {
                var existing = (await _unitOfWork.Attendances.FindAsync(a =>
                    a.Date.Date == dto.Date.Date &&
                    a.EmployeeId == record.EmployeeId &&
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
                        EmployeeId = record.EmployeeId,
                        Remarks = record.Remarks,
                        SchoolId = schoolId.Value,
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

    public async Task<ApiResponse<PagedResult<StaffAttendanceDto>>> GetStaffAttendanceAsync(
        PaginationQuery query, DateTime? date, string? role, string? status)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            var day = date?.Date ?? DateTime.UtcNow.Date;

            var records = (await _unitOfWork.Attendances.GetByDateAsync(day, schoolId))
                .Where(a => a.TeacherId.HasValue || a.EmployeeId.HasValue)
                .ToList();

            if (!string.IsNullOrWhiteSpace(role))
            {
                if (role.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                    records = records.Where(a => a.TeacherId.HasValue).ToList();
                else if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                    records = records.Where(a => a.EmployeeId.HasValue).ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
                records = records.Where(a =>
                    a.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

            var ordered = records
                .OrderBy(a => a.TeacherId.HasValue ? 0 : 1)
                .ThenBy(a => a.Teacher?.User?.FirstName ?? a.Employee?.User?.FirstName ?? string.Empty)
                .ToList();

            var totalCount = ordered.Count;
            var paged = ordered
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

            var dtos = paged.Select(a => new StaffAttendanceDto
            {
                Id = a.Id,
                TeacherId = a.TeacherId,
                EmployeeId = a.EmployeeId,
                Name = a.Teacher?.User is not null
                    ? $"{a.Teacher.User.FirstName} {a.Teacher.User.LastName}".Trim()
                    : a.Employee?.User is not null
                        ? $"{a.Employee.User.FirstName} {a.Employee.User.LastName}".Trim()
                        : string.Empty,
                Role = a.TeacherId.HasValue
                    ? "Teacher"
                    : a.Employee?.Designation?.Name ?? "Employee",
                Department = a.Teacher?.DepartmentName ?? a.Employee?.Department?.Name ?? string.Empty,
                EmployeeCode = a.Teacher?.EmployeeCode ?? a.Employee?.EmployeeCode ?? string.Empty,
                Date = a.Date,
                Status = a.Status.ToString(),
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                Remarks = a.Remarks
            }).ToList();

            var pagedResult = new PagedResult<StaffAttendanceDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm
            };

            return ApiResponse<PagedResult<StaffAttendanceDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<StaffAttendanceDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StaffAttendanceStatsDto>> GetStaffAttendanceStatsAsync(DateTime date)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            var records = await _unitOfWork.Attendances.GetByDateAsync(date.Date, schoolId);

            var teachers = records.Where(a => a.TeacherId.HasValue).ToList();
            var employees = records.Where(a => a.EmployeeId.HasValue).ToList();

            var result = new StaffAttendanceStatsDto
            {
                Date = date.Date,
                TotalTeachers = teachers.Count,
                TeachersPresent = teachers.Count(a => a.Status == AttendanceStatus.Present),
                TeachersAbsent = teachers.Count(a => a.Status != AttendanceStatus.Present),
                TotalEmployees = employees.Count,
                EmployeesPresent = employees.Count(a => a.Status == AttendanceStatus.Present),
                EmployeesAbsent = employees.Count(a => a.Status != AttendanceStatus.Present)
            };

            return ApiResponse<StaffAttendanceStatsDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<StaffAttendanceStatsDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<MyAttendanceDto>> GetMyAttendanceAsync()
    {
        try
        {
            var (teacherId, employeeId) = await ResolveCurrentPersonAsync();
            if (teacherId is null && employeeId is null)
                return ApiResponse<MyAttendanceDto>.FailResponse(
                    "No teacher or employee profile is linked to your account.");

            var dto = await BuildMyAttendanceDtoAsync(DateTime.Now.Date, teacherId, employeeId);
            return ApiResponse<MyAttendanceDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<MyAttendanceDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<MyAttendanceDto>> ClockInAsync(ClockInDto? dto = null)
    {
        try
        {
            var (teacherId, employeeId) = await ResolveCurrentPersonAsync();
            if (teacherId is null && employeeId is null)
                return ApiResponse<MyAttendanceDto>.FailResponse(
                    "No teacher or employee profile is linked to your account.");

            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null || schoolId == Guid.Empty)
                return ApiResponse<MyAttendanceDto>.FailResponse(
                    "Unable to determine the current school context. Please sign in again.");

            var today = DateTime.Now.Date;
            var now = DateTime.Now.TimeOfDay;

            var existing = (await _unitOfWork.Attendances.FindAsync(a =>
                a.Date.Date == today &&
                a.TeacherId == teacherId &&
                a.EmployeeId == employeeId &&
                !a.IsDeleted)).FirstOrDefault();

            if (existing is not null)
            {
                existing.Status = AttendanceStatus.Present;
                existing.CheckInTime ??= now;
                existing.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Attendances.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                var existingDto = await BuildMyAttendanceDtoAsync(today, teacherId, employeeId);
                return ApiResponse<MyAttendanceDto>.SuccessResponse(existingDto);
            }

            var policy = (await _unitOfWork.AttendancePolicies.FindAsync(
                p => p.SchoolId == schoolId && p.IsActive && !p.IsDeleted)).FirstOrDefault();

            var lateMinutes = 0;
            var isLate = false;
            if (policy is not null && now > policy.SchoolStartTime)
            {
                lateMinutes = (int)(now - policy.SchoolStartTime).TotalMinutes;
                isLate = lateMinutes > 0;
            }
            else if (policy is null && now > new TimeSpan(9, 30, 0))
            {
                lateMinutes = (int)(now - new TimeSpan(9, 30, 0)).TotalMinutes;
                isLate = lateMinutes > 0;
            }

            var userId = Guid.TryParse(_currentUserService.UserId, out var uid) ? uid : Guid.Empty;
            var allowedLate = policy?.AllowedLateArrivals ?? 6;
            var month = today.Month;
            var year = today.Year;

            var monthlySummary = userId != Guid.Empty
                ? (await _unitOfWork.AttendanceMonthlySummaries.FindAsync(
                    s => s.UserId == userId && s.Month == month && s.Year == year && !s.IsDeleted)).FirstOrDefault()
                : null;

            var lateCountMonth = monthlySummary?.TotalLateCount ?? 0;
            if (isLate) lateCountMonth++;

            var policyExceeded = policy?.SalaryDeductionEnabled == true && lateCountMonth > allowedLate;
            var salaryDeductionRequired = policyExceeded;

            if (isLate && monthlySummary is null && userId != Guid.Empty)
            {
                monthlySummary = new Domain.Entities.Attendance.AttendanceMonthlySummary
                {
                    UserId = userId,
                    Month = month,
                    Year = year,
                    TotalLateCount = lateCountMonth,
                    AllowedLateCount = allowedLate,
                    PolicyExceeded = policyExceeded,
                    SalaryDeductionCount = salaryDeductionRequired ? 1 : 0,
                    SalaryDeductionAmount = salaryDeductionRequired ? policy?.DeductionAmount ?? 0 : 0,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AttendanceMonthlySummaries.AddAsync(monthlySummary);
            }
            else if (isLate && monthlySummary is not null)
            {
                monthlySummary.TotalLateCount = lateCountMonth;
                monthlySummary.PolicyExceeded = policyExceeded;
                if (salaryDeductionRequired)
                {
                    monthlySummary.SalaryDeductionCount++;
                    monthlySummary.SalaryDeductionAmount += policy?.DeductionAmount ?? 0;
                }
                monthlySummary.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.AttendanceMonthlySummaries.UpdateAsync(monthlySummary);
            }

            var status = isLate ? AttendanceStatus.Late : AttendanceStatus.Present;

            var attendance = new Domain.Entities.Attendance.Attendance
            {
                Date = today,
                Status = status,
                CheckInTime = now,
                TeacherId = teacherId,
                EmployeeId = employeeId,
                SchoolId = schoolId.Value,
                LateMinutes = isLate ? lateMinutes : null,
                LateReason = isLate ? dto?.LateReason : null,
                LateCountMonth = lateCountMonth,
                LatePolicyExceeded = policyExceeded,
                SalaryDeductionRequired = salaryDeductionRequired,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Attendances.AddAsync(attendance);

            await _unitOfWork.SaveChangesAsync();

            string? warning = null;
            if (policyExceeded)
                warning = $"You have exceeded the allowed {allowedLate} late arrivals this month. A salary deduction of {policy?.DeductionAmount} will be applied.";
            else if (isLate && lateCountMonth == allowedLate)
                warning = $"This is your {allowedLate}th late arrival this month. One more late arrival will trigger a salary deduction.";

            var resultDto = await BuildMyAttendanceDtoAsync(today, teacherId, employeeId);
            resultDto.LateMinutes = lateMinutes;
            resultDto.LateReason = isLate ? dto?.LateReason : null;
            resultDto.LateCount = lateCountMonth;
            resultDto.AllowedLateCount = allowedLate;
            resultDto.PolicyExceeded = policyExceeded;
            resultDto.SalaryDeductionRequired = salaryDeductionRequired;
            resultDto.Warning = warning;

            return ApiResponse<MyAttendanceDto>.SuccessResponse(resultDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<MyAttendanceDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<MyAttendanceDto>> ClockOutAsync(ClockOutDto? dto = null)
    {
        try
        {
            var (teacherId, employeeId) = await ResolveCurrentPersonAsync();
            if (teacherId is null && employeeId is null)
                return ApiResponse<MyAttendanceDto>.FailResponse(
                    "No teacher or employee profile is linked to your account.");

            var schoolId = await ResolveSchoolIdAsync();
            var today = DateTime.Now.Date;
            var now = DateTime.Now.TimeOfDay;

            var existing = (await _unitOfWork.Attendances.FindAsync(a =>
                a.Date.Date == today &&
                a.TeacherId == teacherId &&
                a.EmployeeId == employeeId &&
                !a.IsDeleted)).FirstOrDefault();

            if (existing is null)
                return ApiResponse<MyAttendanceDto>.FailResponse("You have not clocked in today.");

            var policy = (await _unitOfWork.AttendancePolicies.FindAsync(
                p => p.SchoolId == schoolId && p.IsActive && !p.IsDeleted)).FirstOrDefault();

            var schoolEndTime = policy?.SchoolEndTime ?? new TimeSpan(18, 30, 0);
            var isEarly = now < schoolEndTime;
            var earlyMinutes = 0;

            if (isEarly)
            {
                earlyMinutes = (int)(schoolEndTime - now).TotalMinutes;
            }

            existing.CheckOutTime = now;
            existing.EarlyMinutes = isEarly ? earlyMinutes : null;
            existing.EarlyReason = isEarly ? dto?.EarlyReason : null;
            existing.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Attendances.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            string? earlyWarning = null;
            if (isEarly && !string.IsNullOrWhiteSpace(dto?.EarlyReason))
            {
                earlyWarning = $"You are leaving {earlyMinutes} minutes early. Your early departure has been recorded.";
            }

            var resultDto = await BuildMyAttendanceDtoAsync(today, teacherId, employeeId);
            resultDto.EarlyMinutes = isEarly ? earlyMinutes : 0;
            resultDto.EarlyReason = isEarly ? dto?.EarlyReason : null;
            resultDto.EarlyDeparture = isEarly;
            resultDto.EarlyWarning = earlyWarning;

            return ApiResponse<MyAttendanceDto>.SuccessResponse(resultDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<MyAttendanceDto>.FailResponse(ex.Message);
        }
    }

    private async Task<(Guid? TeacherId, Guid? EmployeeId)> ResolveCurrentPersonAsync()
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            return (null, null);

        var teacher = await _unitOfWork.Teachers.GetTeacherByUserIdAsync(userId);
        if (teacher is not null && !teacher.IsDeleted)
            return (teacher.Id, null);

        var employee = await _unitOfWork.Employees.GetEmployeeByUserIdAsync(userId);
        if (employee is not null && !employee.IsDeleted)
            return (null, employee.Id);

        return (null, null);
    }

    private async Task<MyAttendanceDto> BuildMyAttendanceDtoAsync(
        DateTime date, Guid? teacherId, Guid? employeeId)
    {
        var record = (await _unitOfWork.Attendances.FindAsync(a =>
                a.Date.Date == date.Date &&
                a.TeacherId == teacherId &&
                a.EmployeeId == employeeId &&
                !a.IsDeleted))
            .FirstOrDefault();

        if (record is null)
            return new MyAttendanceDto { Date = date.Date, Status = "Not Marked" };

        return new MyAttendanceDto
        {
            Id = record.Id,
            Date = record.Date,
            Status = record.Status.ToString(),
            CheckInTime = record.CheckInTime,
            CheckOutTime = record.CheckOutTime,
            Remarks = record.Remarks,
            IsCheckedIn = record.CheckInTime.HasValue,
            IsCheckedOut = record.CheckOutTime.HasValue,
            LateMinutes = record.LateMinutes ?? 0,
            LateReason = record.LateReason,
            LateCount = record.LateCountMonth,
            PolicyExceeded = record.LatePolicyExceeded,
            SalaryDeductionRequired = record.SalaryDeductionRequired,
            EarlyMinutes = record.EarlyMinutes ?? 0,
            EarlyReason = record.EarlyReason,
            EarlyDeparture = record.EarlyMinutes.HasValue && record.EarlyMinutes > 0
        };
    }

    private async Task<Guid?> ResolveSchoolIdAsync()
    {
        var schoolId = _currentUserService.SchoolId;
        if (schoolId is null || schoolId == Guid.Empty)
        {
            var schools = await _unitOfWork.Schools.GetAllAsync();
            schoolId = schools.FirstOrDefault()?.Id;
        }

        return schoolId;
    }

    public async Task<ApiResponse<PagedResult<LateStaffDto>>> GetLateStaffAsync(DateTime? date, int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            var day = date?.Date ?? DateTime.UtcNow.Date;

            var query = _unitOfWork.Attendances.AsQueryable()
                .Where(a => a.Date.Date == day && a.SchoolId == schoolId && !a.IsDeleted && a.LateMinutes.HasValue && a.LateMinutes > 0)
                .Include(a => a.Teacher).ThenInclude(t => t!.User)
                .Include(a => a.Employee).ThenInclude(e => e!.User);

            var totalCount = await query.CountAsync();
            var paged = await query
                .OrderByDescending(a => a.LateMinutes)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = paged.Select(a =>
            {
                var name = a.Teacher?.User is not null
                    ? $"{a.Teacher.User.FirstName} {a.Teacher.User.LastName}".Trim()
                    : a.Employee?.User is not null
                        ? $"{a.Employee.User.FirstName} {a.Employee.User.LastName}".Trim()
                        : string.Empty;
                var role = a.TeacherId.HasValue ? "Teacher" : a.Employee?.Designation?.Name ?? "Employee";
                return new LateStaffDto
                {
                    Id = a.Id,
                    TeacherId = a.TeacherId,
                    EmployeeId = a.EmployeeId,
                    Name = name,
                    Role = role,
                    Date = a.Date,
                    CheckInTime = a.CheckInTime,
                    LateMinutes = a.LateMinutes ?? 0,
                    LateCountMonth = a.LateCountMonth,
                    AllowedLateCount = 6,
                    LateReason = a.LateReason,
                    LatePolicyExceeded = a.LatePolicyExceeded,
                    SalaryDeductionRequired = a.SalaryDeductionRequired
                };
            }).ToList();

            var result = new PagedResult<LateStaffDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return ApiResponse<PagedResult<LateStaffDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<LateStaffDto>>.FailResponse(ex.Message);
        }
    }
}
