using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IInventoryService
{
    Task<ApiResponse<PagedResult<ItemDto>>> GetItemsAsync(
        PaginationQuery query, string? category, Guid? schoolId);

    Task<ApiResponse<ItemDto>> GetItemByIdAsync(Guid id);

    Task<ApiResponse<ItemDto>> CreateItemAsync(CreateItemDto dto);

    Task<ApiResponse<ItemDto>> UpdateItemAsync(Guid id, CreateItemDto dto);

    Task<ApiResponse> DeleteItemAsync(Guid id);

    Task<ApiResponse<StockDto>> GetStockAsync(Guid itemId);

    Task<ApiResponse> AdjustStockAsync(AdjustStockDto dto);

    Task<ApiResponse<List<StockMovementDto>>> GetStockMovementsAsync(
        Guid itemId, DateTime? fromDate, DateTime? toDate);

    Task<ApiResponse<PagedResult<VendorDto>>> GetVendorsAsync(PaginationQuery query);

    Task<ApiResponse<VendorDto>> GetVendorByIdAsync(Guid id);

    Task<ApiResponse<VendorDto>> CreateVendorAsync(CreateVendorDto dto);

    Task<ApiResponse<VendorDto>> UpdateVendorAsync(Guid id, CreateVendorDto dto);

    Task<ApiResponse> DeleteVendorAsync(Guid id);

    public sealed class ItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public Guid? VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public sealed class CreateItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinimumStock { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public Guid? VendorId { get; set; }
    }

    public sealed class StockDto
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public bool IsLowStock { get; set; }
    }

    public sealed class AdjustStockDto
    {
        public Guid ItemId { get; set; }
        public int Quantity { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal? UnitPrice { get; set; }
        public Guid? VendorId { get; set; }
    }

    public sealed class StockMovementDto
    {
        public Guid Id { get; set; }
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public sealed class VendorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public sealed class CreateVendorDto
    {
        public string Name { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? GSTNumber { get; set; }
    }
}
