using SchoolCRM.Domain.Entities.Leave;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface ILeaveCalendarRepository : IGenericRepository<LeaveCalendar>
{
    Task<LeaveCalendar?> GetActiveCalendarAsync(Guid schoolId, int year);
}

public interface ILeaveTypeRepository : IGenericRepository<LeaveType>
{
    Task<IReadOnlyList<LeaveType>> GetBySchoolAsync(Guid schoolId);
}

public interface ILeaveTypeConfigRepository : IGenericRepository<LeaveTypeConfig>
{
    Task<IReadOnlyList<LeaveTypeConfig>> GetByCalendarAsync(Guid calendarId);
    Task<IReadOnlyList<LeaveTypeConfig>> GetApplicableForUserAsync(Guid calendarId, string userType, string gender);
}

public interface ILeaveBalanceRepository : IGenericRepository<LeaveBalance>
{
    Task<IReadOnlyList<LeaveBalance>> GetByUserAsync(Guid userId, Guid calendarId);
    Task<LeaveBalance?> GetByUserAndTypeAsync(Guid userId, Guid leaveTypeId, Guid calendarId);
}

public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
{
    Task<IReadOnlyList<LeaveRequest>> GetByUserAsync(Guid userId);
    Task<IReadOnlyList<LeaveRequest>> GetAllWithDetailsAsync();
    Task<LeaveRequest?> GetByIdWithDetailsAsync(Guid id);
}

public interface ILeaveRequestDayRepository : IGenericRepository<LeaveRequestDay>
{
    Task<IReadOnlyList<LeaveRequestDay>> GetByRequestAsync(Guid leaveRequestId);
    Task<bool> HasConflictAsync(Guid userId, DateTime fromDate, DateTime toDate, Guid? excludeRequestId = null);
}
