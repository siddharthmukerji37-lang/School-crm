using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.DTOs.Chat;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Entities.Notification;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Infrastructure.SignalR;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class ChatService : IChatService
{
    private static readonly string[] AdminRoleNames = Roles.AdminRoles;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatService(
        IUnitOfWork unitOfWork,
        ApplicationDbContext dbContext,
        IHubContext<ChatHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
        _hubContext = hubContext;
    }

    public async Task<ApiResponse<IReadOnlyList<ChatConversationDto>>> GetConversationsAsync(Guid userId)
    {
        try
        {
            var conversations = new List<ChatConversationDto>();
            var peerIds = await _unitOfWork.ChatMessages.GetRecentPeerIdsAsync(userId);

            foreach (var peerId in peerIds)
            {
                var peer = await _dbContext.Users.AsNoTracking()
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == peerId);
                if (peer is null)
                    continue;

                var latest = await _unitOfWork.ChatMessages.GetDirectMessagesAsync(userId, peerId, 1);
                var unread = await _unitOfWork.ChatMessages.GetUnreadDirectCountAsync(userId, peerId);
                var last = latest.FirstOrDefault();

                conversations.Add(new ChatConversationDto
                {
                    Id = $"direct:{peerId}",
                    Type = ChatMessageType.Direct.ToString(),
                    Title = BuildFullName(peer),
                    Subtitle = peer.UserRoles.FirstOrDefault()?.Role?.Name,
                    PeerUserId = peerId,
                    LastMessage = last?.Message,
                    LastMessageAt = last?.CreatedAt,
                    LastSenderId = last?.SenderId,
                    UnreadCount = unread
                });
            }

            var classSectionIds = new List<Guid>();
            var mySection = await GetStudentSectionAsync(userId);
            if (mySection.HasValue)
                classSectionIds.Add(mySection.Value);

            var participatedSections = await _dbContext.ChatMessages.AsNoTracking()
                .Where(m => m.MessageType == ChatMessageType.Class && m.SenderId == userId && m.SectionId != null)
                .Select(m => m.SectionId!.Value)
                .Distinct()
                .ToListAsync();

            foreach (var sectionId in participatedSections)
            {
                if (!classSectionIds.Contains(sectionId))
                    classSectionIds.Add(sectionId);
            }

            foreach (var sectionId in classSectionIds)
            {
                var conversation = await BuildClassConversationAsync(sectionId);
                if (conversation is not null)
                    conversations.Add(conversation);
            }

            var ordered = conversations
                .OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue)
                .ThenBy(c => c.Title)
                .ToList();

            return ApiResponse<IReadOnlyList<ChatConversationDto>>.SuccessResponse(ordered);
        }
        catch (Exception ex)
        {
            return ApiResponse<IReadOnlyList<ChatConversationDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<IReadOnlyList<ChatMessageDto>>> GetMessagesAsync(
        Guid userId, Guid? peerUserId, Guid? sectionId, PaginationQuery query)
    {
        try
        {
            var take = query.PageSize > 0 ? query.PageSize : 50;

            IReadOnlyList<ChatMessage> messages;
            if (sectionId.HasValue)
            {
                messages = await _unitOfWork.ChatMessages.GetClassMessagesAsync(sectionId.Value, take);
            }
            else if (peerUserId.HasValue)
            {
                messages = await _unitOfWork.ChatMessages.GetDirectMessagesAsync(userId, peerUserId.Value, take);
            }
            else
            {
                return ApiResponse<IReadOnlyList<ChatMessageDto>>.FailResponse("A peer user or section is required.");
            }

            var roleNames = await GetSenderRoleNamesAsync(
                messages.Select(m => m.SenderId).Distinct().ToList());

            var items = messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => ToDto(m, roleNames))
                .ToList();

            return ApiResponse<IReadOnlyList<ChatMessageDto>>.SuccessResponse(items);
        }
        catch (Exception ex)
        {
            return ApiResponse<IReadOnlyList<ChatMessageDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ChatMessageDto>> SendAsync(Guid senderId, SendChatMessageDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                return ApiResponse<ChatMessageDto>.FailResponse("Message cannot be empty.");

            var type = Enum.TryParse<ChatMessageType>(dto.MessageType, ignoreCase: true, out var parsed)
                ? parsed
                : ChatMessageType.Direct;

            var sender = await _dbContext.Users.AsNoTracking()
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == senderId);
            if (sender is null)
                return ApiResponse<ChatMessageDto>.FailResponse("Sender not found.");

            Guid? receiverId = null;
            Guid? sectionId = null;

            if (type == ChatMessageType.Class)
            {
                sectionId = dto.SectionId;
                if (!sectionId.HasValue)
                    return ApiResponse<ChatMessageDto>.FailResponse("Section is required for class chat.");

                var isStudent = sender.UserRoles.Any(ur => ur.Role?.Name == Roles.Student);
                if (isStudent)
                {
                    var mySection = await GetStudentSectionAsync(senderId);
                    if (!mySection.HasValue || mySection.Value != sectionId.Value)
                        return ApiResponse<ChatMessageDto>.FailResponse("You can only post to your own class chat.");
                }
            }
            else
            {
                receiverId = dto.ReceiverId;
                if (!receiverId.HasValue || receiverId == senderId)
                    return ApiResponse<ChatMessageDto>.FailResponse("A valid receiver is required.");
            }

            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                SectionId = sectionId,
                MessageType = type,
                Message = dto.Message.Trim(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ChatMessages.AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            var dtoMessage = new ChatMessageDto
            {
                Id = message.Id,
                SenderId = senderId,
                SenderName = BuildFullName(sender),
                SenderRole = sender.UserRoles.FirstOrDefault()?.Role?.Name,
                ReceiverId = receiverId,
                SectionId = sectionId,
                MessageType = type.ToString(),
                Message = message.Message,
                AttachmentUrl = message.AttachmentUrl,
                IsRead = false,
                CreatedAt = message.CreatedAt
            };

            if (type == ChatMessageType.Class)
            {
                await _hubContext.Clients.Group($"section_{sectionId}").SendAsync("ReceiveMessage", dtoMessage);
                await _hubContext.Clients.Group($"user_{senderId}").SendAsync("ReceiveMessage", dtoMessage);
            }
            else
            {
                await _hubContext.Clients.Group($"user_{receiverId}").SendAsync("ReceiveMessage", dtoMessage);
                await _hubContext.Clients.Group($"user_{senderId}").SendAsync("ReceiveMessage", dtoMessage);
            }

            return ApiResponse<ChatMessageDto>.SuccessResponse(dtoMessage);
        }
        catch (Exception ex)
        {
            return ApiResponse<ChatMessageDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> MarkReadAsync(Guid userId, Guid peerUserId)
    {
        try
        {
            if (userId == peerUserId)
                return ApiResponse.FailResponse("Invalid peer user.");

            await _unitOfWork.ChatMessages.MarkDirectReadAsync(userId, peerUserId);
            return ApiResponse.SuccessResponse(ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<IReadOnlyList<ChatUserDto>>> GetAvailableUsersAsync(
        Guid userId, string? role, string? search)
    {
        try
        {
            var callerRoles = await GetUserRolesAsync(userId);
            var isStudent = callerRoles.Contains(Roles.Student);

            var roleFilter = string.IsNullOrWhiteSpace(role) ? null : role.Trim();
            var wantsStudents = roleFilter == null
                || roleFilter.Equals("Student", StringComparison.OrdinalIgnoreCase)
                || roleFilter.Equals("Classmate", StringComparison.OrdinalIgnoreCase);

            var usersQuery = _dbContext.Users.AsNoTracking()
                .Where(u => !u.IsDeleted && u.IsActive && u.Id != userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                usersQuery = usersQuery.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(term)
                    || (u.Email ?? "").ToLower().Contains(term));
            }

            List<ApplicationUser> classmates = new();
            if (isStudent && wantsStudents)
            {
                var mySection = await GetStudentSectionAsync(userId);
                if (mySection.HasValue)
                {
                    var classmateIds = await _dbContext.Students.AsNoTracking()
                        .Where(s => s.SectionId == mySection.Value && !s.IsDeleted && s.UserId != userId)
                        .Select(s => s.UserId)
                        .ToListAsync();

                    classmates = await _dbContext.Users.AsNoTracking()
                        .Where(u => classmateIds.Contains(u.Id))
                        .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                        .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                        .ToListAsync();
                }
            }

            List<ApplicationUser> general = new();
            if (!(isStudent && wantsStudents))
            {
                if (roleFilter != null && !roleFilter.Equals("Classmate", StringComparison.OrdinalIgnoreCase))
                {
                    if (roleFilter.Equals("Staff", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = usersQuery.Where(u => u.UserRoles.Any(ur => AdminRoleNames.Contains(ur.Role.Name)));
                    }
                    else if (roleFilter.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                    {
                        usersQuery = usersQuery.Where(u => u.UserRoles.Any(ur => Roles.TeachingRoles.Contains(ur.Role.Name)));
                    }
                    else
                    {
                        usersQuery = usersQuery.Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleFilter));
                    }
                }

                general = await usersQuery
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                    .Take(100)
                    .ToListAsync();
            }
            else if (roleFilter == null)
            {
                general = await usersQuery
                    .Where(u => u.UserRoles.Any(ur =>
                        Roles.TeachingRoles.Contains(ur.Role.Name) || AdminRoleNames.Contains(ur.Role.Name)))
                    .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                    .Take(100)
                    .ToListAsync();
            }

            var result = general.Select(u => new ChatUserDto
            {
                Id = u.Id,
                FullName = BuildFullName(u),
                Email = u.Email ?? string.Empty,
                Role = u.UserRoles.FirstOrDefault()?.Role?.Name,
                ProfilePictureUrl = u.ProfilePictureUrl
            }).ToList();

            result.AddRange(classmates.Select(u => new ChatUserDto
            {
                Id = u.Id,
                FullName = BuildFullName(u),
                Email = u.Email ?? string.Empty,
                Role = u.UserRoles.FirstOrDefault()?.Role?.Name,
                ProfilePictureUrl = u.ProfilePictureUrl,
                SectionName = "Classmate"
            }));

            var distinct = result.GroupBy(u => u.Id).Select(g => g.First()).ToList();

            return ApiResponse<IReadOnlyList<ChatUserDto>>.SuccessResponse(distinct);
        }
        catch (Exception ex)
        {
            return ApiResponse<IReadOnlyList<ChatUserDto>>.FailResponse(ex.Message);
        }
    }

    private async Task<ChatConversationDto?> BuildClassConversationAsync(Guid sectionId)
    {
        var section = await _dbContext.Sections.AsNoTracking()
            .Include(s => s.ClassRoom)
            .FirstOrDefaultAsync(s => s.Id == sectionId);
        if (section is null)
            return null;

        var latest = await _unitOfWork.ChatMessages.GetClassMessagesAsync(sectionId, 1);
        var last = latest.FirstOrDefault();

        return new ChatConversationDto
        {
            Id = $"class:{sectionId}",
            Type = ChatMessageType.Class.ToString(),
            Title = $"{section.ClassRoom?.Name} - Section {section.Name}",
            Subtitle = "Class Chat",
            SectionId = sectionId,
            LastMessage = last?.Message,
            LastMessageAt = last?.CreatedAt,
            LastSenderId = last?.SenderId,
            UnreadCount = 0
        };
    }

    private async Task<Dictionary<Guid, string>> GetSenderRoleNamesAsync(List<Guid> userIds)
    {
        if (userIds.Count == 0)
            return new Dictionary<Guid, string>();

        var result = await _dbContext.UserRoles.AsNoTracking()
            .Include(ur => ur.Role)
            .Where(ur => userIds.Contains(ur.UserId))
            .GroupBy(ur => ur.UserId)
            .Select(g => new { UserId = g.Key, Role = g.Select(x => x.Role.Name).OrderBy(n => n).FirstOrDefault() })
            .ToDictionaryAsync(x => x.UserId, x => x.Role ?? string.Empty);

        return result;
    }

    private async Task<Guid?> GetStudentSectionAsync(Guid userId)
    {
        var student = await _unitOfWork.Students.GetStudentByUserIdAsync(userId);
        return student?.SectionId;
    }

    private async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await _dbContext.UserRoles.AsNoTracking()
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role != null ? ur.Role.Name : string.Empty)
            .ToListAsync();
    }

    private static ChatMessageDto ToDto(ChatMessage message, IReadOnlyDictionary<Guid, string> roleNames)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = BuildFullName(message.Sender),
            SenderRole = roleNames.GetValueOrDefault(message.SenderId),
            ReceiverId = message.ReceiverId,
            SectionId = message.SectionId,
            MessageType = message.MessageType.ToString(),
            Message = message.Message,
            AttachmentUrl = message.AttachmentUrl,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt
        };
    }

    private static string BuildFullName(ApplicationUser? user)
    {
        if (user is null)
            return string.Empty;
        return $"{user.FirstName} {user.LastName}".Trim();
    }
}
