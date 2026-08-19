using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Leave;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class LeaveCalendarRepository : GenericRepository<LeaveCalendar>, ILeaveCalendarRepository
{
    public LeaveCalendarRepository(ApplicationDbContext context) : base(context) { }

    public async Task<LeaveCalendar?> GetActiveCalendarAsync(Guid schoolId, int year)
    {
        return await _dbSet.FirstOrDefaultAsync(c =>
            c.SchoolId == schoolId && c.Year == year && c.IsActive && !c.IsDeleted);
    }
}

public class LeaveTypeRepository : GenericRepository<LeaveType>, ILeaveTypeRepository
{
    public LeaveTypeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LeaveType>> GetBySchoolAsync(Guid schoolId)
    {
        return await _dbSet.Where(t => t.SchoolId == schoolId && t.IsActive && !t.IsDeleted)
            .OrderBy(t => t.Name).ToListAsync();
    }
}

public class LeaveTypeConfigRepository : GenericRepository<LeaveTypeConfig>, ILeaveTypeConfigRepository
{
    public LeaveTypeConfigRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LeaveTypeConfig>> GetByCalendarAsync(Guid calendarId)
    {
        return await _dbSet
            .Include(c => c.LeaveType)
            .Where(c => c.LeaveCalendarId == calendarId && c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.LeaveType.Name).ToListAsync();
    }

    public async Task<IReadOnlyList<LeaveTypeConfig>> GetApplicableForUserAsync(Guid calendarId, string userType, string gender)
    {
        return await _dbSet
            .Include(c => c.LeaveType)
            .Where(c => c.LeaveCalendarId == calendarId && c.IsActive && !c.IsDeleted
                && (c.ApplicableUserType == "Both" || c.ApplicableUserType == userType)
                && (c.ApplicableGender.ToString() == "Male" || c.ApplicableGender.ToString() == gender || c.ApplicableGender.ToString() == "Other"))
            .OrderBy(c => c.LeaveType.Name).ToListAsync();
    }
}

public class LeaveBalanceRepository : GenericRepository<LeaveBalance>, ILeaveBalanceRepository
{
    public LeaveBalanceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LeaveBalance>> GetByUserAsync(Guid userId, Guid calendarId)
    {
        return await _dbSet
            .Include(b => b.LeaveType)
            .Where(b => b.UserId == userId && b.LeaveCalendarId == calendarId && !b.IsDeleted)
            .OrderBy(b => b.LeaveType.Name).ToListAsync();
    }

    public async Task<LeaveBalance?> GetByUserAndTypeAsync(Guid userId, Guid leaveTypeId, Guid calendarId)
    {
        return await _dbSet.FirstOrDefaultAsync(b =>
            b.UserId == userId && b.LeaveTypeId == leaveTypeId && b.LeaveCalendarId == calendarId && !b.IsDeleted);
    }
}

public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
{
    public LeaveRequestRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LeaveRequest>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(r => r.LeaveType)
            .Include(r => r.LeaveCalendar)
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<IReadOnlyList<LeaveRequest>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(r => r.LeaveType)
            .Include(r => r.LeaveCalendar)
            .Where(r => !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<LeaveRequest?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(r => r.LeaveType)
            .Include(r => r.LeaveCalendar)
            .Include(r => r.LeaveRequestDays)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
    }
}

public class LeaveRequestDayRepository : GenericRepository<LeaveRequestDay>, ILeaveRequestDayRepository
{
    public LeaveRequestDayRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<LeaveRequestDay>> GetByRequestAsync(Guid leaveRequestId)
    {
        return await _dbSet.Where(d => d.LeaveRequestId == leaveRequestId && !d.IsDeleted)
            .OrderBy(d => d.LeaveDate).ToListAsync();
    }

    public async Task<bool> HasConflictAsync(Guid userId, DateTime fromDate, DateTime toDate, Guid? excludeRequestId = null)
    {
        return await _dbSet.AnyAsync(d =>
            d.LeaveRequest.UserId == userId &&
            !d.LeaveRequest.IsDeleted &&
            (d.Status == Domain.Enums.LeaveStatus.Pending || d.Status == Domain.Enums.LeaveStatus.Approved) &&
            d.LeaveDate >= fromDate.Date &&
            d.LeaveDate <= toDate.Date &&
            (excludeRequestId == null || d.LeaveRequestId != excludeRequestId));
    }
}
