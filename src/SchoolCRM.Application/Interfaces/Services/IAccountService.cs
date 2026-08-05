using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IAccountService
{
    Task<ApiResponse<PagedResult<IncomeDto>>> GetIncomeAsync(
        PaginationQuery query, DateTime? fromDate, DateTime? toDate, Guid? schoolId);

    Task<ApiResponse<IncomeDto>> GetIncomeByIdAsync(Guid id);

    Task<ApiResponse<IncomeDto>> CreateIncomeAsync(CreateIncomeDto dto);

    Task<ApiResponse<IncomeDto>> UpdateIncomeAsync(Guid id, CreateIncomeDto dto);

    Task<ApiResponse> DeleteIncomeAsync(Guid id);

    Task<ApiResponse<PagedResult<ExpenseDto>>> GetExpenseAsync(
        PaginationQuery query, DateTime? fromDate, DateTime? toDate, Guid? schoolId);

    Task<ApiResponse<ExpenseDto>> GetExpenseByIdAsync(Guid id);

    Task<ApiResponse<ExpenseDto>> CreateExpenseAsync(CreateExpenseDto dto);

    Task<ApiResponse<ExpenseDto>> UpdateExpenseAsync(Guid id, CreateExpenseDto dto);

    Task<ApiResponse> DeleteExpenseAsync(Guid id);

    Task<ApiResponse<List<LedgerEntryDto>>> GetLedgerAsync(
        DateTime fromDate, DateTime toDate, Guid? schoolId);

    Task<ApiResponse<decimal>> GetBalanceAsync(Guid schoolId);

    public sealed class IncomeDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public sealed class CreateIncomeDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public Guid SchoolId { get; set; }
    }

    public sealed class ExpenseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public sealed class CreateExpenseDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Vendor { get; set; }
        public string? ReferenceNumber { get; set; }
        public Guid SchoolId { get; set; }
    }

    public sealed class LedgerEntryDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public DateTime Date { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
    }
}
