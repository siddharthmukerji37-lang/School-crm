using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SchoolCRM.Infrastructure.SignalR;

[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinSection(Guid sectionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"section_{sectionId}");
    }

    public async Task LeaveSection(Guid sectionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"section_{sectionId}");
    }
}
