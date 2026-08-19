using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Attendance;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class SalaryDeductionRepository : GenericRepository<SalaryDeduction>, ISalaryDeductionRepository
{
    public SalaryDeductionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<SalaryDeduction>> GetByUserAsync(Guid userId)
    {
        return await _context.SalaryDeductions
            .Include(d => d.User)
            .Include(d => d.Attendance)
            .Where(d => d.UserId == userId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SalaryDeduction>> GetByStatusAsync(SalaryDeductionStatus status)
    {
        return await _context.SalaryDeductions
            .Include(d => d.User)
            .Include(d => d.Attendance)
            .Where(d => d.Status == status && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<SalaryDeduction>> GetByMonthAsync(int month, int year)
    {
        return await _context.SalaryDeductions
            .Include(d => d.User)
            .Include(d => d.Attendance)
            .Where(d => d.PayrollMonth == month && d.PayrollYear == year && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsForAttendanceAsync(Guid attendanceId)
    {
        return await _context.SalaryDeductions
            .AnyAsync(d => d.AttendanceId == attendanceId && !d.IsDeleted);
    }
}
