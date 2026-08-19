using SchoolCRM.Domain.Entities.Attendance;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IAttendanceMonthlySummaryRepository : IGenericRepository<AttendanceMonthlySummary>
{
    Task<AttendanceMonthlySummary?> GetByUserAndMonthAsync(Guid userId, int month, int year);
    Task<IReadOnlyList<AttendanceMonthlySummary>> GetByMonthAsync(int month, int year);
}
