using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Employee;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Employee?> GetEmployeeWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.School)
            .Include(e => e.Documents)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee?> GetEmployeeByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(e => e.User)
            .Include(e => e.School)
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> GetPagedEmployeesAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortOrder,
        Guid? departmentId, Guid? designationId, string? status)
    {
        IQueryable<Employee> query = _dbSet
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e =>
                e.EmployeeCode.ToLower().Contains(term) ||
                e.User.FirstName.ToLower().Contains(term) ||
                e.User.LastName.ToLower().Contains(term) ||
                e.User.Email.ToLower().Contains(term));
        }

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (designationId.HasValue)
            query = query.Where(e => e.DesignationId == designationId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.EmployeeStatus>(status, true, out var statusEnum))
            query = query.Where(e => e.Status == statusEnum);

        var totalCount = await query.CountAsync();

        query = sortOrder?.ToLower() == "desc"
            ? query.OrderByDescending(e => sortColumn == "name"
                ? e.User.FirstName
                : sortColumn == "employeeCode"
                    ? e.EmployeeCode
                    : e.CreatedAt.ToString())
            : query.OrderBy(e => sortColumn == "name"
                ? e.User.FirstName
                : sortColumn == "employeeCode"
                    ? e.EmployeeCode
                    : e.CreatedAt.ToString());

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

public class EmployeeLeaveRepository : GenericRepository<EmployeeLeave>, IEmployeeLeaveRepository
{
    public EmployeeLeaveRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<EmployeeLeave>> GetByEmployeeAsync(Guid employeeId)
    {
        return await _dbSet
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.FromDate)
            .ToListAsync();
    }
}

public class EmployeeSalaryRepository : GenericRepository<EmployeeSalary>, IEmployeeSalaryRepository
{
    public EmployeeSalaryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<EmployeeSalary>> GetByEmployeeAndMonthAsync(Guid employeeId, int month, int year)
    {
        return await _dbSet
            .Where(s => s.EmployeeId == employeeId && s.Month == month && s.Year == year)
            .ToListAsync();
    }
}

public class DesignationRepository : GenericRepository<Designation>, IDesignationRepository
{
    public DesignationRepository(ApplicationDbContext context) : base(context) { }

    public new async Task<IReadOnlyList<Designation>> GetAllAsync()
    {
        return await _dbSet.OrderBy(d => d.Level).ToListAsync();
    }
}
