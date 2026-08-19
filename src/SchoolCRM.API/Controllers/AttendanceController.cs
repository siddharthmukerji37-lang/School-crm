using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Attendance;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost("mark")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> MarkAttendanceAsync(
        [FromBody] MarkAttendanceDto dto)
    {
        var result = await _attendanceService.MarkAttendanceAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("bulk-mark")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> BulkMarkAttendanceAsync(
        [FromBody] BulkMarkAttendanceDto dto)
    {
        var currentDate = dto.StartDate;
        ApiResponse lastResult = ApiResponse.SuccessResponse();

        while (currentDate <= dto.EndDate)
        {
            if (dto.SkipWeekends && (currentDate.DayOfWeek == DayOfWeek.Saturday ||
                                     currentDate.DayOfWeek == DayOfWeek.Sunday))
            {
                currentDate = currentDate.AddDays(1);
                continue;
            }

            var markDto = new MarkAttendanceDto
            {
                Date = currentDate,
                SectionId = Guid.Empty,
                ClassRoomId = Guid.Empty,
                Records = dto.StudentIds.Select(studentId => new AttendanceRecordDto
                {
                    StudentId = studentId,
                    Status = dto.Status,
                    Remarks = dto.Remarks
                }).ToList()
            };

            lastResult = await _attendanceService.MarkAttendanceAsync(markDto);
            if (!lastResult.Success)
                return BadRequest(lastResult);

            currentDate = currentDate.AddDays(1);
        }

        return Ok(ApiResponse.SuccessResponse("Bulk attendance marked successfully"));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AttendanceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<AttendanceDto>>>> GetAttendanceAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] DateTime? date = null,
        [FromQuery] Guid? classRoomId = null,
        [FromQuery] Guid? sectionId = null,
        [FromQuery] string? status = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _attendanceService.GetAttendanceAsync(query, date, classRoomId, sectionId, status);
        return Ok(result);
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AttendanceStatsDto>>> GetAttendanceStatsAsync(
        [FromQuery] DateTime date,
        [FromQuery] Guid? classRoomId = null,
        [FromQuery] Guid? sectionId = null)
    {
        var result = await _attendanceService.GetAttendanceStatsAsync(date, classRoomId, sectionId);
        return Ok(result);
    }

    [HttpGet("student/{studentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AttendanceDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PagedResult<AttendanceDto>>>> GetStudentAttendanceAsync(
        [FromRoute] Guid studentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _attendanceService.GetStudentAttendanceAsync(studentId, query);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<MyAttendanceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<MyAttendanceDto>>> GetMyAttendanceAsync()
    {
        var result = await _attendanceService.GetMyAttendanceAsync();
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("me/clock-in")]
    [ProducesResponseType(typeof(ApiResponse<MyAttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MyAttendanceDto>>> ClockInAsync(
        [FromBody] ClockInDto? dto = null)
    {
        var result = await _attendanceService.ClockInAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("me/clock-out")]
    [ProducesResponseType(typeof(ApiResponse<MyAttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MyAttendanceDto>>> ClockOutAsync(
        [FromBody] ClockOutDto? dto = null)
    {
        var result = await _attendanceService.ClockOutAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("staff/teachers/mark")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> MarkTeacherAttendanceAsync(
        [FromBody] MarkStaffAttendanceDto dto)
    {
        var result = await _attendanceService.MarkTeacherAttendanceAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("staff/employees/mark")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> MarkEmployeeAttendanceAsync(
        [FromBody] MarkStaffAttendanceDto dto)
    {
        var result = await _attendanceService.MarkEmployeeAttendanceAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("staff")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StaffAttendanceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<StaffAttendanceDto>>>> GetStaffAttendanceAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] DateTime? date = null,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _attendanceService.GetStaffAttendanceAsync(query, date, role, status);
        return Ok(result);
    }

    [HttpGet("staff/stats")]
    [ProducesResponseType(typeof(ApiResponse<StaffAttendanceStatsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<StaffAttendanceStatsDto>>> GetStaffAttendanceStatsAsync(
        [FromQuery] DateTime date)
    {
        var result = await _attendanceService.GetStaffAttendanceStatsAsync(date);
        return Ok(result);
    }

    [HttpGet("late-staff")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<LateStaffDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<LateStaffDto>>>> GetLateStaffAsync(
        [FromQuery] DateTime? date = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _attendanceService.GetLateStaffAsync(date, pageNumber, pageSize);
        return Ok(result);
    }
}
