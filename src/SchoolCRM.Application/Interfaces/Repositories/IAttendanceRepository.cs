using SchoolCRM.Domain.Entities.Attendance;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IAttendanceRepository : IGenericRepository<Domain.Entities.Attendance.Attendance>
{
    Task<IReadOnlyList<Domain.Entities.Attendance.Attendance>> GetByDateAsync(DateTime date, Guid? schoolId);
    Task<IReadOnlyList<Domain.Entities.Attendance.Attendance>> GetByStudentAsync(Guid studentId, DateTime startDate, DateTime endDate);
    Task<IReadOnlyList<Domain.Entities.Attendance.Attendance>> GetByTeacherAsync(Guid teacherId, DateTime startDate, DateTime endDate);
    Task<IReadOnlyList<Domain.Entities.Attendance.Attendance>> GetByEmployeeAsync(Guid employeeId, DateTime startDate, DateTime endDate);
    Task<(IReadOnlyList<Domain.Entities.Attendance.Attendance> Items, int TotalCount)> GetPagedAttendanceAsync(
        int pageNumber, int pageSize, DateTime? date, Guid? classRoomId, Guid? sectionId,
        string? status, Guid? schoolId);
    Task<Dictionary<AttendanceStatus, int>> GetAttendanceStatsAsync(DateTime date, Guid? classRoomId, Guid? sectionId);
}
