using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Fee;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeesController : ControllerBase
{
    private readonly IFeeService _feeService;

    public FeesController(IFeeService feeService)
    {
        _feeService = feeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FeeStructureDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<FeeStructureDto>>>> GetFeeStructuresAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] Guid? classRoomId = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _feeService.GetFeeStructuresAsync(query, classRoomId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FeeStructureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FeeStructureDto>>> GetFeeStructureByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _feeService.GetFeeStructureByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FeeStructureDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FeeStructureDto>>> CreateFeeStructureAsync(
        [FromBody] FeeStructureDto dto)
    {
        var result = await _feeService.CreateFeeStructureAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FeeStructureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FeeStructureDto>>> UpdateFeeStructureAsync(
        [FromRoute] Guid id,
        [FromBody] FeeStructureDto dto)
    {
        var result = await _feeService.UpdateFeeStructureAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteFeeStructureAsync(
        [FromRoute] Guid id)
    {
        var result = await _feeService.DeleteFeeStructureAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{feeStructureId:guid}/installments")]
    [ProducesResponseType(typeof(ApiResponse<List<FeeInstallmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<FeeInstallmentDto>>>> GetInstallmentsAsync(
        [FromRoute] Guid feeStructureId)
    {
        var result = await _feeService.GetInstallmentsAsync(feeStructureId);
        return Ok(result);
    }

    [HttpPost("installments")]
    [ProducesResponseType(typeof(ApiResponse<FeeInstallmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FeeInstallmentDto>>> CreateInstallmentAsync(
        [FromBody] FeeInstallmentDto dto)
    {
        var result = await _feeService.CreateInstallmentAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("installments/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FeeInstallmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FeeInstallmentDto>>> UpdateInstallmentAsync(
        [FromRoute] Guid id,
        [FromBody] FeeInstallmentDto dto)
    {
        var result = await _feeService.UpdateInstallmentAsync(id, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("installments/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> DeleteInstallmentAsync(
        [FromRoute] Guid id)
    {
        var result = await _feeService.DeleteInstallmentAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("collect")]
    [ProducesResponseType(typeof(ApiResponse<FeeReceiptDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FeeReceiptDto>>> CollectFeeAsync(
        [FromBody] CollectFeeDto dto)
    {
        var result = await _feeService.CollectFeeAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("receipts")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<FeeReceiptDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<FeeReceiptDto>>>> GetFeeReceiptsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] Guid? studentId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _feeService.GetFeeReceiptsAsync(query, studentId, fromDate, toDate);
        return Ok(result);
    }

    [HttpGet("receipt/{receiptNumber}")]
    [ProducesResponseType(typeof(ApiResponse<FeeReceiptDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FeeReceiptDto>>> GetReceiptAsync(
        [FromRoute] string receiptNumber)
    {
        if (Guid.TryParse(receiptNumber, out var receiptId))
        {
            var result = await _feeService.GetFeeReceiptByIdAsync(receiptId);
            if (!result.Success)
                return NotFound(ApiResponse<FeeReceiptDto>.NotFoundResponse("Receipt not found"));

            return Ok(result);
        }

        return NotFound(ApiResponse<FeeReceiptDto>.NotFoundResponse("Receipt not found"));
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(ApiResponse<FeeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FeeSummaryDto>>> GetPendingFeesAsync(
        [FromQuery] Guid studentId)
    {
        var result = await _feeService.GetFeeSummaryAsync(studentId);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("summary/{studentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FeeSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FeeSummaryDto>>> GetStudentFeeSummaryAsync(
        [FromRoute] Guid studentId)
    {
        var result = await _feeService.GetFeeSummaryAsync(studentId);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("class-summary/{classRoomId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FeeSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FeeSummaryDto>>> GetClassFeeSummaryAsync(
        [FromRoute] Guid classRoomId)
    {
        var result = await _feeService.GetClassFeeSummaryAsync(classRoomId);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
