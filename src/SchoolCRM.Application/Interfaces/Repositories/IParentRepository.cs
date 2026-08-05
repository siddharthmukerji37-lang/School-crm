using SchoolCRM.Domain.Entities.Parent;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IParentRepository : IGenericRepository<Parent>
{
    Task<Parent?> GetParentWithDetailsAsync(Guid id);
    Task<Parent?> GetParentByUserIdAsync(Guid userId);
    Task<Parent?> GetParentByCodeAsync(string code);
    Task<(IReadOnlyList<Parent> Items, int TotalCount)> GetPagedParentsAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortOrder);
}
