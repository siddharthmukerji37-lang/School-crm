using System.Linq.Expressions;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IAccountService;

namespace SchoolCRM.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<IncomeDto>>> GetIncomeAsync(
        PaginationQuery query, DateTime? fromDate, DateTime? toDate, Guid? schoolId)
    {
        try
        {
            var incomes = await _unitOfWork.Incomes.GetAllAsync();
            var filtered = incomes.Where(i => !i.IsDeleted).ToList();

            if (fromDate.HasValue)
                filtered = filtered.Where(i => i.Date >= fromDate.Value).ToList();
            if (toDate.HasValue)
                filtered = filtered.Where(i => i.Date <= toDate.Value).ToList();

            var totalCount = filtered.Count;
            var pagedItems = filtered
                .OrderByDescending(i => i.Date)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(i => new IncomeDto
                {
                    Id = i.Id,
                    Title = i.Description,
                    Description = i.Description,
                    Amount = i.Amount,
                    Category = i.Category ?? string.Empty,
                    Date = i.Date,
                    PaymentMethod = i.PaymentMethod.ToString(),
                    ReferenceNumber = i.TransactionReference ?? string.Empty,
                    CreatedBy = i.ReceivedBy ?? string.Empty
                }).ToList();

            return ApiResponse<PagedResult<IncomeDto>>.SuccessResponse(new PagedResult<IncomeDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<IncomeDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<IncomeDto>> GetIncomeByIdAsync(Guid id)
    {
        try
        {
            var income = await _unitOfWork.Incomes.GetByIdAsync(id);
            if (income is null)
                return ApiResponse<IncomeDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<IncomeDto>.SuccessResponse(new IncomeDto
            {
                Id = income.Id,
                Title = income.Description,
                Description = income.Description,
                Amount = income.Amount,
                Category = income.Category ?? string.Empty,
                Date = income.Date,
                PaymentMethod = income.PaymentMethod.ToString(),
                ReferenceNumber = income.TransactionReference ?? string.Empty,
                CreatedBy = income.ReceivedBy ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<IncomeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<IncomeDto>> CreateIncomeAsync(CreateIncomeDto dto)
    {
        try
        {
            var income = new Domain.Entities.Account.Income
            {
                Description = dto.Description,
                Amount = dto.Amount,
                Date = dto.Date,
                Category = dto.Category,
                PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod.Replace(" ", ""), ignoreCase: true),
                TransactionReference = dto.ReferenceNumber,
                SchoolId = dto.SchoolId == Guid.Empty ? Guid.NewGuid() : dto.SchoolId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Incomes.AddAsync(income);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<IncomeDto>.SuccessResponse(new IncomeDto
            {
                Id = income.Id,
                Title = income.Description,
                Description = income.Description,
                Amount = income.Amount,
                Category = income.Category ?? string.Empty,
                Date = income.Date,
                PaymentMethod = income.PaymentMethod.ToString()
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<IncomeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<IncomeDto>> UpdateIncomeAsync(Guid id, CreateIncomeDto dto)
    {
        try
        {
            var income = await _unitOfWork.Incomes.GetByIdAsync(id);
            if (income is null)
                return ApiResponse<IncomeDto>.NotFoundResponse(ApplicationMessages.NotFound);

            income.Description = dto.Description;
            income.Amount = dto.Amount;
            income.Date = dto.Date;
            income.Category = dto.Category;
            income.PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod.Replace(" ", ""), ignoreCase: true);
            income.TransactionReference = dto.ReferenceNumber;
            income.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Incomes.UpdateAsync(income);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<IncomeDto>.SuccessResponse(new IncomeDto
            {
                Id = income.Id,
                Title = income.Description,
                Description = income.Description,
                Amount = income.Amount,
                Category = income.Category ?? string.Empty,
                Date = income.Date,
                PaymentMethod = income.PaymentMethod.ToString()
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<IncomeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteIncomeAsync(Guid id)
    {
        try
        {
            var income = await _unitOfWork.Incomes.GetByIdAsync(id);
            if (income is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            income.IsDeleted = true;
            income.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Incomes.UpdateAsync(income);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<ExpenseDto>>> GetExpenseAsync(
        PaginationQuery query, DateTime? fromDate, DateTime? toDate, Guid? schoolId)
    {
        try
        {
            var expenses = await _unitOfWork.Expenses.GetAllAsync();
            var filtered = expenses.Where(e => !e.IsDeleted).ToList();

            if (fromDate.HasValue)
                filtered = filtered.Where(e => e.Date >= fromDate.Value).ToList();
            if (toDate.HasValue)
                filtered = filtered.Where(e => e.Date <= toDate.Value).ToList();

            var totalCount = filtered.Count;
            var pagedItems = filtered
                .OrderByDescending(e => e.Date)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Title = e.Description,
                    Description = e.Description,
                    Amount = e.Amount,
                    Category = e.Category ?? string.Empty,
                    Date = e.Date,
                    PaymentMethod = e.PaymentMethod.ToString(),
                    Vendor = e.PaidTo ?? string.Empty,
                    ReferenceNumber = e.InvoiceNumber ?? string.Empty,
                    CreatedBy = e.ApprovedBy ?? string.Empty
                }).ToList();

            return ApiResponse<PagedResult<ExpenseDto>>.SuccessResponse(new PagedResult<ExpenseDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<ExpenseDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExpenseDto>> GetExpenseByIdAsync(Guid id)
    {
        try
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense is null)
                return ApiResponse<ExpenseDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<ExpenseDto>.SuccessResponse(new ExpenseDto
            {
                Id = expense.Id,
                Title = expense.Description,
                Description = expense.Description,
                Amount = expense.Amount,
                Category = expense.Category ?? string.Empty,
                Date = expense.Date,
                PaymentMethod = expense.PaymentMethod.ToString(),
                Vendor = expense.PaidTo ?? string.Empty,
                ReferenceNumber = expense.InvoiceNumber ?? string.Empty,
                CreatedBy = expense.ApprovedBy ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            return ApiResponse<ExpenseDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExpenseDto>> CreateExpenseAsync(CreateExpenseDto dto)
    {
        try
        {
            var expense = new Domain.Entities.Account.Expense
            {
                Description = dto.Description,
                Amount = dto.Amount,
                Date = dto.Date,
                Category = dto.Category,
                PaidTo = dto.Vendor,
                PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod.Replace(" ", ""), ignoreCase: true),
                InvoiceNumber = dto.ReferenceNumber,
                SchoolId = dto.SchoolId == Guid.Empty ? Guid.NewGuid() : dto.SchoolId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Expenses.AddAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ExpenseDto>.SuccessResponse(new ExpenseDto
            {
                Id = expense.Id,
                Title = expense.Description,
                Description = expense.Description,
                Amount = expense.Amount,
                Category = expense.Category ?? string.Empty,
                Date = expense.Date,
                PaymentMethod = expense.PaymentMethod.ToString(),
                Vendor = expense.PaidTo ?? string.Empty
            }, ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ExpenseDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<ExpenseDto>> UpdateExpenseAsync(Guid id, CreateExpenseDto dto)
    {
        try
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense is null)
                return ApiResponse<ExpenseDto>.NotFoundResponse(ApplicationMessages.NotFound);

            expense.Description = dto.Description;
            expense.Amount = dto.Amount;
            expense.Date = dto.Date;
            expense.Category = dto.Category;
            expense.PaidTo = dto.Vendor;
            expense.PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod.Replace(" ", ""), ignoreCase: true);
            expense.InvoiceNumber = dto.ReferenceNumber;
            expense.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Expenses.UpdateAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ExpenseDto>.SuccessResponse(new ExpenseDto
            {
                Id = expense.Id,
                Title = expense.Description,
                Description = expense.Description,
                Amount = expense.Amount,
                Category = expense.Category ?? string.Empty,
                Date = expense.Date,
                PaymentMethod = expense.PaymentMethod.ToString(),
                Vendor = expense.PaidTo ?? string.Empty
            }, ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<ExpenseDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteExpenseAsync(Guid id)
    {
        try
        {
            var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
            if (expense is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            expense.IsDeleted = true;
            expense.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Expenses.UpdateAsync(expense);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<LedgerEntryDto>>> GetLedgerAsync(
        DateTime fromDate, DateTime toDate, Guid? schoolId)
    {
        try
        {
            var incomes = await _unitOfWork.Incomes.GetAllAsync();
            var expenses = await _unitOfWork.Expenses.GetAllAsync();

            var entries = new List<LedgerEntryDto>();

            entries.AddRange(incomes
                .Where(i => !i.IsDeleted && i.Date >= fromDate && i.Date <= toDate)
                .Select(i => new LedgerEntryDto
                {
                    Id = i.Id,
                    Type = "Income",
                    Title = i.Description,
                    Credit = i.Amount,
                    Date = i.Date,
                    ReferenceNumber = i.TransactionReference ?? string.Empty
                }));

            entries.AddRange(expenses
                .Where(e => !e.IsDeleted && e.Date >= fromDate && e.Date <= toDate)
                .Select(e => new LedgerEntryDto
                {
                    Id = e.Id,
                    Type = "Expense",
                    Title = e.Description,
                    Debit = e.Amount,
                    Date = e.Date,
                    ReferenceNumber = e.InvoiceNumber ?? string.Empty
                }));

            entries = entries.OrderBy(e => e.Date).ToList();
            decimal balance = 0;
            foreach (var entry in entries)
            {
                balance += entry.Credit - entry.Debit;
                entry.Balance = balance;
            }

            return ApiResponse<List<LedgerEntryDto>>.SuccessResponse(entries);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<LedgerEntryDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<decimal>> GetBalanceAsync(Guid schoolId)
    {
        try
        {
            var (totalIncome, _) = await _unitOfWork.Incomes.GetIncomeSummaryAsync(
                schoolId, DateTime.MinValue, DateTime.UtcNow);
            var (totalExpense, _) = await _unitOfWork.Expenses.GetExpenseSummaryAsync(
                schoolId, DateTime.MinValue, DateTime.UtcNow);

            return ApiResponse<decimal>.SuccessResponse(totalIncome - totalExpense);
        }
        catch (Exception ex)
        {
            return ApiResponse<decimal>.FailResponse(ex.Message);
        }
    }
}
