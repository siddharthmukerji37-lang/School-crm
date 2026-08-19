using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/attendance-policy")]
[Authorize]
public class AttendancePolicyController : ControllerBase
{
    private readonly IAttendancePolicyService _policyService;

    public AttendancePolicyController(IAttendancePolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet("{schoolId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AttendancePolicyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AttendancePolicyDto>>> GetPolicy(Guid schoolId)
    {
        var result = await _policyService.GetPolicyAsync(schoolId);
        return Ok(result);
    }

    [HttpPut("{schoolId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<AttendancePolicyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AttendancePolicyDto>>> UpdatePolicy(
        Guid schoolId, [FromBody] UpdateAttendancePolicyDto dto)
    {
        var result = await _policyService.UpdatePolicyAsync(schoolId, dto);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("monthly-summary")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AttendanceMonthlySummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<AttendanceMonthlySummaryDto>>>> GetMonthlySummaries(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _policyService.GetMonthlySummariesAsync(month, year, pageNumber, pageSize);
        return Ok(result);
    }
}
