using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Parent;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class ParentRepository : GenericRepository<Parent>, IParentRepository
{
    public ParentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Parent?> GetParentWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Students)
                .ThenInclude(s => s.Section)
                    .ThenInclude(sec => sec.ClassRoom)
            .Include(p => p.GuardianDetails)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Parent?> GetParentByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<Parent?> GetParentByCodeAsync(string code)
    {
        return await _dbSet
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.ParentCode == code);
    }

    public async Task<(IReadOnlyList<Parent> Items, int TotalCount)> GetPagedParentsAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortOrder)
    {
        IQueryable<Parent> query = _dbSet
            .Include(p => p.User)
            .Include(p => p.Students);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.ParentCode.ToLower().Contains(term) ||
                p.User.FirstName.ToLower().Contains(term) ||
                p.User.LastName.ToLower().Contains(term) ||
                p.User.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        query = sortOrder?.ToLower() == "desc"
            ? query.OrderByDescending(p => sortColumn == "name"
                ? p.User.FirstName
                : sortColumn == "parentCode"
                    ? p.ParentCode
                    : p.CreatedAt.ToString())
            : query.OrderBy(p => sortColumn == "name"
                ? p.User.FirstName
                : sortColumn == "parentCode"
                    ? p.ParentCode
                    : p.CreatedAt.ToString());

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
