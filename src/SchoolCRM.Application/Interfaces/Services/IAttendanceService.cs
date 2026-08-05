using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IAttendanceService
{
    Task<ApiResponse> MarkAttendanceAsync(MarkAttendanceDto dto);

    Task<ApiResponse<PagedResult<AttendanceDto>>> GetAttendanceAsync(
        PaginationQuery query, DateTime? date, Guid? classRoomId, Guid? sectionId, string? status);

    Task<ApiResponse<AttendanceStatsDto>> GetAttendanceStatsAsync(
        DateTime date, Guid? classRoomId, Guid? sectionId);

    Task<ApiResponse<PagedResult<AttendanceDto>>> GetStudentAttendanceAsync(
        Guid studentId, PaginationQuery query);
}
