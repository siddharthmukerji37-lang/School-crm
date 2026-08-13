using SchoolCRM.Application.DTOs.Chat;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IChatService
{
    Task<ApiResponse<IReadOnlyList<ChatConversationDto>>> GetConversationsAsync(Guid userId);
    Task<ApiResponse<IReadOnlyList<ChatMessageDto>>> GetMessagesAsync(Guid userId, Guid? peerUserId, Guid? sectionId, PaginationQuery query);
    Task<ApiResponse<ChatMessageDto>> SendAsync(Guid senderId, SendChatMessageDto dto);
    Task<ApiResponse> MarkReadAsync(Guid userId, Guid peerUserId);
    Task<ApiResponse<IReadOnlyList<ChatUserDto>>> GetAvailableUsersAsync(Guid userId, string? role, string? search);
}
