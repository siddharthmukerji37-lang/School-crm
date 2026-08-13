using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Chat;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    private Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : null;

    [HttpGet("conversations")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChatConversationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChatConversationDto>>>> GetConversationsAsync()
    {
        var userId = CurrentUserId;
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<IReadOnlyList<ChatConversationDto>>.UnauthorizedResponse());

        var result = await _chatService.GetConversationsAsync(userId.Value);
        return Ok(result);
    }

    [HttpGet("messages")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChatMessageDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChatMessageDto>>>> GetMessagesAsync(
        [FromQuery] Guid? peerUserId,
        [FromQuery] Guid? sectionId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var userId = CurrentUserId;
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<IReadOnlyList<ChatMessageDto>>.UnauthorizedResponse());

        var query = new PaginationQuery(pageNumber, pageSize);
        var result = await _chatService.GetMessagesAsync(userId.Value, peerUserId, sectionId, query);
        return Ok(result);
    }

    [HttpPost("messages")]
    [ProducesResponseType(typeof(ApiResponse<ChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ChatMessageDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendAsync([FromBody] SendChatMessageDto dto)
    {
        var userId = CurrentUserId;
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<ChatMessageDto>.UnauthorizedResponse());

        var result = await _chatService.SendAsync(userId.Value, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("read")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> MarkReadAsync([FromBody] MarkChatReadDto dto)
    {
        var userId = CurrentUserId;
        if (!userId.HasValue)
            return Unauthorized(ApiResponse.FailResponse("Unauthorized", 401));

        var result = await _chatService.MarkReadAsync(userId.Value, dto.PeerUserId);
        return Ok(result);
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChatUserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChatUserDto>>>> GetAvailableUsersAsync(
        [FromQuery] string? role,
        [FromQuery] string? search)
    {
        var userId = CurrentUserId;
        if (!userId.HasValue)
            return Unauthorized(ApiResponse<IReadOnlyList<ChatUserDto>>.UnauthorizedResponse());

        var result = await _chatService.GetAvailableUsersAsync(userId.Value, role, search);
        return Ok(result);
    }
}
