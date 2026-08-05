using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Account;

public class AccountHead : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid SchoolId { get; set; }
}

public class LedgerEntry : BaseEntity
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public TransactionType Type { get; set; }
    public string? PaymentMethod { get; set; }
    public string? BankAccount { get; set; }
    public string? ChequeNumber { get; set; }
    public string? EnteredBy { get; set; }
    public Guid AccountHeadId { get; set; }
    public Guid SchoolId { get; set; }

    public AccountHead AccountHead { get; set; } = null!;
}

public class Income : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Category { get; set; }
    public string? ReceivedFrom { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? Remarks { get; set; }
    public string? ReceivedBy { get; set; }
    public bool IsVerified { get; set; }
    public string? VerifiedBy { get; set; }
    public Guid? AccountHeadId { get; set; }
    public Guid SchoolId { get; set; }

    public AccountHead? AccountHead { get; set; }
}

public class Expense : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Category { get; set; }
    public string? PaidTo { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionReference { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Remarks { get; set; }
    public string? ApprovedBy { get; set; }
    public bool IsApproved { get; set; }
    public Guid? AccountHeadId { get; set; }
    public Guid SchoolId { get; set; }

    public AccountHead? AccountHead { get; set; }
}

public class BankAccount : BaseEntity
{
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? AccountHolderName { get; set; }
    public string? Branch { get; set; }
    public string? IFSCCode { get; set; }
    public string? SWIFTCode { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid SchoolId { get; set; }
}
