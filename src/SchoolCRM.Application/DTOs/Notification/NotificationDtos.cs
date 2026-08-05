using System.ComponentModel.DataAnnotations;

namespace SchoolCRM.Application.DTOs.Notification;

public sealed class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? SenderName { get; set; }
    public string? SenderProfilePictureUrl { get; set; }
}

public sealed class SendNotificationDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Type is required")]
    public string Type { get; set; } = string.Empty;

    public string Priority { get; set; } = "Normal";

    public string? Link { get; set; }

    public List<string>? UserIds { get; set; }

    public string? TargetRole { get; set; }

    public Guid? SchoolId { get; set; }

    public Guid? ClassRoomId { get; set; }

    public Guid? SectionId { get; set; }

    public bool SendToAll { get; set; }
}
