using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IHostelService;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HostelController : ControllerBase
{
    private readonly IHostelService _hostelService;

    public HostelController(IHostelService hostelService)
    {
        _hostelService = hostelService;
    }

    #region Hostels

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<HostelDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<HostelDto>>>> GetHostelsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] Guid? schoolId = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _hostelService.GetHostelsAsync(query, schoolId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HostelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HostelDto>>> GetHostelByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _hostelService.GetHostelByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HostelDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HostelDto>>> CreateHostelAsync(
        [FromBody] HostelDto dto)
    {
        var result = await _hostelService.CreateHostelAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HostelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HostelDto>>> UpdateHostelAsync(
        [FromRoute] Guid id,
        [FromBody] HostelDto dto)
    {
        var result = await _hostelService.UpdateHostelAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteHostelAsync(
        [FromRoute] Guid id)
    {
        var result = await _hostelService.DeleteHostelAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion

    #region Rooms

    [HttpGet("rooms")]
    [ProducesResponseType(typeof(ApiResponse<List<RoomDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<RoomDto>>>> GetAllRoomsAsync()
    {
        var result = await _hostelService.GetRoomsAsync(null);
        return Ok(result);
    }

    [HttpGet("{hostelId:guid}/rooms")]
    [ProducesResponseType(typeof(ApiResponse<List<RoomDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<RoomDto>>>> GetRoomsAsync(
        [FromRoute] Guid hostelId)
    {
        var result = await _hostelService.GetRoomsAsync(hostelId);
        return Ok(result);
    }

    [HttpPost("rooms")]
    [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RoomDto>>> CreateRoomAsync(
        [FromBody] CreateRoomDto dto)
    {
        var result = await _hostelService.CreateRoomAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("rooms/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RoomDto>>> UpdateRoomAsync(
        [FromRoute] Guid id,
        [FromBody] CreateRoomDto dto)
    {
        var result = await _hostelService.UpdateRoomAsync(id, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("rooms/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteRoomAsync(
        [FromRoute] Guid id)
    {
        var result = await _hostelService.DeleteRoomAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("rooms/{roomId:guid}/beds")]
    [ProducesResponseType(typeof(ApiResponse<List<BedDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<BedDto>>>> GetBedsAsync(
        [FromRoute] Guid roomId)
    {
        var result = await _hostelService.GetBedsAsync(roomId);
        return Ok(result);
    }

    #endregion

    #region Allocations

    [HttpGet("available-rooms")]
    [ProducesResponseType(typeof(ApiResponse<List<RoomDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<RoomDto>>>> GetAvailableRoomsAsync(
        [FromQuery] Guid? hostelId = null)
    {
        if (hostelId.HasValue)
        {
            var rooms = await _hostelService.GetRoomsAsync(hostelId.Value);
            if (rooms.Data is not null)
            {
                var availableRooms = rooms.Data.Where(r => r.AvailableBeds > 0).ToList();
                return Ok(ApiResponse<List<RoomDto>>.SuccessResponse(availableRooms));
            }
        }

        return Ok(ApiResponse<List<RoomDto>>.SuccessResponse(new List<RoomDto>()));
    }

    [HttpGet("allocations")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BedAllocationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<BedAllocationDto>>>> GetAllocationsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] Guid? hostelId = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _hostelService.GetAllocationsAsync(query, hostelId);
        return Ok(result);
    }

    [HttpPost("allocate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> AllocateRoomAsync(
        [FromBody] BedAllocationDto dto)
    {
        var result = await _hostelService.AllocateBedAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("checkout/{allocationId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> CheckoutAsync(
        [FromRoute] Guid allocationId)
    {
        var result = await _hostelService.DeallocateBedAsync(allocationId);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion
}
