using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IAttendancePolicyService
{
    Task<ApiResponse<AttendancePolicyDto>> GetPolicyAsync(Guid schoolId);
    Task<ApiResponse<AttendancePolicyDto>> UpdatePolicyAsync(Guid schoolId, UpdateAttendancePolicyDto dto);
    Task<ApiResponse<PagedResult<AttendanceMonthlySummaryDto>>> GetMonthlySummariesAsync(int month, int year, int pageNumber, int pageSize);
}
