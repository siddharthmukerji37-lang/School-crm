using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IAccountService;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    #region Income

    [HttpGet("income")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<IncomeDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<IncomeDto>>>> GetIncomeAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? schoolId = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _accountService.GetIncomeAsync(query, fromDate, toDate, schoolId);
        return Ok(result);
    }

    [HttpGet("income/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncomeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IncomeDto>>> GetIncomeByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _accountService.GetIncomeByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("income")]
    [ProducesResponseType(typeof(ApiResponse<IncomeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IncomeDto>>> CreateIncomeAsync(
        [FromBody] CreateIncomeDto dto)
    {
        var result = await _accountService.CreateIncomeAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("income/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncomeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IncomeDto>>> UpdateIncomeAsync(
        [FromRoute] Guid id,
        [FromBody] CreateIncomeDto dto)
    {
        var result = await _accountService.UpdateIncomeAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("income/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteIncomeAsync(
        [FromRoute] Guid id)
    {
        var result = await _accountService.DeleteIncomeAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion

    #region Expense

    [HttpGet("expense")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ExpenseDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExpenseDto>>>> GetExpenseAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? schoolId = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _accountService.GetExpenseAsync(query, fromDate, toDate, schoolId);
        return Ok(result);
    }

    [HttpGet("expense/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> GetExpenseByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _accountService.GetExpenseByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("expense")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> CreateExpenseAsync(
        [FromBody] CreateExpenseDto dto)
    {
        var result = await _accountService.CreateExpenseAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("expense/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExpenseDto>>> UpdateExpenseAsync(
        [FromRoute] Guid id,
        [FromBody] CreateExpenseDto dto)
    {
        var result = await _accountService.UpdateExpenseAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("expense/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteExpenseAsync(
        [FromRoute] Guid id)
    {
        var result = await _accountService.DeleteExpenseAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion

    #region Ledger & Summary

    [HttpGet("ledger")]
    [ProducesResponseType(typeof(ApiResponse<List<LedgerEntryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<LedgerEntryDto>>>> GetLedgerAsync(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] Guid? schoolId = null)
    {
        var result = await _accountService.GetLedgerAsync(fromDate, toDate, schoolId);
        return Ok(result);
    }

    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<decimal>>> GetSummaryAsync(
        [FromQuery] Guid schoolId)
    {
        var result = await _accountService.GetBalanceAsync(schoolId);
        return Ok(result);
    }

    #endregion
}
