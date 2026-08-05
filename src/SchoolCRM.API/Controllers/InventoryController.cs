using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IInventoryService;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    #region Items

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ItemDto>>>> GetItemsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? category = null,
        [FromQuery] Guid? schoolId = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _inventoryService.GetItemsAsync(query, category, schoolId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> GetItemByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _inventoryService.GetItemByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> CreateItemAsync(
        [FromBody] CreateItemDto dto)
    {
        var result = await _inventoryService.CreateItemAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ItemDto>>> UpdateItemAsync(
        [FromRoute] Guid id,
        [FromBody] CreateItemDto dto)
    {
        var result = await _inventoryService.UpdateItemAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteItemAsync(
        [FromRoute] Guid id)
    {
        var result = await _inventoryService.DeleteItemAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{itemId:guid}/stock")]
    [ProducesResponseType(typeof(ApiResponse<StockDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StockDto>>> GetStockAsync(
        [FromRoute] Guid itemId)
    {
        var result = await _inventoryService.GetStockAsync(itemId);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{itemId:guid}/movements")]
    [ProducesResponseType(typeof(ApiResponse<List<StockMovementDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<StockMovementDto>>>> GetStockMovementsAsync(
        [FromRoute] Guid itemId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var result = await _inventoryService.GetStockMovementsAsync(itemId, fromDate, toDate);
        return Ok(result);
    }

    #endregion

    #region Stock Adjustment

    [HttpPost("adjust")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> AdjustStockAsync(
        [FromBody] AdjustStockDto dto)
    {
        var result = await _inventoryService.AdjustStockAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region Vendors

    [HttpGet("vendors")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<VendorDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<VendorDto>>>> GetVendorsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _inventoryService.GetVendorsAsync(query);
        return Ok(result);
    }

    [HttpGet("vendors/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VendorDto>>> GetVendorByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _inventoryService.GetVendorByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("vendors")]
    [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<VendorDto>>> CreateVendorAsync(
        [FromBody] CreateVendorDto dto)
    {
        var result = await _inventoryService.CreateVendorAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("vendors/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<VendorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<VendorDto>>> UpdateVendorAsync(
        [FromRoute] Guid id,
        [FromBody] CreateVendorDto dto)
    {
        var result = await _inventoryService.UpdateVendorAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("vendors/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteVendorAsync(
        [FromRoute] Guid id)
    {
        var result = await _inventoryService.DeleteVendorAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion
}
