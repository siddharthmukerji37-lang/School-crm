using SchoolCRM.Domain.Entities.Attendance;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IAttendancePolicyRepository : IGenericRepository<AttendancePolicy>
{
    Task<AttendancePolicy?> GetBySchoolIdAsync(Guid schoolId);
}
