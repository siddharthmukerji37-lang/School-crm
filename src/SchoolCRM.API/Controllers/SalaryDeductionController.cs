using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/salary-deductions")]
[Authorize]
public class SalaryDeductionController : ControllerBase
{
    private readonly ISalaryDeductionService _deductionService;
    private readonly ICurrentUserService _currentUserService;

    public SalaryDeductionController(ISalaryDeductionService deductionService, ICurrentUserService currentUserService)
    {
        _deductionService = deductionService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SalaryDeductionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SalaryDeductionDto>>>> GetDeductions(
        [FromQuery] int? month = null,
        [FromQuery] int? year = null,
        [FromQuery] string? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _deductionService.GetDeductionsAsync(month, year, status, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<SalaryDeductionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SalaryDeductionDto>>> ApproveDeduction(
        Guid id, [FromBody] ApproveDeductionDto dto)
    {
        var approvedBy = _currentUserService.Email ?? "Admin";
        var result = await _deductionService.ApproveDeductionAsync(id, dto, approvedBy);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ApiResponse<SalaryDeductionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SalaryDeductionDto>>> RejectDeduction(
        Guid id, [FromBody] ApproveDeductionDto dto)
    {
        var rejectedBy = _currentUserService.Email ?? "Admin";
        var result = await _deductionService.RejectDeductionAsync(id, dto, rejectedBy);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SalaryDeductionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SalaryDeductionDto>>>> GetMyDeductions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            return BadRequest(ApiResponse.FailResponse("Unable to identify user."));

        var result = await _deductionService.GetUserDeductionsAsync(userId, pageNumber, pageSize);
        return Ok(result);
    }
}
