using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Leave;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/leaves")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    public LeaveController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    [HttpGet("types")]
    public async Task<ActionResult<ApiResponse<List<LeaveTypeConfigDto>>>> GetLeaveTypesForUser()
    {
        var result = await _leaveService.GetLeaveTypesForUserAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("balance")]
    public async Task<ActionResult<ApiResponse<List<LeaveBalanceDto>>>> GetMyLeaveBalance()
    {
        var result = await _leaveService.GetMyLeaveBalanceAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> ApplyLeave([FromBody] ApplyLeaveDto dto)
    {
        var result = await _leaveService.ApplyLeaveAsync(dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("my-requests")]
    public async Task<ActionResult<ApiResponse<List<LeaveRequestDto>>>> GetMyLeaveRequests()
    {
        var result = await _leaveService.GetMyLeaveRequestsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/cancel")]
    public async Task<ActionResult<ApiResponse>> CancelLeave(Guid id)
    {
        var result = await _leaveService.CancelLeaveAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
