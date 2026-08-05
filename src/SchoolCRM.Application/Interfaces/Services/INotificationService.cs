using SchoolCRM.Application.DTOs.Notification;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface INotificationService
{
    Task<ApiResponse<PagedResult<NotificationDto>>> GetNotificationsAsync(
        string userId, PaginationQuery query, bool unreadOnly);

    Task<ApiResponse<int>> GetUnreadCountAsync(string userId);

    Task<ApiResponse> MarkAsReadAsync(Guid notificationId);

    Task<ApiResponse> MarkAllAsReadAsync(string userId);
}
