using SchoolCRM.Application.DTOs.Notification;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface INoticeService
{
    Task<ApiResponse<PagedResult<NoticeDto>>> GetNoticesAsync(PaginationQuery query);
    Task<ApiResponse<NoticeDto>> GetNoticeByIdAsync(Guid id);
    Task<ApiResponse<NoticeDto>> CreateNoticeAsync(CreateNoticeDto dto, Guid userId, string createdByName);
    Task<ApiResponse<NoticeDto>> UpdateNoticeAsync(Guid id, UpdateNoticeDto dto);
    Task<ApiResponse> DeleteNoticeAsync(Guid id);
    Task<ApiResponse<List<NoticeDto>>> GetPublishedNoticesAsync();
}
