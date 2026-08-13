using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.DTOs.Notification;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Notification;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Infrastructure.SignalR;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private static readonly string[] AdminRoles =
        { "SuperAdmin", "SchoolAdmin", "Principal", "VicePrincipal" };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        IUnitOfWork unitOfWork,
        ApplicationDbContext dbContext,
        IHubContext<NotificationHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _hubContext = hubContext;
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

    public async Task<ApiResponse> SendAsync(SendNotificationDto dto)
    {
        try
        {
            var type = Enum.TryParse<NotificationType>(dto.Type, ignoreCase: true, out var parsedType)
                ? parsedType
                : NotificationType.Info;

            var userIds = new HashSet<Guid>();

            if (dto.UserIds is not null)
            {
                foreach (var raw in dto.UserIds)
                {
                    if (Guid.TryParse(raw, out var id))
                        userIds.Add(id);
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.TargetRole))
                userIds.UnionWith(await GetUserIdsByRoleAsync(dto.TargetRole));

            if (dto.ClassRoomId.HasValue)
                userIds.UnionWith(await GetUserIdsOfStudentsInClassAsync(
                    dto.ClassRoomId.Value, dto.SectionId));

            if (dto.SendToAll)
                userIds = (await _dbContext.Users
                    .Where(u => !u.IsDeleted && u.IsActive)
                    .Select(u => u.Id)
                    .ToListAsync()).ToHashSet();

            if (userIds.Count == 0)
                return ApiResponse.FailResponse("No recipients resolved for the notification.");

            await NotifyUsersAsync(userIds, dto.Title, dto.Message, type, dto.Link);

            return ApiResponse.SuccessResponse(ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task NotifyUsersAsync(IEnumerable<Guid> userIds, string title, string message,
        NotificationType type, string? link = null, string? data = null)
    {
        var uniqueIds = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
        if (uniqueIds.Count == 0)
            return;

        var now = DateTime.UtcNow;
        var notifications = new List<Notification>(uniqueIds.Count);

        foreach (var userId in uniqueIds)
        {
            notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Link = link,
                Data = data,
                IsRead = false,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await _unitOfWork.Notifications.AddRangeAsync(notifications);
        await _unitOfWork.SaveChangesAsync();

        foreach (var notification in notifications)
        {
            await _hubContext.Clients.Group($"user_{notification.UserId}")
                .SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    link = notification.Link,
                    isRead = false,
                    createdAt = notification.CreatedAt
                });
        }
    }

    public async Task NotifyAdminsAsync(string title, string message,
        NotificationType type = NotificationType.Info, string? link = null)
    {
        var adminIds = await GetUserIdsByRoleAsync(AdminRoles);
        await NotifyUsersAsync(adminIds, title, message, type, link);
    }

    public async Task NotifyStudentsOfClassAsync(Guid classRoomId, string title, string message,
        NotificationType type = NotificationType.Info, Guid? sectionId = null, string? link = null, string? data = null)
    {
        var userIds = await GetUserIdsOfStudentsInClassAsync(classRoomId, sectionId);
        await NotifyUsersAsync(userIds, title, message, type, link, data);
    }

    private async Task<List<Guid>> GetUserIdsByRoleAsync(params string[] roleNames)
    {
        return await _dbContext.Users
            .Where(u => !u.IsDeleted && u.IsActive
                && u.UserRoles.Any(ur => roleNames.Contains(ur.Role.Name)))
            .Select(u => u.Id)
            .ToListAsync();
    }

    private async Task<List<Guid>> GetUserIdsOfStudentsInClassAsync(Guid classRoomId, Guid? sectionId)
    {
        var sections = (await _unitOfWork.Sections.FindAsync(s =>
            s.ClassRoomId == classRoomId && !s.IsDeleted)).ToList();

        if (sectionId.HasValue)
            sections = sections.Where(s => s.Id == sectionId.Value).ToList();

        var userIds = new HashSet<Guid>();
        foreach (var section in sections)
        {
            var students = await _unitOfWork.Students.GetBySectionAsync(section.Id);
            foreach (var student in students.Where(s => !s.IsDeleted))
                userIds.Add(student.UserId);
        }

        return userIds.ToList();
    }
}
