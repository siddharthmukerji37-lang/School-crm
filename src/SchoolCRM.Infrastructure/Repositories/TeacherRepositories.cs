using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Teacher;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
{
    public TeacherRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Teacher?> GetTeacherWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.User)
            .Include(t => t.Department)
            .Include(t => t.School)
            .Include(t => t.Documents)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Teacher?> GetTeacherByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(t => t.User)
            .Include(t => t.Department)
            .Include(t => t.School)
            .FirstOrDefaultAsync(t => t.UserId == userId);
    }

    public async Task<Teacher?> GetTeacherByEmployeeCodeAsync(string employeeCode)
    {
        return await _dbSet
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.EmployeeCode == employeeCode);
    }

    public async Task<(IReadOnlyList<Teacher> Items, int TotalCount)> GetPagedTeachersAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortOrder,
        Guid? departmentId, Guid? schoolId, string? status)
    {
        IQueryable<Teacher> query = _dbSet
            .Include(t => t.User)
            .Include(t => t.Department);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(t =>
                t.EmployeeCode.ToLower().Contains(term) ||
                t.User.FirstName.ToLower().Contains(term) ||
                t.User.LastName.ToLower().Contains(term) ||
                t.User.Email.ToLower().Contains(term));
        }

        if (departmentId.HasValue)
            query = query.Where(t => t.DepartmentId == departmentId.Value);

        if (schoolId.HasValue)
            query = query.Where(t => t.SchoolId == schoolId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.TeacherStatus>(status, true, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);

        var totalCount = await query.CountAsync();

        query = sortOrder?.ToLower() == "desc"
            ? query.OrderByDescending(t => sortColumn == "name"
                ? t.User.FirstName
                : sortColumn == "employeeCode"
                    ? t.EmployeeCode
                    : t.CreatedAt.ToString())
            : query.OrderBy(t => sortColumn == "name"
                ? t.User.FirstName
                : sortColumn == "employeeCode"
                    ? t.EmployeeCode
                    : t.CreatedAt.ToString());

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}

public class TeacherLeaveRepository : GenericRepository<TeacherLeave>, ITeacherLeaveRepository
{
    public TeacherLeaveRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TeacherLeave>> GetByTeacherAsync(Guid teacherId)
    {
        return await _dbSet
            .Where(l => l.TeacherId == teacherId)
            .OrderByDescending(l => l.FromDate)
            .ToListAsync();
    }
}

public class TeacherSalaryRepository : GenericRepository<TeacherSalary>, ITeacherSalaryRepository
{
    public TeacherSalaryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<TeacherSalary>> GetByTeacherAndMonthAsync(Guid teacherId, int month, int year)
    {
        return await _dbSet
            .Where(s => s.TeacherId == teacherId && s.Month == month && s.Year == year)
            .ToListAsync();
    }
}
