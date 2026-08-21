using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.DTOs;
using SchoolCRM.Application.DTOs.Payroll;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Attendance;
using SchoolCRM.Domain.Entities.Employee;
using SchoolCRM.Domain.Entities.Leave;
using SchoolCRM.Domain.Entities.Payroll;
using SchoolCRM.Domain.Entities.Teacher;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Infrastructure.Repositories;
using SchoolCRM.Shared.Models;
using SchoolCRM.Application.Interfaces;

namespace SchoolCRM.Infrastructure.Services;

public class PayrollService : IPayrollService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ApplicationDbContext _context;

    public PayrollService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _context = context;
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

    private async Task<(Guid? TeacherId, Guid? EmployeeId, string UserType, string EmpCode)> ResolveCurrentUserTypeAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid))
            return (null, null, "", "");
        var teacher = (await _unitOfWork.Teachers.FindAsync(t => t.UserId == userGuid && !t.IsDeleted)).FirstOrDefault();
        if (teacher is not null) return (teacher.Id, null, "Teacher", teacher.EmployeeCode ?? "");
        var employee = (await _unitOfWork.Employees.FindAsync(e => e.UserId == userGuid && !e.IsDeleted)).FirstOrDefault();
        if (employee is not null) return (null, employee.Id, "Employee", employee.EmployeeCode ?? "");
        return (null, null, "", "");
    }

    private static decimal CalculateDailySalary(SalaryProfile profile, PayrollSetting setting)
    {
        var gross = profile.BasicSalary + profile.Allowances;
        var divisor = profile.PayrollDivisor ?? setting.PayrollDivisor;
        if (divisor <= 0) divisor = 30;
        return gross / divisor;
    }

    private async Task<SchoolCRM.Domain.Entities.Identity.ApplicationUser?> FindUserByIdAsync(string userId)
    {
        if (!Guid.TryParse(userId, out var userGuid)) return null;
        return await _context.Set<SchoolCRM.Domain.Entities.Identity.ApplicationUser>()
            .Where(u => u.Id == userGuid).FirstOrDefaultAsync();
    }

    public async Task<ApiResponse<PayrollSettingDto>> GetPayrollSettingsAsync()
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null)
                return ApiResponse<PayrollSettingDto>.FailResponse("Unable to determine school.");

            var setting = await ((PayrollSettingRepository)_unitOfWork.PayrollSettings).GetActiveAsync(schoolId.Value);
            if (setting is null)
            {
                setting = new PayrollSetting
                {
                    AllowedLateCount = 6,
                    LateDeductionEnabled = true,
                    LateDeductionType = DeductionType.FixedAmount,
                    LateDeductionAmount = 500,
                    PayrollDivisor = 30,
                    RequireAccountApproval = true,
                    IsActive = true,
                    SchoolId = schoolId.Value,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.PayrollSettings.AddAsync(setting);
                await _unitOfWork.SaveChangesAsync();
            }

            return ApiResponse<PayrollSettingDto>.SuccessResponse(new PayrollSettingDto
            {
                Id = setting.Id,
                AllowedLateCount = setting.AllowedLateCount,
                LateDeductionEnabled = setting.LateDeductionEnabled,
                LateDeductionType = (int)setting.LateDeductionType,
                LateDeductionAmount = setting.LateDeductionAmount,
                PayrollDivisor = setting.PayrollDivisor,
                RequireAccountApproval = setting.RequireAccountApproval
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PayrollSettingDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PayrollSettingDto>> SavePayrollSettingsAsync(CreatePayrollSettingDto dto)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null)
                return ApiResponse<PayrollSettingDto>.FailResponse("Unable to determine school.");

            var setting = await ((PayrollSettingRepository)_unitOfWork.PayrollSettings).GetActiveAsync(schoolId.Value);
            if (setting is null)
            {
                setting = new PayrollSetting { SchoolId = schoolId.Value, CreatedAt = DateTime.UtcNow };
                await _unitOfWork.PayrollSettings.AddAsync(setting);
            }

            setting.AllowedLateCount = dto.AllowedLateCount;
            setting.LateDeductionEnabled = dto.LateDeductionEnabled;
            setting.LateDeductionType = (DeductionType)dto.LateDeductionType;
            setting.LateDeductionAmount = dto.LateDeductionAmount;
            setting.PayrollDivisor = dto.PayrollDivisor;
            setting.RequireAccountApproval = dto.RequireAccountApproval;
            setting.IsActive = true;
            setting.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<PayrollSettingDto>.SuccessResponse(new PayrollSettingDto
            {
                Id = setting.Id,
                AllowedLateCount = setting.AllowedLateCount,
                LateDeductionEnabled = setting.LateDeductionEnabled,
                LateDeductionType = (int)setting.LateDeductionType,
                LateDeductionAmount = setting.LateDeductionAmount,
                PayrollDivisor = setting.PayrollDivisor,
                RequireAccountApproval = setting.RequireAccountApproval
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PayrollSettingDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SalaryProfileDto>> GetMySalaryProfileAsync()
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            return ApiResponse<SalaryProfileDto>.FailResponse("User not found.");
        return await GetSalaryProfileAsync(userId);
    }

    public async Task<ApiResponse<SalaryProfileDto>> GetSalaryProfileAsync(string userId)
    {
        try
        {
            var profile = await ((SalaryProfileRepository)_unitOfWork.SalaryProfiles).GetByUserIdAsync(userId);
            if (profile is null)
                return ApiResponse<SalaryProfileDto>.FailResponse("No salary profile found.");

            ApplicationUser? user = null;
            if (Guid.TryParse(userId, out var userGuid))
            {
                user = await _context.Set<SchoolCRM.Domain.Entities.Identity.ApplicationUser>()
                    .Where(u => u.Id == userGuid).FirstOrDefaultAsync();
            }
            var (_, _, userType, empCode) = await ResolveCurrentUserTypeAsync(userId);

            return ApiResponse<SalaryProfileDto>.SuccessResponse(new SalaryProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "",
                UserType = userType,
                BasicSalary = profile.BasicSalary,
                Allowances = profile.Allowances,
                EffectiveFrom = profile.EffectiveFrom,
                PayrollDivisor = profile.PayrollDivisor,
                IsActive = profile.IsActive,
                BankName = profile.BankName,
                BankAccountNumber = profile.BankAccountNumber,
                BankIFSC = profile.BankIFSC
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<SalaryProfileDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<SalaryProfileDto>>> GetAllSalaryProfilesAsync()
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null)
                return ApiResponse<List<SalaryProfileDto>>.FailResponse("Unable to determine school.");

            var profiles = await ((SalaryProfileRepository)_unitOfWork.SalaryProfiles).GetBySchoolAsync(schoolId.Value);
            var dtos = new List<SalaryProfileDto>();

            var userIds = profiles
                .Where(p => Guid.TryParse(p.UserId, out _))
                .Select(p => Guid.Parse(p.UserId))
                .ToList();

            var users = await _context.Set<SchoolCRM.Domain.Entities.Identity.ApplicationUser>()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            foreach (var profile in profiles)
            {
                ApplicationUser? user = null;
                if (Guid.TryParse(profile.UserId, out var userGuid))
                    users.TryGetValue(userGuid, out user);

                var (_, _, userType, _) = await ResolveCurrentUserTypeAsync(profile.UserId);
                dtos.Add(new SalaryProfileDto
                {
                    Id = profile.Id,
                    UserId = profile.UserId,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "N/A",
                    UserType = userType,
                    BasicSalary = profile.BasicSalary,
                    Allowances = profile.Allowances,
                    EffectiveFrom = profile.EffectiveFrom,
                    PayrollDivisor = profile.PayrollDivisor,
                    IsActive = profile.IsActive,
                    BankName = profile.BankName,
                    BankAccountNumber = profile.BankAccountNumber,
                    BankIFSC = profile.BankIFSC
                });
            }

            return ApiResponse<List<SalaryProfileDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<SalaryProfileDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SalaryProfileDto>> CreateSalaryProfileAsync(CreateSalaryProfileDto dto)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null)
                return ApiResponse<SalaryProfileDto>.FailResponse("Unable to determine school.");

            var existing = await ((SalaryProfileRepository)_unitOfWork.SalaryProfiles).GetByUserIdAsync(dto.UserId);
            if (existing is not null)
                return ApiResponse<SalaryProfileDto>.FailResponse("Salary profile already exists for this user.");

            var profile = new SalaryProfile
            {
                UserId = dto.UserId,
                BasicSalary = dto.BasicSalary,
                Allowances = dto.Allowances,
                EffectiveFrom = dto.EffectiveFrom,
                PayrollDivisor = dto.PayrollDivisor,
                IsActive = dto.IsActive,
                BankName = dto.BankName,
                BankAccountNumber = dto.BankAccountNumber,
                BankIFSC = dto.BankIFSC,
                SchoolId = schoolId.Value,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SalaryProfiles.AddAsync(profile);
            await _unitOfWork.SaveChangesAsync();

            var user = await FindUserByIdAsync(dto.UserId);
            var (_, _, userType, _) = await ResolveCurrentUserTypeAsync(dto.UserId);

            return ApiResponse<SalaryProfileDto>.SuccessResponse(new SalaryProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "",
                UserType = userType,
                BasicSalary = profile.BasicSalary,
                Allowances = profile.Allowances,
                EffectiveFrom = profile.EffectiveFrom,
                PayrollDivisor = profile.PayrollDivisor,
                IsActive = profile.IsActive,
                BankName = profile.BankName,
                BankAccountNumber = profile.BankAccountNumber,
                BankIFSC = profile.BankIFSC
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<SalaryProfileDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SalaryProfileDto>> UpdateSalaryProfileAsync(Guid id, CreateSalaryProfileDto dto)
    {
        try
        {
            var profile = await _unitOfWork.SalaryProfiles.GetByIdAsync(id);
            if (profile is null)
                return ApiResponse<SalaryProfileDto>.FailResponse("Salary profile not found.");

            profile.BasicSalary = dto.BasicSalary;
            profile.Allowances = dto.Allowances;
            profile.EffectiveFrom = dto.EffectiveFrom;
            profile.PayrollDivisor = dto.PayrollDivisor;
            profile.IsActive = dto.IsActive;
            profile.BankName = dto.BankName;
            profile.BankAccountNumber = dto.BankAccountNumber;
            profile.BankIFSC = dto.BankIFSC;
            profile.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SalaryProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync();

            var user = await FindUserByIdAsync(profile.UserId);
            var (_, _, userType, _) = await ResolveCurrentUserTypeAsync(profile.UserId);

            return ApiResponse<SalaryProfileDto>.SuccessResponse(new SalaryProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "",
                UserType = userType,
                BasicSalary = profile.BasicSalary,
                Allowances = profile.Allowances,
                EffectiveFrom = profile.EffectiveFrom,
                PayrollDivisor = profile.PayrollDivisor,
                IsActive = profile.IsActive,
                BankName = profile.BankName,
                BankAccountNumber = profile.BankAccountNumber,
                BankIFSC = profile.BankIFSC
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<SalaryProfileDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<SalaryComponentDto>>> GetSalaryComponentsAsync(Guid profileId)
    {
        try
        {
            var components = await ((SalaryComponentRepository)_unitOfWork.SalaryComponents).GetByProfileAsync(profileId);
            var dtos = components.Select(c => new SalaryComponentDto
            {
                Id = c.Id,
                SalaryProfileId = c.SalaryProfileId,
                ComponentName = c.ComponentName,
                ComponentType = c.ComponentType,
                Amount = c.Amount,
                IsActive = c.IsActive
            }).ToList();
            return ApiResponse<List<SalaryComponentDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<SalaryComponentDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SalaryComponentDto>> AddSalaryComponentAsync(Guid profileId, CreateSalaryComponentDto dto)
    {
        try
        {
            var component = new SalaryComponent
            {
                SalaryProfileId = profileId,
                ComponentName = dto.ComponentName,
                ComponentType = dto.ComponentType,
                Amount = dto.Amount,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.SalaryComponents.AddAsync(component);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SalaryComponentDto>.SuccessResponse(new SalaryComponentDto
            {
                Id = component.Id,
                SalaryProfileId = component.SalaryProfileId,
                ComponentName = component.ComponentName,
                ComponentType = component.ComponentType,
                Amount = component.Amount,
                IsActive = component.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<SalaryComponentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SalaryComponentDto>> UpdateSalaryComponentAsync(Guid id, CreateSalaryComponentDto dto)
    {
        try
        {
            var component = await _unitOfWork.SalaryComponents.GetByIdAsync(id);
            if (component is null)
                return ApiResponse<SalaryComponentDto>.FailResponse("Component not found.");

            component.ComponentName = dto.ComponentName;
            component.ComponentType = dto.ComponentType;
            component.Amount = dto.Amount;
            component.IsActive = dto.IsActive;
            component.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SalaryComponents.UpdateAsync(component);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<SalaryComponentDto>.SuccessResponse(new SalaryComponentDto
            {
                Id = component.Id,
                SalaryProfileId = component.SalaryProfileId,
                ComponentName = component.ComponentName,
                ComponentType = component.ComponentType,
                Amount = component.Amount,
                IsActive = component.IsActive
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<SalaryComponentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteSalaryComponentAsync(Guid id)
    {
        try
        {
            var component = await _unitOfWork.SalaryComponents.GetByIdAsync(id);
            if (component is null)
                return ApiResponse.FailResponse("Component not found.");

            component.IsDeleted = true;
            component.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SalaryComponents.UpdateAsync(component);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse("Component deleted.");
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<PayrollDto>>> GenerateMonthlyPayrollAsync(GeneratePayrollDto dto)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null)
                return ApiResponse<List<PayrollDto>>.FailResponse("Unable to determine school.");

            var setting = await ((PayrollSettingRepository)_unitOfWork.PayrollSettings).GetActiveAsync(schoolId.Value);
            if (setting is null)
                return ApiResponse<List<PayrollDto>>.FailResponse("Payroll settings not configured.");

            var profiles = await ((SalaryProfileRepository)_unitOfWork.SalaryProfiles).GetBySchoolAsync(schoolId.Value);
            var createdPayrolls = new List<PayrollDto>();

            foreach (var profile in profiles)
            {
                if (!Guid.TryParse(profile.UserId, out _)) continue;

                var existing = await ((PayrollRepository)_unitOfWork.Payrolls)
                    .GetByUserAndMonthAsync(profile.UserId, dto.Month, dto.Year);
                if (existing is not null) continue;

                var (teacherId, employeeId, userType, empCode) = await ResolveCurrentUserTypeAsync(profile.UserId);
                var dailySalary = CalculateDailySalary(profile, setting);
                var grossSalary = profile.BasicSalary + profile.Allowances;

                int lateCount = 0;
                if (teacherId.HasValue)
                {
                    lateCount = (await _unitOfWork.Attendances.FindAsync(a =>
                        a.TeacherId == teacherId.Value &&
                        a.Date.Month == dto.Month && a.Date.Year == dto.Year &&
                        a.Status == AttendanceStatus.Late && !a.IsDeleted)).Count;
                }
                else if (employeeId.HasValue)
                {
                    lateCount = (await _unitOfWork.Attendances.FindAsync(a =>
                        a.EmployeeId == employeeId.Value &&
                        a.Date.Month == dto.Month && a.Date.Year == dto.Year &&
                        a.Status == AttendanceStatus.Late && !a.IsDeleted)).Count;
                }

                decimal lateDeduction = 0;
                if (setting.LateDeductionEnabled && lateCount > setting.AllowedLateCount)
                {
                    var extraLate = lateCount - setting.AllowedLateCount;
                    lateDeduction = extraLate * setting.LateDeductionAmount;
                }

                var userGuid = Guid.Parse(profile.UserId);
                var approvedLeaves = (await _unitOfWork.LeaveRequests.FindAsync(lr =>
                    lr.UserId == userGuid &&
                    lr.Status == LeaveStatus.Approved &&
                    lr.FromDate.Month == dto.Month && lr.FromDate.Year == dto.Year &&
                    !lr.IsDeleted)).ToList();

                int unpaidLeaveDays = 0;
                int paidLeaveDays = 0;

                foreach (var leaveReq in approvedLeaves)
                {
                    var config = (await _unitOfWork.LeaveTypeConfigs.FindAsync(c =>
                        c.LeaveTypeId == leaveReq.LeaveTypeId && c.LeaveCalendarId == leaveReq.LeaveCalendarId && !c.IsDeleted)).FirstOrDefault();
                    if (config is not null && !config.IsPaid)
                    {
                        unpaidLeaveDays += leaveReq.TotalDays;
                    }
                    else
                    {
                        paidLeaveDays += leaveReq.TotalDays;
                    }
                }

                var unpaidLeaveDeduction = unpaidLeaveDays * dailySalary;

                var gross = grossSalary;
                var totalDeductions = lateDeduction + unpaidLeaveDeduction;
                var netSalary = gross - totalDeductions;

                var payroll = new Domain.Entities.Payroll.Payroll
                {
                    UserId = profile.UserId,
                    PayrollMonth = dto.Month,
                    PayrollYear = dto.Year,
                    BasicSalary = profile.BasicSalary,
                    TotalAllowances = profile.Allowances,
                    GrossSalary = gross,
                    LateCount = lateCount,
                    AllowedLateCount = setting.AllowedLateCount,
                    LateDeduction = lateDeduction,
                    PaidLeaveDays = paidLeaveDays,
                    UnpaidLeaveDays = unpaidLeaveDays,
                    UnpaidLeaveDeduction = unpaidLeaveDeduction,
                    OtherDeductions = 0,
                    TotalDeductions = totalDeductions,
                    NetSalary = netSalary,
                    DailySalary = dailySalary,
                    PayrollDivisor = profile.PayrollDivisor ?? setting.PayrollDivisor,
                    Status = PayrollStatus.Calculated,
                    CalculatedAt = DateTime.UtcNow,
                    SchoolId = schoolId.Value,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Payrolls.AddAsync(payroll);

                if (lateDeduction > 0)
                {
                    await _unitOfWork.PayrollDeductions.AddAsync(new PayrollDeduction
                    {
                        PayrollId = payroll.Id,
                        DeductionType = "Late",
                        Description = $"Late deduction: {lateCount - setting.AllowedLateCount} late arrivals beyond {setting.AllowedLateCount} allowed",
                        Days = lateCount - setting.AllowedLateCount,
                        Amount = lateDeduction,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (unpaidLeaveDeduction > 0)
                {
                    await _unitOfWork.PayrollDeductions.AddAsync(new PayrollDeduction
                    {
                        PayrollId = payroll.Id,
                        DeductionType = "UnpaidLeave",
                        Description = $"Unpaid leave deduction: {unpaidLeaveDays} days",
                        Days = unpaidLeaveDays,
                        Amount = unpaidLeaveDeduction,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                var user = await FindUserByIdAsync(profile.UserId);
                var userName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "";

                createdPayrolls.Add(new PayrollDto
                {
                    Id = payroll.Id,
                    UserId = payroll.UserId,
                    UserName = userName,
                    UserType = userType,
                    EmployeeName = empCode,
                    PayrollMonth = payroll.PayrollMonth,
                    PayrollYear = payroll.PayrollYear,
                    BasicSalary = payroll.BasicSalary,
                    TotalAllowances = payroll.TotalAllowances,
                    GrossSalary = payroll.GrossSalary,
                    LateCount = payroll.LateCount,
                    AllowedLateCount = payroll.AllowedLateCount,
                    LateDeduction = payroll.LateDeduction,
                    PaidLeaveDays = payroll.PaidLeaveDays,
                    UnpaidLeaveDays = payroll.UnpaidLeaveDays,
                    UnpaidLeaveDeduction = payroll.UnpaidLeaveDeduction,
                    OtherDeductions = payroll.OtherDeductions,
                    TotalDeductions = payroll.TotalDeductions,
                    NetSalary = payroll.NetSalary,
                    DailySalary = payroll.DailySalary,
                    PayrollDivisor = payroll.PayrollDivisor,
                    Status = (int)payroll.Status,
                    CalculatedAt = payroll.CalculatedAt
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<List<PayrollDto>>.SuccessResponse(createdPayrolls);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PayrollDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PayrollDto>> GetPayrollAsync(Guid id)
    {
        try
        {
            var payroll = await _unitOfWork.Payrolls.GetByIdAsync(id);
            if (payroll is null)
                return ApiResponse<PayrollDto>.FailResponse("Payroll not found.");

            var user = await FindUserByIdAsync(payroll.UserId);
            var (_, _, userType, empCode) = await ResolveCurrentUserTypeAsync(payroll.UserId);

            return ApiResponse<PayrollDto>.SuccessResponse(new PayrollDto
            {
                Id = payroll.Id,
                UserId = payroll.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "",
                UserType = userType,
                EmployeeName = empCode,
                PayrollMonth = payroll.PayrollMonth,
                PayrollYear = payroll.PayrollYear,
                BasicSalary = payroll.BasicSalary,
                TotalAllowances = payroll.TotalAllowances,
                GrossSalary = payroll.GrossSalary,
                LateCount = payroll.LateCount,
                AllowedLateCount = payroll.AllowedLateCount,
                LateDeduction = payroll.LateDeduction,
                DailySalary = payroll.DailySalary,
                PayrollDivisor = payroll.PayrollDivisor,
                PaidLeaveDays = payroll.PaidLeaveDays,
                UnpaidLeaveDays = payroll.UnpaidLeaveDays,
                UnpaidLeaveDeduction = payroll.UnpaidLeaveDeduction,
                OtherDeductions = payroll.OtherDeductions,
                TotalDeductions = payroll.TotalDeductions,
                NetSalary = payroll.NetSalary,
                Status = (int)payroll.Status,
                CalculatedAt = payroll.CalculatedAt,
                ApprovedBy = payroll.ApprovedBy,
                ApprovedAt = payroll.ApprovedAt,
                PaidBy = payroll.PaidBy,
                PaidAt = payroll.PaidAt
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PayrollDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<PayrollDto>>> GetPayrollsAsync(int month, int year)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null)
                return ApiResponse<List<PayrollDto>>.FailResponse("Unable to determine school.");

            var payrolls = await ((PayrollRepository)_unitOfWork.Payrolls).GetByMonthAsync(month, year, schoolId.Value);
            var dtos = new List<PayrollDto>();

            foreach (var p in payrolls)
            {
                var user = await FindUserByIdAsync(p.UserId);
                var (_, _, userType, empCode) = await ResolveCurrentUserTypeAsync(p.UserId);
                dtos.Add(new PayrollDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "",
                    UserType = userType,
                    EmployeeName = empCode,
                    PayrollMonth = p.PayrollMonth,
                    PayrollYear = p.PayrollYear,
                    BasicSalary = p.BasicSalary,
                    TotalAllowances = p.TotalAllowances,
                    GrossSalary = p.GrossSalary,
                    LateCount = p.LateCount,
                    AllowedLateCount = p.AllowedLateCount,
                    LateDeduction = p.LateDeduction,
                    DailySalary = p.DailySalary,
                    PayrollDivisor = p.PayrollDivisor,
                    PaidLeaveDays = p.PaidLeaveDays,
                    UnpaidLeaveDays = p.UnpaidLeaveDays,
                    UnpaidLeaveDeduction = p.UnpaidLeaveDeduction,
                    OtherDeductions = p.OtherDeductions,
                    TotalDeductions = p.TotalDeductions,
                    NetSalary = p.NetSalary,
                    Status = (int)p.Status,
                    CalculatedAt = p.CalculatedAt,
                    ApprovedBy = p.ApprovedBy,
                    ApprovedAt = p.ApprovedAt,
                    PaidBy = p.PaidBy,
                    PaidAt = p.PaidAt
                });
            }

            return ApiResponse<List<PayrollDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PayrollDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PayrollDto>> ApprovePayrollAsync(Guid id)
    {
        try
        {
            var payroll = await _unitOfWork.Payrolls.GetByIdAsync(id);
            if (payroll is null)
                return ApiResponse<PayrollDto>.FailResponse("Payroll not found.");

            if (payroll.Status != PayrollStatus.Calculated && payroll.Status != PayrollStatus.UnderReview)
                return ApiResponse<PayrollDto>.FailResponse("Only calculated or under-review payroll can be approved.");

            payroll.Status = PayrollStatus.Approved;
            payroll.ApprovedBy = _currentUserService.UserId;
            payroll.ApprovedAt = DateTime.UtcNow;
            payroll.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Payrolls.UpdateAsync(payroll);
            await _unitOfWork.SaveChangesAsync();

            var user = await FindUserByIdAsync(payroll.UserId);
            var (_, _, userType, empCode) = await ResolveCurrentUserTypeAsync(payroll.UserId);

            return ApiResponse<PayrollDto>.SuccessResponse(new PayrollDto
            {
                Id = payroll.Id,
                UserId = payroll.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "",
                UserType = userType,
                EmployeeName = empCode,
                PayrollMonth = payroll.PayrollMonth,
                PayrollYear = payroll.PayrollYear,
                BasicSalary = payroll.BasicSalary,
                TotalAllowances = payroll.TotalAllowances,
                GrossSalary = payroll.GrossSalary,
                LateCount = payroll.LateCount,
                AllowedLateCount = payroll.AllowedLateCount,
                LateDeduction = payroll.LateDeduction,
                DailySalary = payroll.DailySalary,
                PayrollDivisor = payroll.PayrollDivisor,
                PaidLeaveDays = payroll.PaidLeaveDays,
                UnpaidLeaveDays = payroll.UnpaidLeaveDays,
                UnpaidLeaveDeduction = payroll.UnpaidLeaveDeduction,
                OtherDeductions = payroll.OtherDeductions,
                TotalDeductions = payroll.TotalDeductions,
                NetSalary = payroll.NetSalary,
                Status = (int)payroll.Status,
                CalculatedAt = payroll.CalculatedAt,
                ApprovedBy = payroll.ApprovedBy,
                ApprovedAt = payroll.ApprovedAt,
                PaidBy = payroll.PaidBy,
                PaidAt = payroll.PaidAt
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PayrollDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PayrollDto>> MarkPaidAsync(Guid id)
    {
        try
        {
            var payroll = await _unitOfWork.Payrolls.GetByIdAsync(id);
            if (payroll is null)
                return ApiResponse<PayrollDto>.FailResponse("Payroll not found.");

            if (payroll.Status != PayrollStatus.Approved && payroll.Status != PayrollStatus.PayslipGenerated)
                return ApiResponse<PayrollDto>.FailResponse("Only approved payroll can be marked as paid.");

            payroll.Status = PayrollStatus.Paid;
            payroll.PaidBy = _currentUserService.UserId;
            payroll.PaidAt = DateTime.UtcNow;
            payroll.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Payrolls.UpdateAsync(payroll);
            await _unitOfWork.SaveChangesAsync();

            var user = await FindUserByIdAsync(payroll.UserId);
            var (_, _, userType, empCode) = await ResolveCurrentUserTypeAsync(payroll.UserId);

            return ApiResponse<PayrollDto>.SuccessResponse(new PayrollDto
            {
                Id = payroll.Id,
                UserId = payroll.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "",
                UserType = userType,
                EmployeeName = empCode,
                PayrollMonth = payroll.PayrollMonth,
                PayrollYear = payroll.PayrollYear,
                BasicSalary = payroll.BasicSalary,
                TotalAllowances = payroll.TotalAllowances,
                GrossSalary = payroll.GrossSalary,
                LateCount = payroll.LateCount,
                AllowedLateCount = payroll.AllowedLateCount,
                LateDeduction = payroll.LateDeduction,
                DailySalary = payroll.DailySalary,
                PayrollDivisor = payroll.PayrollDivisor,
                PaidLeaveDays = payroll.PaidLeaveDays,
                UnpaidLeaveDays = payroll.UnpaidLeaveDays,
                UnpaidLeaveDeduction = payroll.UnpaidLeaveDeduction,
                OtherDeductions = payroll.OtherDeductions,
                TotalDeductions = payroll.TotalDeductions,
                NetSalary = payroll.NetSalary,
                Status = (int)payroll.Status,
                CalculatedAt = payroll.CalculatedAt,
                ApprovedBy = payroll.ApprovedBy,
                ApprovedAt = payroll.ApprovedAt,
                PaidBy = payroll.PaidBy,
                PaidAt = payroll.PaidAt
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PayrollDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PayslipDto>> GeneratePayslipAsync(Guid payrollId)
    {
        try
        {
            var payroll = await _unitOfWork.Payrolls.GetByIdAsync(payrollId);
            if (payroll is null)
                return ApiResponse<PayslipDto>.FailResponse("Payroll not found.");

            if (payroll.Status != PayrollStatus.Approved)
                return ApiResponse<PayslipDto>.FailResponse("Only approved payroll can generate payslip.");

            var existing = await ((PayslipRepository)_unitOfWork.Payslips).GetByPayrollIdAsync(payrollId);
            if (existing is not null)
            {
                return ApiResponse<PayslipDto>.SuccessResponse(new PayslipDto
                {
                    Id = existing.Id,
                    PayrollId = existing.PayrollId,
                    UserId = existing.UserId,
                    PayslipNumber = existing.PayslipNumber,
                    PayrollMonth = existing.PayrollMonth,
                    PayrollYear = existing.PayrollYear,
                    GrossSalary = existing.GrossSalary,
                    TotalDeductions = existing.TotalDeductions,
                    NetSalary = existing.NetSalary,
                    PdfPath = existing.PdfPath,
                    Status = existing.Status,
                    GeneratedBy = existing.GeneratedBy,
                    GeneratedAt = existing.GeneratedAt
                });
            }

            var monthStr = payroll.PayrollMonth.ToString("D2");
            var seq = (await _context.Set<Payslip>()
                .Where(p => p.PayrollMonth == payroll.PayrollMonth && p.PayrollYear == payroll.PayrollYear && !p.IsDeleted)
                .CountAsync()) + 1;

            var payslip = new Payslip
            {
                PayrollId = payroll.Id,
                UserId = payroll.UserId,
                PayslipNumber = $"PS-{payroll.PayrollYear}-{monthStr}-{seq:D3}",
                PayrollMonth = payroll.PayrollMonth,
                PayrollYear = payroll.PayrollYear,
                GrossSalary = payroll.GrossSalary,
                TotalDeductions = payroll.TotalDeductions,
                NetSalary = payroll.NetSalary,
                Status = "Generated",
                GeneratedBy = _currentUserService.UserId,
                GeneratedAt = DateTime.UtcNow,
                SchoolId = payroll.SchoolId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Payslips.AddAsync(payslip);

            payroll.Status = PayrollStatus.PayslipGenerated;
            payroll.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Payrolls.UpdateAsync(payroll);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<PayslipDto>.SuccessResponse(new PayslipDto
            {
                Id = payslip.Id,
                PayrollId = payslip.PayrollId,
                UserId = payslip.UserId,
                PayslipNumber = payslip.PayslipNumber,
                PayrollMonth = payslip.PayrollMonth,
                PayrollYear = payslip.PayrollYear,
                GrossSalary = payslip.GrossSalary,
                TotalDeductions = payslip.TotalDeductions,
                NetSalary = payslip.NetSalary,
                PdfPath = payslip.PdfPath,
                Status = payslip.Status,
                GeneratedBy = payslip.GeneratedBy,
                GeneratedAt = payslip.GeneratedAt
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PayslipDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<PayrollDto>>> GetMyPayrollsAsync()
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return ApiResponse<List<PayrollDto>>.FailResponse("User not found.");

            var payrolls = await ((PayrollRepository)_unitOfWork.Payrolls).GetByUserIdAsync(userId);
            var dtos = payrolls.Select(p => new PayrollDto
            {
                Id = p.Id,
                UserId = p.UserId,
                PayrollMonth = p.PayrollMonth,
                PayrollYear = p.PayrollYear,
                BasicSalary = p.BasicSalary,
                TotalAllowances = p.TotalAllowances,
                GrossSalary = p.GrossSalary,
                LateCount = p.LateCount,
                AllowedLateCount = p.AllowedLateCount,
                LateDeduction = p.LateDeduction,
                PaidLeaveDays = p.PaidLeaveDays,
                UnpaidLeaveDays = p.UnpaidLeaveDays,
                UnpaidLeaveDeduction = p.UnpaidLeaveDeduction,
                OtherDeductions = p.OtherDeductions,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                Status = (int)p.Status,
                PaidAt = p.PaidAt
            }).ToList();

            return ApiResponse<List<PayrollDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PayrollDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PayslipDto>> GetMyPayslipAsync(Guid payrollId)
    {
        try
        {
            var userId = _currentUserService.UserId;
            var payslip = await ((PayslipRepository)_unitOfWork.Payslips).GetByPayrollIdAsync(payrollId);
            if (payslip is null || payslip.UserId != userId)
                return ApiResponse<PayslipDto>.FailResponse("Payslip not found.");

            return ApiResponse<PayslipDto>.SuccessResponse(new PayslipDto
            {
                Id = payslip.Id,
                PayrollId = payslip.PayrollId,
                UserId = payslip.UserId,
                PayslipNumber = payslip.PayslipNumber,
                PayrollMonth = payslip.PayrollMonth,
                PayrollYear = payslip.PayrollYear,
                GrossSalary = payslip.GrossSalary,
                TotalDeductions = payslip.TotalDeductions,
                NetSalary = payslip.NetSalary,
                PdfPath = payslip.PdfPath,
                Status = payslip.Status,
                GeneratedBy = payslip.GeneratedBy,
                GeneratedAt = payslip.GeneratedAt
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PayslipDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<PayslipDto>>> GetMyPayslipsAsync()
    {
        try
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return ApiResponse<List<PayslipDto>>.FailResponse("User not found.");

            var payslips = await ((PayslipRepository)_unitOfWork.Payslips).GetByUserIdAsync(userId);
            var dtos = payslips.Select(p => new PayslipDto
            {
                Id = p.Id,
                PayrollId = p.PayrollId,
                UserId = p.UserId,
                PayslipNumber = p.PayslipNumber,
                PayrollMonth = p.PayrollMonth,
                PayrollYear = p.PayrollYear,
                GrossSalary = p.GrossSalary,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                PdfPath = p.PdfPath,
                Status = p.Status,
                GeneratedBy = p.GeneratedBy,
                GeneratedAt = p.GeneratedAt
            }).ToList();

            return ApiResponse<List<PayslipDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<PayslipDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PayrollReportDto>> GetPayrollReportAsync(int month, int year)
    {
        try
        {
            var schoolId = await ResolveSchoolIdAsync();
            if (schoolId is null)
                return ApiResponse<PayrollReportDto>.FailResponse("Unable to determine school.");

            var payrolls = await ((PayrollRepository)_unitOfWork.Payrolls).GetByMonthAsync(month, year, schoolId.Value);

            var report = new PayrollReportDto
            {
                TotalEmployees = payrolls.Count,
                PayrollGenerated = payrolls.Count,
                PayrollApproved = payrolls.Count(p => p.Status == PayrollStatus.Approved || p.Status == PayrollStatus.PayslipGenerated || p.Status == PayrollStatus.Paid),
                PayrollPending = payrolls.Count(p => p.Status != PayrollStatus.Approved && p.Status != PayrollStatus.PayslipGenerated && p.Status != PayrollStatus.Paid),
                TotalGrossSalary = payrolls.Sum(p => p.GrossSalary),
                TotalLateDeductions = payrolls.Sum(p => p.LateDeduction),
                TotalUnpaidLeaveDeductions = payrolls.Sum(p => p.UnpaidLeaveDeduction),
                TotalOtherDeductions = payrolls.Sum(p => p.OtherDeductions),
                TotalNetPayroll = payrolls.Sum(p => p.NetSalary)
            };

            return ApiResponse<PayrollReportDto>.SuccessResponse(report);
        }
        catch (Exception ex)
        {
            return ApiResponse<PayrollReportDto>.FailResponse(ex.Message);
        }
    }
}
