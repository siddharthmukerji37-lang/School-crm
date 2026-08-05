using SchoolCRM.Application.DTOs.Dashboard;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<ApiResponse<DashboardDto>> GetDashboardStatsAsync(Guid schoolId, string userRole);

    Task<ApiResponse<List<ChartDataDto>>> GetAttendanceChartDataAsync(Guid schoolId, int months);

    Task<ApiResponse<List<ChartDataDto>>> GetFeeChartDataAsync(Guid schoolId, int months);
}
