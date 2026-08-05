using SchoolCRM.Domain.Entities.Notification;
using SchoolCRM.Domain.Entities.Setting;
using SchoolCRM.Domain.Entities.Account;
using SchoolCRM.Domain.Entities.Inventory;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface INotificationRepository : IGenericRepository<Domain.Entities.Notification.Notification>
{
    Task<IReadOnlyList<Domain.Entities.Notification.Notification>> GetByUserAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
}

public interface IAnnouncementRepository : IGenericRepository<Announcement>
{
    Task<IReadOnlyList<Announcement>> GetPublishedAsync(Guid schoolId);
}

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAuditLogsAsync(
        int pageNumber, int pageSize, string? entityName, string? action, DateTime? startDate, DateTime? endDate);
}

public interface IAccountHeadRepository : IGenericRepository<AccountHead>
{
    Task<IReadOnlyList<AccountHead>> GetByTypeAsync(TransactionType type, Guid schoolId);
}

public interface IIncomeRepository : IGenericRepository<Income>
{
    Task<(decimal TotalIncome, int Count)> GetIncomeSummaryAsync(Guid schoolId, DateTime startDate, DateTime endDate);
}

public interface IExpenseRepository : IGenericRepository<Expense>
{
    Task<(decimal TotalExpense, int Count)> GetExpenseSummaryAsync(Guid schoolId, DateTime startDate, DateTime endDate);
}

public interface IInventoryItemRepository : IGenericRepository<InventoryItem>
{
    Task<IReadOnlyList<InventoryItem>> GetLowStockItemsAsync(Guid schoolId);
}
