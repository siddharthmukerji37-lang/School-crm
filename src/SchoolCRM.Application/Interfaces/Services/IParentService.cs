using SchoolCRM.Application.DTOs.Parent;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IParentService
{
    Task<ApiResponse<PagedResult<ParentDto>>> GetParentsAsync(PaginationQuery query);

    Task<ApiResponse<ParentDto>> GetParentByIdAsync(Guid id);

    Task<ApiResponse<ParentDto>> CreateParentAsync(CreateParentDto dto);

    Task<ApiResponse<ParentDto>> UpdateParentAsync(Guid id, UpdateParentDto dto);

    Task<ApiResponse> DeleteParentAsync(Guid id);
}
