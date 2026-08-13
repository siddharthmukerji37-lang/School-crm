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

    Task<ApiResponse> MarkTeacherAttendanceAsync(MarkStaffAttendanceDto dto);

    Task<ApiResponse> MarkEmployeeAttendanceAsync(MarkStaffAttendanceDto dto);

    Task<ApiResponse<PagedResult<StaffAttendanceDto>>> GetStaffAttendanceAsync(
        PaginationQuery query, DateTime? date, string? role, string? status);

    Task<ApiResponse<StaffAttendanceStatsDto>> GetStaffAttendanceStatsAsync(DateTime date);

    Task<ApiResponse<MyAttendanceDto>> GetMyAttendanceAsync();

    Task<ApiResponse<MyAttendanceDto>> ClockInAsync();

    Task<ApiResponse<MyAttendanceDto>> ClockOutAsync();
}
