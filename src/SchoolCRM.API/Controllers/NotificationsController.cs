using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Notification;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Notification;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationsController(INotificationService notificationService, IUnitOfWork unitOfWork)
    {
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<NotificationDto>>>> GetNotificationsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] bool unreadOnly = false)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<PagedResult<NotificationDto>>.UnauthorizedResponse());

        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _notificationService.GetNotificationsAsync(userId, query, unreadOnly);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCountAsync()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse<int>.UnauthorizedResponse());

        var result = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> MarkAsReadAsync(
        [FromRoute] Guid id)
    {
        var result = await _notificationService.MarkAsReadAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("mark-all-read")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> MarkAllAsReadAsync()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse.FailResponse("User not authenticated", 401));

        var result = await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(result);
    }

    [HttpGet("announcements")]
    public async Task<ActionResult> GetAnnouncementsAsync()
    {
        var announcements = await _unitOfWork.Announcements.GetAllAsync();
        var items = announcements.OrderByDescending(a => a.CreatedAt).ToList();
        return Ok(ApiResponse<List<Announcement>>.SuccessResponse(items));
    }

    [HttpPost("timetable-change")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> RequestTimetableChangeAsync(
        [FromBody] TimetableChangeRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(ApiResponse.FailResponse("Change description is required."));

        var title = "Timetable change requested";
        var message = $"A teacher has requested a timetable change.\n\n{dto.Message}";
        await _notificationService.NotifyAdminsAsync(title, message,
            SchoolCRM.Domain.Enums.NotificationType.Warning, link: "/timetable");

        return Ok(ApiResponse.SuccessResponse("Change request sent to admin."));
    }

    [HttpPost("announcements")]
    public async Task<ActionResult> CreateAnnouncementAsync([FromBody] AnnouncementRequestDto dto)
    {
        var school = (await _unitOfWork.Schools.GetAllAsync()).FirstOrDefault();

        var announcement = new Announcement
        {
            Title = dto.Title,
            Content = dto.Description,
            TargetAudience = dto.Type,
            Priority = dto.Priority,
            PublishDate = dto.PublishDate ?? DateTime.UtcNow,
            IsPublished = true,
            SchoolId = school?.Id ?? Guid.Empty,
            CreatedByName = User.Identity?.Name,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Announcements.AddAsync(announcement);
        await _unitOfWork.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, ApiResponse<Announcement>.SuccessResponse(announcement));
    }
}

public sealed class AnnouncementRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public string? Type { get; set; }
    public DateTime? PublishDate { get; set; }
}

public sealed class TimetableChangeRequestDto
{
    public string Message { get; set; } = string.Empty;
}
