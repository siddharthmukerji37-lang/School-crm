using SchoolCRM.Application.DTOs.Fee;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IFeeService
{
    Task<ApiResponse<PagedResult<FeeStructureDto>>> GetFeeStructuresAsync(
        PaginationQuery query, Guid? classRoomId);

    Task<ApiResponse<FeeStructureDto>> GetFeeStructureByIdAsync(Guid id);

    Task<ApiResponse<FeeStructureDto>> CreateFeeStructureAsync(FeeStructureDto dto);

    Task<ApiResponse<FeeStructureDto>> UpdateFeeStructureAsync(Guid id, FeeStructureDto dto);

    Task<ApiResponse> DeleteFeeStructureAsync(Guid id);

    Task<ApiResponse<List<FeeInstallmentDto>>> GetInstallmentsAsync(Guid feeStructureId);

    Task<ApiResponse<FeeInstallmentDto>> CreateInstallmentAsync(FeeInstallmentDto dto);

    Task<ApiResponse<FeeInstallmentDto>> UpdateInstallmentAsync(Guid id, FeeInstallmentDto dto);

    Task<ApiResponse> DeleteInstallmentAsync(Guid id);

    Task<ApiResponse<FeeReceiptDto>> CollectFeeAsync(CollectFeeDto dto);

    Task<ApiResponse<PagedResult<FeeReceiptDto>>> GetFeeReceiptsAsync(
        PaginationQuery query, Guid? studentId, DateTime? fromDate, DateTime? toDate);

    Task<ApiResponse<PagedResult<FeeReceiptDto>>> GetMyFeeReceiptsAsync(PaginationQuery query);

    Task<ApiResponse<FeeReceiptDto>> GetFeeReceiptByIdAsync(Guid id);

    Task<ApiResponse<FeeSummaryDto>> GetFeeSummaryAsync(Guid studentId);

    Task<ApiResponse<FeeSummaryDto>> GetClassFeeSummaryAsync(Guid classRoomId);
}
