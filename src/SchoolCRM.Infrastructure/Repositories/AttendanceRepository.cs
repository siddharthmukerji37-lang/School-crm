using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using Attendance = SchoolCRM.Domain.Entities.Attendance.Attendance;

namespace SchoolCRM.Infrastructure.Repositories;

public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Attendance>> GetByDateAsync(DateTime date, Guid? schoolId)
    {
        var query = _dbSet
            .Include(a => a.Student)
                .ThenInclude(s => s!.User)
            .Include(a => a.Teacher)
                .ThenInclude(t => t!.User)
            .Include(a => a.Employee)
                .ThenInclude(e => e!.User)
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Designation)
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
            .Where(a => a.Date.Date == date.Date);

        if (schoolId.HasValue)
            query = query.Where(a => a.SchoolId == schoolId.Value);

        return await query.OrderBy(a => a.Date).ToListAsync();
    }

    public async Task<IReadOnlyList<Attendance>> GetByStudentAsync(Guid studentId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(a => a.Student)
            .Where(a => a.StudentId == studentId
                && a.Date.Date >= startDate.Date
                && a.Date.Date <= endDate.Date)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Attendance>> GetByTeacherAsync(Guid teacherId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(a => a.Teacher)
            .Where(a => a.TeacherId == teacherId
                && a.Date.Date >= startDate.Date
                && a.Date.Date <= endDate.Date)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Attendance>> GetByEmployeeAsync(Guid employeeId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(a => a.Employee)
            .Where(a => a.EmployeeId == employeeId
                && a.Date.Date >= startDate.Date
                && a.Date.Date <= endDate.Date)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Attendance> Items, int TotalCount)> GetPagedAttendanceAsync(
        int pageNumber, int pageSize, DateTime? date, Guid? classRoomId, Guid? sectionId,
        string? status, Guid? schoolId)
    {
        IQueryable<Attendance> query = _dbSet
            .Include(a => a.Student)
                .ThenInclude(s => s!.Section)
                    .ThenInclude(sec => sec!.ClassRoom)
            .Include(a => a.Student)
                .ThenInclude(s => s!.User)
            .Include(a => a.Teacher)
                .ThenInclude(t => t!.User)
            .Include(a => a.Employee)
                .ThenInclude(e => e!.User);

        if (date.HasValue)
            query = query.Where(a => a.Date.Date == date.Value.Date);

        if (classRoomId.HasValue)
            query = query.Where(a => a.Student != null && a.Student.Section.ClassRoomId == classRoomId.Value);

        if (sectionId.HasValue)
            query = query.Where(a => a.Student != null && a.Student.SectionId == sectionId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AttendanceStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        if (schoolId.HasValue)
            query = query.Where(a => a.SchoolId == schoolId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Student != null ? a.Student.RollNumber : string.Empty)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Dictionary<AttendanceStatus, int>> GetAttendanceStatsAsync(
        DateTime date, Guid? classRoomId, Guid? sectionId)
    {
        IQueryable<Attendance> query = _dbSet
            .Include(a => a.Student)
            .Where(a => a.Date.Date == date.Date);

        if (classRoomId.HasValue)
            query = query.Where(a => a.Student != null && a.Student.Section.ClassRoomId == classRoomId.Value);

        if (sectionId.HasValue)
            query = query.Where(a => a.Student != null && a.Student.SectionId == sectionId.Value);

        return await query
            .GroupBy(a => a.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }
}
