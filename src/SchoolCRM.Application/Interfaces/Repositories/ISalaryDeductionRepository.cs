using SchoolCRM.Domain.Entities.Attendance;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface ISalaryDeductionRepository : IGenericRepository<SalaryDeduction>
{
    Task<IReadOnlyList<SalaryDeduction>> GetByUserAsync(Guid userId);
    Task<IReadOnlyList<SalaryDeduction>> GetByStatusAsync(SalaryDeductionStatus status);
    Task<IReadOnlyList<SalaryDeduction>> GetByMonthAsync(int month, int year);
    Task<bool> ExistsForAttendanceAsync(Guid attendanceId);
}
