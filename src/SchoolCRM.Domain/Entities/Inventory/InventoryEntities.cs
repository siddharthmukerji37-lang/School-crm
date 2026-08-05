using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Inventory;

public class InventoryCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid SchoolId { get; set; }
}

public class Vendor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? GSTNumber { get; set; }
    public string? PANNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid SchoolId { get; set; }
}

public class InventoryItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CurrentStock { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string? Location { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CategoryId { get; set; }
    public Guid? VendorId { get; set; }
    public Guid SchoolId { get; set; }

    public InventoryCategory Category { get; set; } = null!;
    public Vendor? Vendor { get; set; }
    public ICollection<StockTransaction> Transactions { get; set; } = new List<StockTransaction>();
}

public class StockTransaction : BaseEntity
{
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? IssuedTo { get; set; }
    public string? ReceivedFrom { get; set; }
    public string? Remarks { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? PerformedBy { get; set; }
    public Guid ItemId { get; set; }

    public InventoryItem Item { get; set; } = null!;
}
