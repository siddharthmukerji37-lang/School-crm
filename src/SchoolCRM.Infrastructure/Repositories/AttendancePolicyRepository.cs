using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Attendance;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class AttendancePolicyRepository : GenericRepository<AttendancePolicy>, IAttendancePolicyRepository
{
    public AttendancePolicyRepository(ApplicationDbContext context) : base(context) { }

    public async Task<AttendancePolicy?> GetBySchoolIdAsync(Guid schoolId)
    {
        return await _context.AttendancePolicies
            .FirstOrDefaultAsync(p => p.SchoolId == schoolId && p.IsActive && !p.IsDeleted);
    }
}
