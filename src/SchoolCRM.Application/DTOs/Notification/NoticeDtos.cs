namespace SchoolCRM.Application.DTOs.Notification;

public sealed class NoticeDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public DateTime? PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsPublished { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CreateNoticeDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public DateTime? PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsPublished { get; set; } = true;
}

public sealed class UpdateNoticeDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public DateTime? PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsPublished { get; set; }
}
