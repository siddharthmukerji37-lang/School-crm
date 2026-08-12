using SchoolCRM.Application.DTOs.Notification;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface INotificationService
{
    Task<ApiResponse<PagedResult<NotificationDto>>> GetNotificationsAsync(
        string userId, PaginationQuery query, bool unreadOnly);

    Task<ApiResponse<int>> GetUnreadCountAsync(string userId);

    Task<ApiResponse> MarkAsReadAsync(Guid notificationId);

    Task<ApiResponse> MarkAllAsReadAsync(string userId);

    Task<ApiResponse> SendAsync(SendNotificationDto dto);

    Task NotifyUsersAsync(IEnumerable<Guid> userIds, string title, string message,
        NotificationType type, string? link = null);

    Task NotifyAdminsAsync(string title, string message,
        NotificationType type = NotificationType.Info, string? link = null);

    Task NotifyStudentsOfClassAsync(Guid classRoomId, string title, string message,
        NotificationType type = NotificationType.Info, Guid? sectionId = null, string? link = null);
}
