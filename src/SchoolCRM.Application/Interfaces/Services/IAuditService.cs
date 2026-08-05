using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IAuditService
{
    Task<ApiResponse<PagedResult<AuditLogDto>>> GetAuditLogsAsync(
        PaginationQuery query, Guid? schoolId, string? userId, string? action, DateTime? fromDate, DateTime? toDate);

    Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(Guid id);

    Task LogAsync(string userId, string action, string entityType, string entityId, string? details, string? ipAddress);

    Task<ApiResponse<List<AuditLogDto>>> GetRecentActivityAsync(Guid schoolId, int count);

    public sealed class AuditLogDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
