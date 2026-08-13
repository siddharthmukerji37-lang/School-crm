using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Notification;

public class Notification : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Link { get; set; }
    public string? Data { get; set; }
    public Guid UserId { get; set; }

    public Identity.ApplicationUser User { get; set; } = null!;
}

public class Announcement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? TargetAudience { get; set; }
    public string? Priority { get; set; }
    public DateTime? PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsPublished { get; set; }
    public string? AttachmentUrl { get; set; }
    public Guid SchoolId { get; set; }
    public string? CreatedByName { get; set; }
}

public class Circular : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CircularNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public string? TargetAudience { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsPublished { get; set; }
    public Guid SchoolId { get; set; }
    public string? CreatedByName { get; set; }
}

public class ChatMessage : BaseEntity
{
    public string Message { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public Guid SenderId { get; set; }
    public Guid? ReceiverId { get; set; }
    public Guid? SectionId { get; set; }
    public ChatMessageType MessageType { get; set; } = ChatMessageType.Direct;
    public Guid? ParentMessageId { get; set; }

    public Identity.ApplicationUser Sender { get; set; } = null!;
    public Identity.ApplicationUser? Receiver { get; set; }
    public School.Section? Section { get; set; }
    public ChatMessage? ParentMessage { get; set; }
}
