using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IAuditService;

namespace SchoolCRM.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<AuditLogDto>>> GetAuditLogsAsync(
        PaginationQuery query, Guid? schoolId, string? userId, string? action,
        DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.AuditLogs.GetPagedAuditLogsAsync(
                query.PageNumber, query.PageSize, action, action, fromDate, toDate);

            var dtos = items.Select(MapToDto).ToList();

            var pagedResult = new PagedResult<AuditLogDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return ApiResponse<PagedResult<AuditLogDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<AuditLogDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<AuditLogDto>> GetAuditLogByIdAsync(Guid id)
    {
        try
        {
            var log = await _unitOfWork.AuditLogs.GetByIdAsync(id);
            if (log is null)
                return ApiResponse<AuditLogDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<AuditLogDto>.SuccessResponse(MapToDto(log));
        }
        catch (Exception ex)
        {
            return ApiResponse<AuditLogDto>.FailResponse(ex.Message);
        }
    }

    public async Task LogAsync(string userId, string action, string entityType, string entityId,
        string? details, string? ipAddress)
    {
        try
        {
            var log = new Domain.Entities.Setting.AuditLog
            {
                PerformedBy = userId,
                EntityName = entityType,
                EntityId = entityId,
                Action = Enum.TryParse<AuditAction>(action, out var auditAction)
                    ? auditAction : AuditAction.Create,
                NewValues = details,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // Audit logging should not throw exceptions
        }
    }

    public async Task<ApiResponse<List<AuditLogDto>>> GetRecentActivityAsync(Guid schoolId, int count)
    {
        try
        {
            var (items, _) = await _unitOfWork.AuditLogs.GetPagedAuditLogsAsync(
                1, count, null, null, null, null);

            var dtos = items.OrderByDescending(l => l.Timestamp).Select(MapToDto).ToList();
            return ApiResponse<List<AuditLogDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<AuditLogDto>>.FailResponse(ex.Message);
        }
    }

    private static AuditLogDto MapToDto(Domain.Entities.Setting.AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            UserId = log.PerformedBy,
            UserName = log.PerformedBy,
            Action = log.Action.ToString(),
            EntityType = log.EntityName,
            EntityId = log.EntityId,
            OldValues = log.OldValues,
            NewValues = log.NewValues,
            IpAddress = log.IpAddress,
            Timestamp = log.Timestamp
        };
    }
}
