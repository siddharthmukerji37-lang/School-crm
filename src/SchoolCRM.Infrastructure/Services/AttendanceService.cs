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

    public async Task<ApiResponse<MyAttendanceDto>> ClockInAsync()
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
            }
            else
            {
                await _unitOfWork.Attendances.AddAsync(new Domain.Entities.Attendance.Attendance
                {
                    Date = today,
                    Status = AttendanceStatus.Present,
                    CheckInTime = now,
                    TeacherId = teacherId,
                    EmployeeId = employeeId,
                    SchoolId = schoolId.Value,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();

            var dto = await BuildMyAttendanceDtoAsync(today, teacherId, employeeId);
            return ApiResponse<MyAttendanceDto>.SuccessResponse(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<MyAttendanceDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<MyAttendanceDto>> ClockOutAsync()
    {
        try
        {
            var (teacherId, employeeId) = await ResolveCurrentPersonAsync();
            if (teacherId is null && employeeId is null)
                return ApiResponse<MyAttendanceDto>.FailResponse(
                    "No teacher or employee profile is linked to your account.");

            var today = DateTime.Now.Date;

            var existing = (await _unitOfWork.Attendances.FindAsync(a =>
                a.Date.Date == today &&
                a.TeacherId == teacherId &&
                a.EmployeeId == employeeId &&
                !a.IsDeleted)).FirstOrDefault();

            if (existing is null)
                return ApiResponse<MyAttendanceDto>.FailResponse("You have not clocked in today.");

            existing.CheckOutTime = DateTime.Now.TimeOfDay;
            existing.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Attendances.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync();

            var dto = await BuildMyAttendanceDtoAsync(today, teacherId, employeeId);
            return ApiResponse<MyAttendanceDto>.SuccessResponse(dto);
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
            IsCheckedOut = record.CheckOutTime.HasValue
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
}
