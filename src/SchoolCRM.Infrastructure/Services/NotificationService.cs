using SchoolCRM.Application.DTOs.Notification;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetNotificationsAsync(
        string userId, PaginationQuery query, bool unreadOnly)
    {
        try
        {
            if (!Guid.TryParse(userId, out var guidUserId))
                return ApiResponse<PagedResult<NotificationDto>>.FailResponse("Invalid user ID.");

            var notifications = await _unitOfWork.Notifications.GetByUserAsync(guidUserId, unreadOnly);
            var totalCount = notifications.Count;

            var pagedItems = notifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type.ToString(),
                    IsRead = n.IsRead,
                    Link = n.Link,
                    CreatedAt = n.CreatedAt,
                    ReadAt = n.ReadAt
                }).ToList();

            var pagedResult = new PagedResult<NotificationDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return ApiResponse<PagedResult<NotificationDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<NotificationDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var guidUserId))
                return ApiResponse<int>.FailResponse("Invalid user ID.");

            var count = await _unitOfWork.Notifications.GetUnreadCountAsync(guidUserId);
            return ApiResponse<int>.SuccessResponse(count);
        }
        catch (Exception ex)
        {
            return ApiResponse<int>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> MarkAsReadAsync(Guid notificationId)
    {
        try
        {
            var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
            if (notification is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Notifications.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> MarkAllAsReadAsync(string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out var guidUserId))
                return ApiResponse.FailResponse("Invalid user ID.");

            var notifications = await _unitOfWork.Notifications.GetByUserAsync(guidUserId, true);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Notifications.UpdateAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync();
            return ApiResponse.SuccessResponse(ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }
}
