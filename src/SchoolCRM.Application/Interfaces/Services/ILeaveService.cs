using SchoolCRM.Application.DTOs.Leave;
using SchoolCRM.Application.DTOs;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface ILeaveService
{
    // User APIs
    Task<ApiResponse<List<LeaveTypeConfigDto>>> GetLeaveTypesForUserAsync();
    Task<ApiResponse<List<LeaveBalanceDto>>> GetMyLeaveBalanceAsync();
    Task<ApiResponse<LeaveRequestDto>> ApplyLeaveAsync(ApplyLeaveDto dto);
    Task<ApiResponse<List<LeaveRequestDto>>> GetMyLeaveRequestsAsync();
    Task<ApiResponse> CancelLeaveAsync(Guid requestId);

    // Admin APIs
    Task<ApiResponse<LeaveCalendarDto>> CreateLeaveCalendarAsync(CreateLeaveCalendarDto dto);
    Task<ApiResponse<List<LeaveCalendarDto>>> GetLeaveCalendarsAsync();
    Task<ApiResponse<LeaveCalendarDto>> GetActiveLeaveCalendarAsync();

    Task<ApiResponse<LeaveTypeDto>> CreateLeaveTypeAsync(CreateLeaveTypeDto dto);
    Task<ApiResponse<List<LeaveTypeDto>>> GetLeaveTypesAsync();
    Task<ApiResponse<LeaveTypeDto>> UpdateLeaveTypeAsync(Guid id, CreateLeaveTypeDto dto);

    Task<ApiResponse<LeaveTypeConfigDto>> CreateLeaveTypeConfigAsync(Guid calendarId, CreateLeaveTypeConfigDto dto);
    Task<ApiResponse<LeaveTypeConfigDto>> UpdateLeaveTypeConfigAsync(Guid id, UpdateLeaveTypeConfigDto dto);
    Task<ApiResponse<List<LeaveTypeConfigDto>>> GetLeaveTypeConfigsAsync(Guid calendarId);

    Task<ApiResponse<List<LeaveBalanceDto>>> GetUserLeaveBalancesAsync(Guid userId);
    Task<ApiResponse> InitializeLeaveBalancesAsync(Guid calendarId);

    Task<ApiResponse<PagedResult<LeaveRequestDto>>> GetAllLeaveRequestsAsync(PaginationQuery query);
    Task<ApiResponse<LeaveRequestDto>> ApproveLeaveAsync(Guid requestId, ApproveLeaveDto dto);
    Task<ApiResponse<LeaveRequestDto>> RejectLeaveAsync(Guid requestId, RejectLeaveDto dto);

    Task<ApiResponse<List<LeaveRequestDto>>> GetPendingRequestsAsync();
}
