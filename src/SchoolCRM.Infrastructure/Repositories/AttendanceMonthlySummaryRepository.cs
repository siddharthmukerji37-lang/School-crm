using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Attendance;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class AttendanceMonthlySummaryRepository : GenericRepository<AttendanceMonthlySummary>, IAttendanceMonthlySummaryRepository
{
    public AttendanceMonthlySummaryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<AttendanceMonthlySummary?> GetByUserAndMonthAsync(Guid userId, int month, int year)
    {
        return await _context.AttendanceMonthlySummaries
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Month == month && s.Year == year && !s.IsDeleted);
    }

    public async Task<IReadOnlyList<AttendanceMonthlySummary>> GetByMonthAsync(int month, int year)
    {
        return await _context.AttendanceMonthlySummaries
            .Include(s => s.User)
            .Where(s => s.Month == month && s.Year == year && !s.IsDeleted)
            .OrderByDescending(s => s.TotalLateCount)
            .ToListAsync();
    }
}
