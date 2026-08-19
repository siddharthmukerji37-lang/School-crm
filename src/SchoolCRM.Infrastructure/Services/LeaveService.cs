using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.DTOs.Leave;
using SchoolCRM.Application.DTOs;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Attendance;
using SchoolCRM.Domain.Entities.Leave;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class LeaveService : ILeaveService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public LeaveService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    private async Task<(Guid? TeacherId, Guid? EmployeeId, string UserType, string Gender)> ResolveCurrentUserAsync()
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            return (null, null, "", "");

        var teacher = (await _unitOfWork.Teachers.FindAsync(t => t.UserId == userId && !t.IsDeleted)).FirstOrDefault();
        if (teacher is not null)
            return (teacher.Id, null, "Teacher", teacher.Gender.ToString());

        var employee = (await _unitOfWork.Employees.FindAsync(e => e.UserId == userId && !e.IsDeleted)).FirstOrDefault();
        if (employee is not null)
            return (null, employee.Id, "Employee", employee.Gender.ToString());

        return (null, null, "", "");
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

    private async Task<LeaveCalendar?> GetActiveCalendarAsync()
    {
        var schoolId = await ResolveSchoolIdAsync();
        if (schoolId is null) return null;
        var year = DateTime.Now.Year;
        return await ((Repositories.LeaveCalendarRepository)_unitOfWork.LeaveCalendars)
            .GetActiveCalendarAsync(schoolId.Value, year);
    }

    private async Task<UserProfile?> GetUserProfileAsync(Guid userId)
    {
        var teacher = (await _unitOfWork.Teachers.FindAsync(t => t.UserId == userId && !t.IsDeleted)).FirstOrDefault();
        if (teacher is not null)
            return new UserProfile { Name = $"{teacher.FirstName} {teacher.LastName}".Trim(), Type = "Teacher" };

        var employee = (await _unitOfWork.Employees.FindAsync(e => e.UserId == userId && !e.IsDeleted)).FirstOrDefault();
        if (employee is not null)
            return new UserProfile { Name = $"{employee.FirstName} {employee.LastName}".Trim(), Type = "Employee" };

        return null;
    }

    private class UserProfile
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
    }

    private int CalculateLeaveDays(DateTime fromDate, DateTime toDate)
    {
        var days = 0;
        var current = fromDate.Date;
        while (current <= toDate.Date)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                days++;
            current = current.AddDays(1);
        }
        return Math.Max(days, 1);
    }

    // ======================== USER APIs ========================

    public async Task<ApiResponse<List<LeaveTypeConfigDto>>> GetLeaveTypesForUserAsync()
    {
        try
        {
            var (_, _, userType, gender) = await ResolveCurrentUserAsync();
            if (string.IsNullOrEmpty(userType))
                return ApiResponse<List<LeaveTypeConfigDto>>.FailResponse("No teacher or employee profile found.");

            var calendar = await GetActiveCalendarAsync();
            if (calendar is null)
                return ApiResponse<List<LeaveTypeConfigDto>>.FailResponse("No active leave calendar found.");

            var configs = await ((Repositories.LeaveTypeConfigRepository)_unitOfWork.LeaveTypeConfigs)
                .GetApplicableForUserAsync(calendar.Id, userType, gender);

            var dtos = configs.Select(c => new LeaveTypeConfigDto
            {
                Id = c.Id,
                LeaveTypeId = c.LeaveTypeId,
                LeaveTypeName = c.LeaveType.Name,
                LeaveTypeCode = c.LeaveType.Code,
                TotalDays = c.TotalDays,
                IsPaid = c.IsPaid,
                ApplicableGender = c.ApplicableGender,
                ApplicableUserType = c.ApplicableUserType,
                MinimumDays = c.MinimumDays,
                MaximumDays = c.MaximumDays,
                IsActive = c.IsActive
            }).ToList();

            return ApiResponse<List<LeaveTypeConfigDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LeaveTypeConfigDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeaveBalanceDto>>> GetMyLeaveBalanceAsync()
    {
        try
        {
            var (teacherId, employeeId, _, _) = await ResolveCurrentUserAsync();
            var userId = Guid.TryParse(_currentUserService.UserId, out var uid) ? uid : Guid.Empty;
            if (userId == Guid.Empty)
                return ApiResponse<List<LeaveBalanceDto>>.FailResponse("User not found.");

            var calendar = await GetActiveCalendarAsync();
            if (calendar is null)
                return ApiResponse<List<LeaveBalanceDto>>.FailResponse("No active leave calendar found.");

            var balances = await ((Repositories.LeaveBalanceRepository)_unitOfWork.LeaveBalances)
                .GetByUserAsync(userId, calendar.Id);

            var dtos = balances.Select(b => new LeaveBalanceDto
            {
                Id = b.Id,
                LeaveTypeId = b.LeaveTypeId,
                LeaveTypeName = b.LeaveType.Name,
                AllocatedDays = b.AllocatedDays,
                UsedDays = b.UsedDays,
                PendingDays = b.PendingDays,
                RemainingDays = b.AllocatedDays - b.UsedDays
            }).ToList();

            return ApiResponse<List<LeaveBalanceDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LeaveBalanceDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<LeaveRequestDto>> ApplyLeaveAsync(ApplyLeaveDto dto)
    {
        try
        {
            var userId = Guid.TryParse(_currentUserService.UserId, out var uid) ? uid : Guid.Empty;
            if (userId == Guid.Empty)
                return ApiResponse<LeaveRequestDto>.FailResponse("User not found.");

            var (_, _, userType, _) = await ResolveCurrentUserAsync();

            var calendar = await GetActiveCalendarAsync();
            if (calendar is null)
                return ApiResponse<LeaveRequestDto>.FailResponse("No active leave calendar found.");

            if (dto.FromDate.Date > dto.ToDate.Date)
                return ApiResponse<LeaveRequestDto>.FailResponse("From date cannot be after To date.");

            if (dto.FromDate.Date < DateTime.Today)
                return ApiResponse<LeaveRequestDto>.FailResponse("Cannot apply for leave in the past.");

            var totalDays = CalculateLeaveDays(dto.FromDate, dto.ToDate);

            var config = (await _unitOfWork.LeaveTypeConfigs.FindAsync(c =>
                c.LeaveCalendarId == calendar.Id && c.LeaveTypeId == dto.LeaveTypeId && c.IsActive && !c.IsDeleted)).FirstOrDefault();

            if (config is null)
                return ApiResponse<LeaveRequestDto>.FailResponse("Leave type is not available for the current calendar.");

            if (totalDays < config.MinimumDays || totalDays > config.MaximumDays)
                return ApiResponse<LeaveRequestDto>.FailResponse($"Leave duration must be between {config.MinimumDays} and {config.MaximumDays} days.");

            var balance = await ((Repositories.LeaveBalanceRepository)_unitOfWork.LeaveBalances)
                .GetByUserAndTypeAsync(userId, dto.LeaveTypeId, calendar.Id);

            if (balance is null)
                return ApiResponse<LeaveRequestDto>.FailResponse("No leave balance found for this leave type.");

            if (balance.AllocatedDays - balance.UsedDays < totalDays)
                return ApiResponse<LeaveRequestDto>.FailResponse($"Insufficient leave balance. Available: {balance.AllocatedDays - balance.UsedDays} days.");

            var dayRepo = (Repositories.LeaveRequestDayRepository)_unitOfWork.LeaveRequestDays;
            if (await dayRepo.HasConflictAsync(userId, dto.FromDate, dto.ToDate))
                return ApiResponse<LeaveRequestDto>.FailResponse("You already have a leave request for one or more of the selected dates.");

            if (string.IsNullOrWhiteSpace(dto.Reason))
                return ApiResponse<LeaveRequestDto>.FailResponse("Reason is required.");

            if (config.LeaveType.RequiresAttachment && string.IsNullOrEmpty(dto.AttachmentPath))
                return ApiResponse<LeaveRequestDto>.FailResponse("Attachment is required for this leave type.");

            var request = new LeaveRequest
            {
                UserId = userId,
                LeaveTypeId = dto.LeaveTypeId,
                LeaveCalendarId = calendar.Id,
                FromDate = dto.FromDate.Date,
                ToDate = dto.ToDate.Date,
                TotalDays = totalDays,
                Reason = dto.Reason.Trim(),
                AttachmentPath = dto.AttachmentPath,
                Status = LeaveStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.LeaveRequests.AddAsync(request);

            var days = new List<LeaveRequestDay>();
            var current = dto.FromDate.Date;
            while (current <= dto.ToDate.Date)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    days.Add(new LeaveRequestDay
                    {
                        LeaveRequestId = request.Id,
                        LeaveDate = current,
                        Status = LeaveStatus.Pending
                    });
                }
                current = current.AddDays(1);
            }
            await ((Repositories.LeaveRequestDayRepository)_unitOfWork.LeaveRequestDays).AddRangeAsync(days);

            balance.PendingDays += totalDays;
            await _unitOfWork.LeaveBalances.UpdateAsync(balance);

            await _unitOfWork.SaveChangesAsync();

            var profile = await GetUserProfileAsync(userId);
            var leaveType = (await _unitOfWork.LeaveTypes.GetByIdAsync(dto.LeaveTypeId));

            return ApiResponse<LeaveRequestDto>.SuccessResponse(new LeaveRequestDto
            {
                Id = request.Id,
                UserId = userId,
                UserName = profile?.Name ?? "",
                UserType = profile?.Type ?? "",
                LeaveTypeId = dto.LeaveTypeId,
                LeaveTypeName = leaveType?.Name ?? "",
                LeaveTypeCode = leaveType?.Code ?? "",
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = totalDays,
                Reason = request.Reason,
                Status = LeaveStatus.Pending,
                CreatedAt = request.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<LeaveRequestDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeaveRequestDto>>> GetMyLeaveRequestsAsync()
    {
        try
        {
            var userId = Guid.TryParse(_currentUserService.UserId, out var uid) ? uid : Guid.Empty;
            if (userId == Guid.Empty)
                return ApiResponse<List<LeaveRequestDto>>.FailResponse("User not found.");

            var requests = await ((Repositories.LeaveRequestRepository)_unitOfWork.LeaveRequests)
                .GetByUserAsync(userId);

            var dtos = requests.Select(r =>
            {
                var profile = GetUserProfileAsync(r.UserId).Result;
                return new LeaveRequestDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = profile?.Name ?? "",
                    UserType = profile?.Type ?? "",
                    LeaveTypeId = r.LeaveTypeId,
                    LeaveTypeName = r.LeaveType?.Name ?? "",
                    LeaveTypeCode = r.LeaveType?.Code ?? "",
                    FromDate = r.FromDate,
                    ToDate = r.ToDate,
                    TotalDays = r.TotalDays,
                    Reason = r.Reason,
                    AttachmentPath = r.AttachmentPath,
                    Status = r.Status,
                    ApprovedBy = r.ApprovedBy,
                    ApprovedAt = r.ApprovedAt,
                    RejectedBy = r.RejectedBy,
                    RejectedAt = r.RejectedAt,
                    AdminReason = r.AdminReason,
                    CreatedAt = r.CreatedAt
                };
            }).ToList();

            return ApiResponse<List<LeaveRequestDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LeaveRequestDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> CancelLeaveAsync(Guid requestId)
    {
        try
        {
            var userId = Guid.TryParse(_currentUserService.UserId, out var uid) ? uid : Guid.Empty;
            var request = await ((Repositories.LeaveRequestRepository)_unitOfWork.LeaveRequests)
                .GetByIdWithDetailsAsync(requestId);

            if (request is null)
                return ApiResponse.FailResponse("Leave request not found.");

            if (request.UserId != userId)
                return ApiResponse.FailResponse("You can only cancel your own leave requests.");

            if (request.Status != LeaveStatus.Pending && request.Status != LeaveStatus.Approved)
                return ApiResponse.FailResponse("Only pending or approved leave can be cancelled.");

            if (request.Status == LeaveStatus.Approved && request.FromDate.Date < DateTime.Today)
                return ApiResponse.FailResponse("Cannot cancel past approved leave.");

            var balance = await ((Repositories.LeaveBalanceRepository)_unitOfWork.LeaveBalances)
                .GetByUserAndTypeAsync(userId, request.LeaveTypeId, request.LeaveCalendarId);

            if (balance is not null && request.Status == LeaveStatus.Approved)
            {
                balance.UsedDays -= request.TotalDays;
                await _unitOfWork.LeaveBalances.UpdateAsync(balance);
            }
            else if (balance is not null && request.Status == LeaveStatus.Pending)
            {
                balance.PendingDays -= request.TotalDays;
                await _unitOfWork.LeaveBalances.UpdateAsync(balance);
            }

            if (request.Status == LeaveStatus.Approved)
            {
                var days = await ((Repositories.LeaveRequestDayRepository)_unitOfWork.LeaveRequestDays)
                    .GetByRequestAsync(requestId);

                foreach (var day in days.Where(d => d.LeaveDate.Date >= DateTime.Today))
                {
                    var existingAttendance = (await _unitOfWork.Attendances.FindAsync(a =>
                        a.Date.Date == day.LeaveDate.Date &&
                        ((request.UserId == userId && a.TeacherId != null) || (request.UserId == userId && a.EmployeeId != null)) &&
                        !a.IsDeleted)).FirstOrDefault();

                    if (existingAttendance is not null && existingAttendance.Status == AttendanceStatus.Leave)
                    {
                        existingAttendance.Status = AttendanceStatus.Absent;
                        existingAttendance.LeaveRequestId = null;
                        existingAttendance.LeaveTypeId = null;
                        existingAttendance.LeaveReason = null;
                        existingAttendance.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.Attendances.UpdateAsync(existingAttendance);
                    }

                    day.Status = LeaveStatus.Cancelled;
                    await _unitOfWork.LeaveRequestDays.UpdateAsync(day);
                }
            }

            request.Status = LeaveStatus.Cancelled;
            request.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.LeaveRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse("Leave request cancelled successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    // ======================== ADMIN APIs ========================

    public async Task<ApiResponse<LeaveCalendarDto>> CreateLeaveCalendarAsync(CreateLeaveCalendarDto dto)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null)
                return ApiResponse<LeaveCalendarDto>.FailResponse("Unable to determine school.");

            var calendar = new LeaveCalendar
            {
                Name = dto.Name,
                Year = dto.Year,
                StartDate = dto.StartDate.Date,
                EndDate = dto.EndDate.Date,
                IsActive = dto.IsActive,
                SchoolId = schoolId.Value,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.LeaveCalendars.AddAsync(calendar);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<LeaveCalendarDto>.SuccessResponse(new LeaveCalendarDto
            {
                Id = calendar.Id,
                Name = calendar.Name,
                Year = calendar.Year,
                StartDate = calendar.StartDate,
                EndDate = calendar.EndDate,
                IsActive = calendar.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<LeaveCalendarDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeaveCalendarDto>>> GetLeaveCalendarsAsync()
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            var calendars = (await _unitOfWork.LeaveCalendars.FindAsync(c =>
                c.SchoolId == schoolId && !c.IsDeleted)).OrderByDescending(c => c.Year).ToList();

            var dtos = calendars.Select(c => new LeaveCalendarDto
            {
                Id = c.Id,
                Name = c.Name,
                Year = c.Year,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive
            }).ToList();

            return ApiResponse<List<LeaveCalendarDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LeaveCalendarDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<LeaveCalendarDto>> GetActiveLeaveCalendarAsync()
    {
        try
        {
            var calendar = await GetActiveCalendarAsync();
            if (calendar is null)
                return ApiResponse<LeaveCalendarDto>.FailResponse("No active leave calendar found.");

            return ApiResponse<LeaveCalendarDto>.SuccessResponse(new LeaveCalendarDto
            {
                Id = calendar.Id,
                Name = calendar.Name,
                Year = calendar.Year,
                StartDate = calendar.StartDate,
                EndDate = calendar.EndDate,
                IsActive = calendar.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<LeaveCalendarDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<LeaveTypeDto>> CreateLeaveTypeAsync(CreateLeaveTypeDto dto)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null)
                return ApiResponse<LeaveTypeDto>.FailResponse("Unable to determine school.");

            var existing = (await _unitOfWork.LeaveTypes.FindAsync(t =>
                t.Code == dto.Code && t.SchoolId == schoolId && !t.IsDeleted)).FirstOrDefault();

            if (existing is not null)
                return ApiResponse<LeaveTypeDto>.FailResponse("A leave type with this code already exists.");

            var leaveType = new LeaveType
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                RequiresApproval = dto.RequiresApproval,
                RequiresAttachment = dto.RequiresAttachment,
                SchoolId = schoolId.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.LeaveTypes.AddAsync(leaveType);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<LeaveTypeDto>.SuccessResponse(new LeaveTypeDto
            {
                Id = leaveType.Id,
                Name = leaveType.Name,
                Code = leaveType.Code,
                Description = leaveType.Description,
                RequiresApproval = leaveType.RequiresApproval,
                RequiresAttachment = leaveType.RequiresAttachment,
                IsActive = leaveType.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<LeaveTypeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeaveTypeDto>>> GetLeaveTypesAsync()
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null)
                return ApiResponse<List<LeaveTypeDto>>.FailResponse("Unable to determine school.");

            var types = await ((Repositories.LeaveTypeRepository)_unitOfWork.LeaveTypes)
                .GetBySchoolAsync(schoolId.Value);

            var dtos = types.Select(t => new LeaveTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Code = t.Code,
                Description = t.Description,
                RequiresApproval = t.RequiresApproval,
                RequiresAttachment = t.RequiresAttachment,
                IsActive = t.IsActive
            }).ToList();

            return ApiResponse<List<LeaveTypeDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LeaveTypeDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<LeaveTypeDto>> UpdateLeaveTypeAsync(Guid id, CreateLeaveTypeDto dto)
    {
        try
        {
            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(id);
            if (leaveType is null)
                return ApiResponse<LeaveTypeDto>.FailResponse("Leave type not found.");

            leaveType.Name = dto.Name;
            leaveType.Code = dto.Code;
            leaveType.Description = dto.Description;
            leaveType.RequiresApproval = dto.RequiresApproval;
            leaveType.RequiresAttachment = dto.RequiresAttachment;
            leaveType.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.LeaveTypes.UpdateAsync(leaveType);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<LeaveTypeDto>.SuccessResponse(new LeaveTypeDto
            {
                Id = leaveType.Id,
                Name = leaveType.Name,
                Code = leaveType.Code,
                Description = leaveType.Description,
                RequiresApproval = leaveType.RequiresApproval,
                RequiresAttachment = leaveType.RequiresAttachment,
                IsActive = leaveType.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<LeaveTypeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<LeaveTypeConfigDto>> CreateLeaveTypeConfigAsync(Guid calendarId, CreateLeaveTypeConfigDto dto)
    {
        try
        {
            var existing = (await _unitOfWork.LeaveTypeConfigs.FindAsync(c =>
                c.LeaveCalendarId == calendarId && c.LeaveTypeId == dto.LeaveTypeId && !c.IsDeleted)).FirstOrDefault();

            if (existing is not null)
                return ApiResponse<LeaveTypeConfigDto>.FailResponse("This leave type is already configured for this calendar.");

            var config = new LeaveTypeConfig
            {
                LeaveCalendarId = calendarId,
                LeaveTypeId = dto.LeaveTypeId,
                TotalDays = dto.TotalDays,
                IsPaid = dto.IsPaid,
                ApplicableGender = dto.ApplicableGender,
                ApplicableUserType = dto.ApplicableUserType,
                MinimumDays = dto.MinimumDays,
                MaximumDays = dto.MaximumDays,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.LeaveTypeConfigs.AddAsync(config);
            await _unitOfWork.SaveChangesAsync();

            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(dto.LeaveTypeId);

            return ApiResponse<LeaveTypeConfigDto>.SuccessResponse(new LeaveTypeConfigDto
            {
                Id = config.Id,
                LeaveTypeId = config.LeaveTypeId,
                LeaveTypeName = leaveType?.Name ?? "",
                LeaveTypeCode = leaveType?.Code ?? "",
                TotalDays = config.TotalDays,
                IsPaid = config.IsPaid,
                ApplicableGender = config.ApplicableGender,
                ApplicableUserType = config.ApplicableUserType,
                MinimumDays = config.MinimumDays,
                MaximumDays = config.MaximumDays,
                IsActive = config.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<LeaveTypeConfigDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<LeaveTypeConfigDto>> UpdateLeaveTypeConfigAsync(Guid id, UpdateLeaveTypeConfigDto dto)
    {
        try
        {
            var config = await _unitOfWork.LeaveTypeConfigs.GetByIdAsync(id);
            if (config is null)
                return ApiResponse<LeaveTypeConfigDto>.FailResponse("Leave type configuration not found.");

            config.TotalDays = dto.TotalDays;
            config.IsPaid = dto.IsPaid;
            config.ApplicableGender = dto.ApplicableGender;
            config.ApplicableUserType = dto.ApplicableUserType;
            config.MinimumDays = dto.MinimumDays;
            config.MaximumDays = dto.MaximumDays;
            config.IsActive = dto.IsActive;
            config.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.LeaveTypeConfigs.UpdateAsync(config);
            await _unitOfWork.SaveChangesAsync();

            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(config.LeaveTypeId);

            return ApiResponse<LeaveTypeConfigDto>.SuccessResponse(new LeaveTypeConfigDto
            {
                Id = config.Id,
                LeaveTypeId = config.LeaveTypeId,
                LeaveTypeName = leaveType?.Name ?? "",
                LeaveTypeCode = leaveType?.Code ?? "",
                TotalDays = config.TotalDays,
                IsPaid = config.IsPaid,
                ApplicableGender = config.ApplicableGender,
                ApplicableUserType = config.ApplicableUserType,
                MinimumDays = config.MinimumDays,
                MaximumDays = config.MaximumDays,
                IsActive = config.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<LeaveTypeConfigDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeaveTypeConfigDto>>> GetLeaveTypeConfigsAsync(Guid calendarId)
    {
        try
        {
            var configs = await ((Repositories.LeaveTypeConfigRepository)_unitOfWork.LeaveTypeConfigs)
                .GetByCalendarAsync(calendarId);

            var dtos = configs.Select(c => new LeaveTypeConfigDto
            {
                Id = c.Id,
                LeaveTypeId = c.LeaveTypeId,
                LeaveTypeName = c.LeaveType?.Name ?? "",
                LeaveTypeCode = c.LeaveType?.Code ?? "",
                TotalDays = c.TotalDays,
                IsPaid = c.IsPaid,
                ApplicableGender = c.ApplicableGender,
                ApplicableUserType = c.ApplicableUserType,
                MinimumDays = c.MinimumDays,
                MaximumDays = c.MaximumDays,
                IsActive = c.IsActive
            }).ToList();

            return ApiResponse<List<LeaveTypeConfigDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LeaveTypeConfigDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeaveBalanceDto>>> GetUserLeaveBalancesAsync(Guid userId)
    {
        try
        {
            var calendar = await GetActiveCalendarAsync();
            if (calendar is null)
                return ApiResponse<List<LeaveBalanceDto>>.FailResponse("No active leave calendar found.");

            var balances = await ((Repositories.LeaveBalanceRepository)_unitOfWork.LeaveBalances)
                .GetByUserAsync(userId, calendar.Id);

            var dtos = balances.Select(b => new LeaveBalanceDto
            {
                Id = b.Id,
                LeaveTypeId = b.LeaveTypeId,
                LeaveTypeName = b.LeaveType?.Name ?? "",
                AllocatedDays = b.AllocatedDays,
                UsedDays = b.UsedDays,
                PendingDays = b.PendingDays,
                RemainingDays = b.AllocatedDays - b.UsedDays
            }).ToList();

            return ApiResponse<List<LeaveBalanceDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LeaveBalanceDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> InitializeLeaveBalancesAsync(Guid calendarId)
    {
        try
        {
            var configs = await ((Repositories.LeaveTypeConfigRepository)_unitOfWork.LeaveTypeConfigs)
                .GetByCalendarAsync(calendarId);

            var teachers = await _unitOfWork.Teachers.FindAsync(t => !t.IsDeleted);
            var employees = await _unitOfWork.Employees.FindAsync(e => !e.IsDeleted);

            var users = new List<(Guid UserId, string Type, string Gender)>();
            foreach (var t in teachers)
                users.Add((t.UserId, "Teacher", t.Gender.ToString()));
            foreach (var e in employees)
                users.Add((e.UserId, "Employee", e.Gender.ToString()));

            var count = 0;
            foreach (var (userId, userType, gender) in users)
            {
                foreach (var config in configs.Where(c => c.IsActive))
                {
                    var applicable = config.ApplicableUserType == "Both" || config.ApplicableUserType == userType;
                    var genderOk = config.ApplicableGender == Gender.Male || config.ApplicableGender.ToString() == gender || config.ApplicableGender.ToString() == "Other";

                    if (!applicable || !genderOk) continue;

                    var existing = await ((Repositories.LeaveBalanceRepository)_unitOfWork.LeaveBalances)
                        .GetByUserAndTypeAsync(userId, config.LeaveTypeId, calendarId);

                    if (existing is null)
                    {
                        await _unitOfWork.LeaveBalances.AddAsync(new LeaveBalance
                        {
                            UserId = userId,
                            LeaveTypeId = config.LeaveTypeId,
                            LeaveCalendarId = calendarId,
                            AllocatedDays = config.TotalDays,
                            UsedDays = 0,
                            PendingDays = 0
                        });
                        count++;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse($"Leave balances initialized for {count} records.");
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<LeaveRequestDto>>> GetAllLeaveRequestsAsync(PaginationQuery query)
    {
        try
        {
            var requests = await ((Repositories.LeaveRequestRepository)_unitOfWork.LeaveRequests)
                .GetAllWithDetailsAsync();

            var totalCount = requests.Count;
            var items = requests.Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize).ToList();

            var dtos = items.Select(r =>
            {
                var profile = GetUserProfileAsync(r.UserId).Result;
                return new LeaveRequestDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = profile?.Name ?? "",
                    UserType = profile?.Type ?? "",
                    LeaveTypeId = r.LeaveTypeId,
                    LeaveTypeName = r.LeaveType?.Name ?? "",
                    LeaveTypeCode = r.LeaveType?.Code ?? "",
                    FromDate = r.FromDate,
                    ToDate = r.ToDate,
                    TotalDays = r.TotalDays,
                    Reason = r.Reason,
                    AttachmentPath = r.AttachmentPath,
                    Status = r.Status,
                    ApprovedBy = r.ApprovedBy,
                    ApprovedAt = r.ApprovedAt,
                    RejectedBy = r.RejectedBy,
                    RejectedAt = r.RejectedAt,
                    AdminReason = r.AdminReason,
                    CreatedAt = r.CreatedAt
                };
            }).ToList();

            var result = new PagedResult<LeaveRequestDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return ApiResponse<PagedResult<LeaveRequestDto>>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<LeaveRequestDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<LeaveRequestDto>> ApproveLeaveAsync(Guid requestId, ApproveLeaveDto dto)
    {
        try
        {
            var request = await ((Repositories.LeaveRequestRepository)_unitOfWork.LeaveRequests)
                .GetByIdWithDetailsAsync(requestId);

            if (request is null)
                return ApiResponse<LeaveRequestDto>.FailResponse("Leave request not found.");

            if (request.Status != LeaveStatus.Pending)
                return ApiResponse<LeaveRequestDto>.FailResponse("Only pending requests can be approved.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                request.Status = LeaveStatus.Approved;
                request.ApprovedBy = _currentUserService.UserId;
                request.ApprovedAt = DateTime.UtcNow;
                request.AdminReason = dto.AdminReason;
                request.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.LeaveRequests.UpdateAsync(request);

                var balance = await ((Repositories.LeaveBalanceRepository)_unitOfWork.LeaveBalances)
                    .GetByUserAndTypeAsync(request.UserId, request.LeaveTypeId, request.LeaveCalendarId);

                if (balance is not null)
                {
                    balance.UsedDays += request.TotalDays;
                    balance.PendingDays = Math.Max(0, balance.PendingDays - request.TotalDays);
                    balance.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.LeaveBalances.UpdateAsync(balance);
                }

                foreach (var day in request.LeaveRequestDays)
                {
                    day.Status = LeaveStatus.Approved;
                    await _unitOfWork.LeaveRequestDays.UpdateAsync(day);

                    var teacher = (await _unitOfWork.Teachers.FindAsync(t => t.UserId == request.UserId && !t.IsDeleted)).FirstOrDefault();
                    var employee = (await _unitOfWork.Employees.FindAsync(e => e.UserId == request.UserId && !t.IsDeleted)).FirstOrDefault();

                    var existingAttendance = (await _unitOfWork.Attendances.FindAsync(a =>
                        a.Date.Date == day.LeaveDate.Date &&
                        ((teacher != null && a.TeacherId == teacher.Id) || (employee != null && a.EmployeeId == employee.Id)) &&
                        !a.IsDeleted)).FirstOrDefault();

                    if (existingAttendance is not null)
                    {
                        existingAttendance.Status = AttendanceStatus.Leave;
                        existingAttendance.LeaveRequestId = request.Id;
                        existingAttendance.LeaveTypeId = request.LeaveTypeId;
                        existingAttendance.LeaveReason = request.Reason;
                        existingAttendance.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.Attendances.UpdateAsync(existingAttendance);
                    }
                    else
                    {
                        var schoolId = await ResolveSchoolIdAsync();
                        if (schoolId is not null)
                        {
                            await _unitOfWork.Attendances.AddAsync(new Domain.Entities.Attendance.Attendance
                            {
                                Date = day.LeaveDate,
                                Status = AttendanceStatus.Leave,
                                TeacherId = teacher?.Id,
                                EmployeeId = employee?.Id,
                                SchoolId = schoolId.Value,
                                LeaveRequestId = request.Id,
                                LeaveTypeId = request.LeaveTypeId,
                                LeaveReason = request.Reason,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            var profile = await GetUserProfileAsync(request.UserId);
            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(request.LeaveTypeId);

            return ApiResponse<LeaveRequestDto>.SuccessResponse(new LeaveRequestDto
            {
                Id = request.Id,
                UserId = request.UserId,
                UserName = profile?.Name ?? "",
                UserType = profile?.Type ?? "",
                LeaveTypeId = request.LeaveTypeId,
                LeaveTypeName = leaveType?.Name ?? "",
                LeaveTypeCode = leaveType?.Code ?? "",
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = request.TotalDays,
                Reason = request.Reason,
                Status = request.Status,
                ApprovedBy = request.ApprovedBy,
                ApprovedAt = request.ApprovedAt,
                AdminReason = request.AdminReason,
                CreatedAt = request.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<LeaveRequestDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<LeaveRequestDto>> RejectLeaveAsync(Guid requestId, RejectLeaveDto dto)
    {
        try
        {
            var request = await ((Repositories.LeaveRequestRepository)_unitOfWork.LeaveRequests)
                .GetByIdWithDetailsAsync(requestId);

            if (request is null)
                return ApiResponse<LeaveRequestDto>.FailResponse("Leave request not found.");

            if (request.Status != LeaveStatus.Pending)
                return ApiResponse<LeaveRequestDto>.FailResponse("Only pending requests can be rejected.");

            var balance = await ((Repositories.LeaveBalanceRepository)_unitOfWork.LeaveBalances)
                .GetByUserAndTypeAsync(request.UserId, request.LeaveTypeId, request.LeaveCalendarId);

            if (balance is not null)
            {
                balance.PendingDays = Math.Max(0, balance.PendingDays - request.TotalDays);
                balance.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.LeaveBalances.UpdateAsync(balance);
            }

            foreach (var day in request.LeaveRequestDays)
            {
                day.Status = LeaveStatus.Rejected;
                await _unitOfWork.LeaveRequestDays.UpdateAsync(day);
            }

            request.Status = LeaveStatus.Rejected;
            request.RejectedBy = _currentUserService.UserId;
            request.RejectedAt = DateTime.UtcNow;
            request.AdminReason = dto.Reason;
            request.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.LeaveRequests.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();

            var profile = await GetUserProfileAsync(request.UserId);
            var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(request.LeaveTypeId);

            return ApiResponse<LeaveRequestDto>.SuccessResponse(new LeaveRequestDto
            {
                Id = request.Id,
                UserId = request.UserId,
                UserName = profile?.Name ?? "",
                UserType = profile?.Type ?? "",
                LeaveTypeId = request.LeaveTypeId,
                LeaveTypeName = leaveType?.Name ?? "",
                LeaveTypeCode = leaveType?.Code ?? "",
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                TotalDays = request.TotalDays,
                Reason = request.Reason,
                Status = request.Status,
                RejectedBy = request.RejectedBy,
                RejectedAt = request.RejectedAt,
                AdminReason = request.AdminReason,
                CreatedAt = request.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<LeaveRequestDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeaveRequestDto>>> GetPendingRequestsAsync()
    {
        try
        {
            var requests = await ((Repositories.LeaveRequestRepository)_unitOfWork.LeaveRequests)
                .GetAllWithDetailsAsync();

            var pending = requests.Where(r => r.Status == LeaveStatus.Pending).ToList();

            var dtos = pending.Select(r =>
            {
                var profile = GetUserProfileAsync(r.UserId).Result;
                return new LeaveRequestDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = profile?.Name ?? "",
                    UserType = profile?.Type ?? "",
                    LeaveTypeId = r.LeaveTypeId,
                    LeaveTypeName = r.LeaveType?.Name ?? "",
                    LeaveTypeCode = r.LeaveType?.Code ?? "",
                    FromDate = r.FromDate,
                    ToDate = r.ToDate,
                    TotalDays = r.TotalDays,
                    Reason = r.Reason,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                };
            }).ToList();

            return ApiResponse<List<LeaveRequestDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LeaveRequestDto>>.FailResponse(ex.Message);
        }
    }
}
