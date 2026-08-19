using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Attendance;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class AttendancePolicyService : IAttendancePolicyService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendancePolicyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<AttendancePolicyDto>> GetPolicyAsync(Guid schoolId)
    {
        var results = await _unitOfWork.AttendancePolicies.FindAsync(
            p => p.SchoolId == schoolId && p.IsActive && !p.IsDeleted);
        var policy = results.FirstOrDefault();

        if (policy is null)
        {
            var defaultPolicy = new AttendancePolicy
            {
                SchoolId = schoolId,
                AllowedLateArrivals = 6,
                SalaryDeductionEnabled = true,
                DeductionType = DeductionType.FixedAmount,
                DeductionAmount = 0,
                RequireAdminApproval = true,
                SchoolStartTime = new TimeSpan(9, 30, 0),
                SchoolEndTime = new TimeSpan(18, 30, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AttendancePolicies.AddAsync(defaultPolicy);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<AttendancePolicyDto>.SuccessResponse(MapToDto(defaultPolicy));
        }

        return ApiResponse<AttendancePolicyDto>.SuccessResponse(MapToDto(policy));
    }

    public async Task<ApiResponse<AttendancePolicyDto>> UpdatePolicyAsync(Guid schoolId, UpdateAttendancePolicyDto dto)
    {
        var results = await _unitOfWork.AttendancePolicies.FindAsync(
            p => p.SchoolId == schoolId && p.IsActive && !p.IsDeleted);
        var policy = results.FirstOrDefault();

        if (policy is null)
        {
            policy = new AttendancePolicy
            {
                SchoolId = schoolId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AttendancePolicies.AddAsync(policy);
        }

        policy.AllowedLateArrivals = dto.AllowedLateArrivals;
        policy.SalaryDeductionEnabled = dto.SalaryDeductionEnabled;
        policy.DeductionType = dto.DeductionType;
        policy.DeductionAmount = dto.DeductionAmount;
        policy.RequireAdminApproval = dto.RequireAdminApproval;
        policy.SchoolStartTime = dto.SchoolStartTime;
        policy.SchoolEndTime = dto.SchoolEndTime;
        policy.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.AttendancePolicies.UpdateAsync(policy);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<AttendancePolicyDto>.SuccessResponse(MapToDto(policy));
    }

    public async Task<ApiResponse<PagedResult<AttendanceMonthlySummaryDto>>> GetMonthlySummariesAsync(
        int month, int year, int pageNumber, int pageSize)
    {
        var items = await _unitOfWork.AttendanceMonthlySummaries.GetByMonthAsync(month, year);
        var pagedItems = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var result = new PagedResult<AttendanceMonthlySummaryDto>
        {
            Items = pagedItems.Select(s => new AttendanceMonthlySummaryDto
            {
                Id = s.Id,
                UserId = s.UserId,
                UserName = s.User?.FirstName + " " + s.User?.LastName ?? "",
                Month = s.Month,
                Year = s.Year,
                TotalLateCount = s.TotalLateCount,
                AllowedLateCount = s.AllowedLateCount,
                PolicyExceeded = s.PolicyExceeded,
                SalaryDeductionCount = s.SalaryDeductionCount,
                SalaryDeductionAmount = s.SalaryDeductionAmount
            }).ToList(),
            TotalCount = items.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<AttendanceMonthlySummaryDto>>.SuccessResponse(result);
    }

    private static AttendancePolicyDto MapToDto(AttendancePolicy policy)
    {
        return new AttendancePolicyDto
        {
            Id = policy.Id,
            SchoolId = policy.SchoolId,
            AllowedLateArrivals = policy.AllowedLateArrivals,
            SalaryDeductionEnabled = policy.SalaryDeductionEnabled,
            DeductionType = policy.DeductionType,
            DeductionAmount = policy.DeductionAmount,
            RequireAdminApproval = policy.RequireAdminApproval,
            SchoolStartTime = policy.SchoolStartTime,
            SchoolEndTime = policy.SchoolEndTime,
            IsActive = policy.IsActive
        };
    }
}
