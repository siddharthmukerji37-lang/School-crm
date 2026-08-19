using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class SalaryDeductionService : ISalaryDeductionService
{
    private readonly IUnitOfWork _unitOfWork;

    public SalaryDeductionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<SalaryDeductionDto>>> GetDeductionsAsync(
        int? month, int? year, string? status, int pageNumber, int pageSize)
    {
        var query = (await _unitOfWork.SalaryDeductions.GetAllAsync()).AsEnumerable();

        if (month.HasValue && year.HasValue)
            query = query.Where(d => d.PayrollMonth == month.Value && d.PayrollYear == year.Value);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<SalaryDeductionStatus>(status, true, out var statusEnum))
            query = query.Where(d => d.Status == statusEnum);

        var items = query.OrderByDescending(d => d.CreatedAt).ToList();
        var totalCount = items.Count;
        var pagedItems = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var result = new PagedResult<SalaryDeductionDto>
        {
            Items = pagedItems.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<SalaryDeductionDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<SalaryDeductionDto>> ApproveDeductionAsync(Guid id, ApproveDeductionDto dto, string approvedBy)
    {
        var deduction = await _unitOfWork.SalaryDeductions.GetByIdAsync(id);
        if (deduction is null)
            return ApiResponse<SalaryDeductionDto>.FailResponse("Deduction not found.");

        if (deduction.Status != SalaryDeductionStatus.Pending)
            return ApiResponse<SalaryDeductionDto>.FailResponse("Only pending deductions can be approved.");

        deduction.Status = SalaryDeductionStatus.Approved;
        deduction.ApprovedBy = approvedBy;
        deduction.ApprovedAt = DateTime.UtcNow;
        deduction.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SalaryDeductions.UpdateAsync(deduction);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<SalaryDeductionDto>.SuccessResponse(MapToDto(deduction));
    }

    public async Task<ApiResponse<SalaryDeductionDto>> RejectDeductionAsync(Guid id, ApproveDeductionDto dto, string rejectedBy)
    {
        var deduction = await _unitOfWork.SalaryDeductions.GetByIdAsync(id);
        if (deduction is null)
            return ApiResponse<SalaryDeductionDto>.FailResponse("Deduction not found.");

        if (deduction.Status != SalaryDeductionStatus.Pending)
            return ApiResponse<SalaryDeductionDto>.FailResponse("Only pending deductions can be rejected.");

        deduction.Status = SalaryDeductionStatus.Rejected;
        deduction.ApprovedBy = rejectedBy;
        deduction.ApprovedAt = DateTime.UtcNow;
        deduction.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SalaryDeductions.UpdateAsync(deduction);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<SalaryDeductionDto>.SuccessResponse(MapToDto(deduction));
    }

    public async Task<ApiResponse<PagedResult<SalaryDeductionDto>>> GetUserDeductionsAsync(Guid userId, int pageNumber, int pageSize)
    {
        var items = await _unitOfWork.SalaryDeductions.GetByUserAsync(userId);
        var pagedItems = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var result = new PagedResult<SalaryDeductionDto>
        {
            Items = pagedItems.Select(MapToDto).ToList(),
            TotalCount = items.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<SalaryDeductionDto>>.SuccessResponse(result);
    }

    private static SalaryDeductionDto MapToDto(Domain.Entities.Attendance.SalaryDeduction d)
    {
        return new SalaryDeductionDto
        {
            Id = d.Id,
            UserId = d.UserId,
            UserName = d.User?.FirstName + " " + d.User?.LastName ?? "",
            UserRole = "",
            AttendanceId = d.AttendanceId,
            AttendanceDate = d.Attendance?.Date ?? DateTime.MinValue,
            LateMinutes = d.Attendance?.LateMinutes ?? 0,
            LateCountMonth = 0,
            AllowedLateCount = 6,
            DeductionType = d.DeductionType,
            DeductionAmount = d.DeductionAmount,
            Reason = d.Reason,
            Status = d.Status,
            ApprovedBy = d.ApprovedBy,
            ApprovedAt = d.ApprovedAt,
            PayrollMonth = d.PayrollMonth,
            PayrollYear = d.PayrollYear,
            CreatedAt = d.CreatedAt
        };
    }
}
