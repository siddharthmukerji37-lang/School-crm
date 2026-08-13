using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Application.DTOs.Chat;

public sealed class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderRole { get; set; }
    public Guid? ReceiverId { get; set; }
    public Guid? SectionId { get; set; }
    public string MessageType { get; set; } = ChatMessageType.Direct.ToString();
    public string Message { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class SendChatMessageDto
{
    public string Message { get; set; } = string.Empty;
    public Guid? ReceiverId { get; set; }
    public Guid? SectionId { get; set; }
    public string MessageType { get; set; } = ChatMessageType.Direct.ToString();
}

public sealed class ChatConversationDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = ChatMessageType.Direct.ToString();
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public Guid? PeerUserId { get; set; }
    public Guid? SectionId { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public Guid? LastSenderId { get; set; }
    public int UnreadCount { get; set; }
}

public sealed class ChatUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? SectionName { get; set; }
}

public sealed class MarkChatReadDto
{
    public Guid PeerUserId { get; set; }
}
