using System.ComponentModel.DataAnnotations;

namespace SchoolCRM.Application.DTOs.Fee;

public sealed class FeeStructureDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ClassRoomId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string FeeType { get; set; } = string.Empty;
    public int FineAfterDays { get; set; } = 30;
    public decimal FineAmount { get; set; }
    public DateTime? FineStartDate { get; set; }
    public DateTime? FineEndDate { get; set; }
    public bool IsInstallmentApplicable { get; set; }
    public int? NumberOfInstallments { get; set; }
    public bool IsActive { get; set; }
    public List<FeeInstallmentDto> Installments { get; set; } = new();
    public List<FeeComponentDto> Components { get; set; } = new();
}

public sealed class FeeComponentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}

public sealed class FeeInstallmentDto
{
    public Guid Id { get; set; }
    public Guid FeeStructureId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal? FineAmount { get; set; }
    public int? FineAfterDays { get; set; }
}

public sealed class FeeReceiptDto
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public Guid FeeStructureId { get; set; }
    public string FeeStructureName { get; set; } = string.Empty;
    public string FeeType { get; set; } = string.Empty;
    public Guid? InstallmentId { get; set; }
    public string? InstallmentName { get; set; }
    public decimal Amount { get; set; }
    public decimal FineAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Remarks { get; set; }
    public string PaidBy { get; set; } = string.Empty;
}

public sealed class CollectFeeDto
{
    [Required(ErrorMessage = "Student ID is required")]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Fee structure ID is required")]
    public Guid FeeStructureId { get; set; }

    public Guid? InstallmentId { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Fine amount must be non-negative")]
    public decimal FineAmount { get; set; }

    [Required(ErrorMessage = "Payment method is required")]
    public string PaymentMethod { get; set; } = string.Empty;

    public string? TransactionReference { get; set; }

    [Required(ErrorMessage = "Payment date is required")]
    public DateTime PaymentDate { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [MaxLength(200)]
    public string? ReceivedBy { get; set; }
}

public sealed class FeeSummaryDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string AcademicYearName { get; set; } = string.Empty;
    public decimal TotalFeeAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalPendingAmount { get; set; }
    public decimal TotalFineAmount { get; set; }
    public List<FeeInstallmentStatusDto> Installments { get; set; } = new();
}

public sealed class FeeInstallmentStatusDto
{
    public Guid InstallmentId { get; set; }
    public Guid FeeStructureId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal FineAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
