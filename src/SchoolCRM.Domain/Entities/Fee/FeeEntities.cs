using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Fee;

public class FeeHead : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsRefundable { get; set; }
    public bool IsOneTime { get; set; }
    public Guid SchoolId { get; set; }
}

public class FeeStructure : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Guid? FeeHeadId { get; set; }
    public Guid ClassRoomId { get; set; }
    public Guid AcademicYearId { get; set; }
    public bool IsRequired { get; set; } = true;
    public string? Description { get; set; }

    public FeeHead? FeeHead { get; set; }
    public School.ClassRoom ClassRoom { get; set; } = null!;
    public School.AcademicYear AcademicYear { get; set; } = null!;
    public ICollection<FeeInstallment> Installments { get; set; } = new List<FeeInstallment>();
}

public class FeeInstallment : BaseEntity
{
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public FeeStatus Status { get; set; } = FeeStatus.Pending;
    public decimal PaidAmount { get; set; }
    public decimal Fine { get; set; }
    public decimal Discount { get; set; }
    public decimal Scholarship { get; set; }
    public Guid StudentId { get; set; }
    public Guid FeeStructureId { get; set; }

    public Student.Student Student { get; set; } = null!;
    public FeeStructure FeeStructure { get; set; } = null!;
    public ICollection<FeeReceipt> Receipts { get; set; } = new List<FeeReceipt>();
}

public class FeeReceipt : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Fine { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPaid { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? PaymentNotes { get; set; }
    public DateTime PaidAt { get; set; }
    public string? ReceivedBy { get; set; }
    public Guid FeeInstallmentId { get; set; }

    public FeeInstallment FeeInstallment { get; set; } = null!;
}

public class FeeDiscount : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public bool IsPercentage { get; set; }
    public string? ApplicableFor { get; set; }
    public Guid SchoolId { get; set; }
}

public class Scholarship : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public bool IsPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Criteria { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid SchoolId { get; set; }
}
