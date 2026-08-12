using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SchoolCRM.Infrastructure.SignalR;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to notification hub", userId);
        }

        var role = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(role))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("User {UserId} disconnected from notification hub", userId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendNotificationToUser(string userId, string title, string message, string type)
    {
        await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SendNotificationToRole(string role, string title, string message, string type)
    {
        await Clients.Group($"role_{role}").SendAsync("ReceiveNotification", new
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task BroadcastNotification(string title, string message, string type)
    {
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task MarkNotificationRead(string notificationId)
    {
        await Clients.Caller.SendAsync("NotificationMarkedRead", notificationId);
    }
}
