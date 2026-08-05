using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Dashboard;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<DashboardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DashboardDto>>> GetDashboardStatsAsync(
        [FromQuery] Guid? schoolId = null)
    {
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "User";
        var resolvedSchoolId = schoolId ?? Guid.Empty;

        var result = await _dashboardService.GetDashboardStatsAsync(resolvedSchoolId, userRole);
        return Ok(result);
    }

    [HttpGet("attendance-chart")]
    [ProducesResponseType(typeof(ApiResponse<List<ChartDataDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ChartDataDto>>>> GetAttendanceChartDataAsync(
        [FromQuery] Guid? schoolId = null,
        [FromQuery] int months = 6)
    {
        var resolvedSchoolId = schoolId ?? Guid.Empty;
        var result = await _dashboardService.GetAttendanceChartDataAsync(resolvedSchoolId, months);
        return Ok(result);
    }

    [HttpGet("fee-chart")]
    [ProducesResponseType(typeof(ApiResponse<List<ChartDataDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ChartDataDto>>>> GetFeeChartDataAsync(
        [FromQuery] Guid? schoolId = null,
        [FromQuery] int months = 6)
    {
        var resolvedSchoolId = schoolId ?? Guid.Empty;
        var result = await _dashboardService.GetFeeChartDataAsync(resolvedSchoolId, months);
        return Ok(result);
    }
}
