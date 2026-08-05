using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Notification;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NoticesController : ControllerBase
{
    private readonly INoticeService _noticeService;

    public NoticesController(INoticeService noticeService)
    {
        _noticeService = noticeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<NoticeDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<NoticeDto>>>> GetNoticesAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm);
        var result = await _noticeService.GetNoticesAsync(query);
        return Ok(result);
    }

    [HttpGet("published")]
    [ProducesResponseType(typeof(ApiResponse<List<NoticeDto>>), StatusCodes.Status200OK)]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<NoticeDto>>>> GetPublishedNoticesAsync()
    {
        var result = await _noticeService.GetPublishedNoticesAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NoticeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NoticeDto>>> GetNoticeByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _noticeService.GetNoticeByIdAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<NoticeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<NoticeDto>>> CreateNoticeAsync(
        [FromBody] CreateNoticeDto dto)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized(ApiResponse<NoticeDto>.UnauthorizedResponse());

        var userId = Guid.Parse(userIdClaim);
        var createdByName = User.Identity?.Name ?? "Unknown";

        var result = await _noticeService.CreateNoticeAsync(dto, userId, createdByName);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NoticeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<NoticeDto>>> UpdateNoticeAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateNoticeDto dto)
    {
        var result = await _noticeService.UpdateNoticeAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteNoticeAsync(
        [FromRoute] Guid id)
    {
        var result = await _noticeService.DeleteNoticeAsync(id);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }
}
