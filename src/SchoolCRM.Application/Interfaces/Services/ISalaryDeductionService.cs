using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface ISalaryDeductionService
{
    Task<ApiResponse<PagedResult<SalaryDeductionDto>>> GetDeductionsAsync(int? month, int? year, string? status, int pageNumber, int pageSize);
    Task<ApiResponse<SalaryDeductionDto>> ApproveDeductionAsync(Guid id, ApproveDeductionDto dto, string approvedBy);
    Task<ApiResponse<SalaryDeductionDto>> RejectDeductionAsync(Guid id, ApproveDeductionDto dto, string rejectedBy);
    Task<ApiResponse<PagedResult<SalaryDeductionDto>>> GetUserDeductionsAsync(Guid userId, int pageNumber, int pageSize);
}
