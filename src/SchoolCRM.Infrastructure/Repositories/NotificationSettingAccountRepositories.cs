using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Notification;
using SchoolCRM.Domain.Entities.Setting;
using SchoolCRM.Domain.Entities.Account;
using SchoolCRM.Domain.Entities.Inventory;
using SchoolCRM.Infrastructure.Data;
using Notification = SchoolCRM.Domain.Entities.Notification.Notification;

namespace SchoolCRM.Infrastructure.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Notification>> GetByUserAsync(Guid userId, bool unreadOnly = false)
    {
        var query = _dbSet.Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}

public class AnnouncementRepository : GenericRepository<Announcement>, IAnnouncementRepository
{
    public AnnouncementRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Announcement>> GetPublishedAsync(Guid schoolId)
    {
        return await _dbSet
            .Where(a => a.SchoolId == schoolId
                && a.IsPublished
                && (!a.ExpiryDate.HasValue || a.ExpiryDate.Value >= DateTime.UtcNow))
            .OrderByDescending(a => a.PublishDate)
            .ToListAsync();
    }
}

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAuditLogsAsync(
        int pageNumber, int pageSize, string? entityName, string? action, DateTime? startDate, DateTime? endDate)
    {
        IQueryable<AuditLog> query = _dbSet;

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(al => al.EntityName == entityName);

        if (!string.IsNullOrWhiteSpace(action) && Enum.TryParse<SchoolCRM.Domain.Enums.AuditAction>(action, true, out var actionEnum))
            query = query.Where(al => al.Action == actionEnum);

        if (startDate.HasValue)
            query = query.Where(al => al.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(al => al.Timestamp <= endDate.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(al => al.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

public class AccountHeadRepository : GenericRepository<AccountHead>, IAccountHeadRepository
{
    public AccountHeadRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AccountHead>> GetByTypeAsync(SchoolCRM.Domain.Enums.TransactionType type, Guid schoolId)
    {
        return await _dbSet
            .Where(ah => ah.Type == type && ah.SchoolId == schoolId && ah.IsActive)
            .OrderBy(ah => ah.Name)
            .ToListAsync();
    }
}

public class IncomeRepository : GenericRepository<Income>, IIncomeRepository
{
    public IncomeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(decimal TotalIncome, int Count)> GetIncomeSummaryAsync(Guid schoolId, DateTime startDate, DateTime endDate)
    {
        var result = await _dbSet
            .Where(i => i.SchoolId == schoolId
                && i.Date.Date >= startDate.Date
                && i.Date.Date <= endDate.Date)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalIncome = g.Sum(i => i.Amount),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        return result is null ? (0m, 0) : (result.TotalIncome, result.Count);
    }
}

public class ExpenseRepository : GenericRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(decimal TotalExpense, int Count)> GetExpenseSummaryAsync(Guid schoolId, DateTime startDate, DateTime endDate)
    {
        var result = await _dbSet
            .Where(e => e.SchoolId == schoolId
                && e.Date.Date >= startDate.Date
                && e.Date.Date <= endDate.Date)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalExpense = g.Sum(e => e.Amount),
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        return result is null ? (0m, 0) : (result.TotalExpense, result.Count);
    }
}

public class InventoryItemRepository : GenericRepository<InventoryItem>, IInventoryItemRepository
{
    public InventoryItemRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<InventoryItem>> GetLowStockItemsAsync(Guid schoolId)
    {
        return await _dbSet
            .Include(ii => ii.Category)
            .Include(ii => ii.Vendor)
            .Where(ii => ii.SchoolId == schoolId
                && ii.IsActive
                && ii.CurrentStock <= ii.MinimumStock)
            .OrderBy(ii => ii.CurrentStock)
            .ToListAsync();
    }
}
