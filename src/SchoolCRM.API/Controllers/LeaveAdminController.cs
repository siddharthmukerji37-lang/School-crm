using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs;
using SchoolCRM.Application.DTOs.Leave;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/admin/leaves")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class LeaveAdminController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveAdminController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    [HttpPost("calendar")]
    public async Task<ActionResult<ApiResponse<LeaveCalendarDto>>> CreateCalendar([FromBody] CreateLeaveCalendarDto dto)
    {
        var result = await _leaveService.CreateLeaveCalendarAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("calendar")]
    public async Task<ActionResult<ApiResponse<List<LeaveCalendarDto>>>> GetCalendars()
    {
        var result = await _leaveService.GetLeaveCalendarsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("calendar/active")]
    public async Task<ActionResult<ApiResponse<LeaveCalendarDto>>> GetActiveCalendar()
    {
        var result = await _leaveService.GetActiveLeaveCalendarAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("types")]
    public async Task<ActionResult<ApiResponse<LeaveTypeDto>>> CreateLeaveType([FromBody] CreateLeaveTypeDto dto)
    {
        var result = await _leaveService.CreateLeaveTypeAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<List<LeaveTypeDto>>>> GetLeaveTypes()
    {
        var result = await _leaveService.GetLeaveTypesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("types/{id}")]
    public async Task<ActionResult<ApiResponse<LeaveTypeDto>>> UpdateLeaveType(Guid id, [FromBody] CreateLeaveTypeDto dto)
    {
        var result = await _leaveService.UpdateLeaveTypeAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("calendar/{calendarId}/configs")]
    public async Task<ActionResult<ApiResponse<LeaveTypeConfigDto>>> CreateConfig(Guid calendarId, [FromBody] CreateLeaveTypeConfigDto dto)
    {
        var result = await _leaveService.CreateLeaveTypeConfigAsync(calendarId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("configs/{id}")]
    public async Task<ActionResult<ApiResponse<LeaveTypeConfigDto>>> UpdateConfig(Guid id, [FromBody] UpdateLeaveTypeConfigDto dto)
    {
        var result = await _leaveService.UpdateLeaveTypeConfigAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("calendar/{calendarId}/configs")]
    public async Task<ActionResult<ApiResponse<List<LeaveTypeConfigDto>>>> GetConfigs(Guid calendarId)
    {
        var result = await _leaveService.GetLeaveTypeConfigsAsync(calendarId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("calendar/{calendarId}/initialize-balances")]
    public async Task<ActionResult<ApiResponse>> InitializeBalances(Guid calendarId)
    {
        var result = await _leaveService.InitializeLeaveBalancesAsync(calendarId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("balances/{userId}")]
    public async Task<ActionResult<ApiResponse<List<LeaveBalanceDto>>>> GetUserBalances(Guid userId)
    {
        var result = await _leaveService.GetUserLeaveBalancesAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<LeaveRequestDto>>>> GetAllRequests(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _leaveService.GetAllLeaveRequestsAsync(new PaginationQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<ApiResponse<List<LeaveRequestDto>>>> GetPendingRequests()
    {
        var result = await _leaveService.GetPendingRequestsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/approve")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> ApproveLeave(Guid id, [FromBody] ApproveLeaveDto dto)
    {
        var result = await _leaveService.ApproveLeaveAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/reject")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> RejectLeave(Guid id, [FromBody] RejectLeaveDto dto)
    {
        var result = await _leaveService.RejectLeaveAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
