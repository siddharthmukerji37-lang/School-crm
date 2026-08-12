using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IHomeworkService;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HomeworkController : ControllerBase
{
    private readonly IHomeworkService _homeworkService;

    public HomeworkController(IHomeworkService homeworkService)
    {
        _homeworkService = homeworkService;
    }

    #region Homework

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<HomeworkDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<HomeworkDto>>>> GetHomeworkAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] Guid? classRoomId = null,
        [FromQuery] Guid? sectionId = null,
        [FromQuery] Guid? subjectId = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _homeworkService.GetHomeworkAsync(query, classRoomId, sectionId, subjectId, fromDate, toDate);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HomeworkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HomeworkDto>>> GetHomeworkByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _homeworkService.GetHomeworkByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HomeworkDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HomeworkDto>>> CreateHomeworkAsync(
        [FromBody] CreateHomeworkDto dto)
    {
        var result = await _homeworkService.CreateHomeworkAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HomeworkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HomeworkDto>>> UpdateHomeworkAsync(
        [FromRoute] Guid id,
        [FromBody] CreateHomeworkDto dto)
    {
        var result = await _homeworkService.UpdateHomeworkAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteHomeworkAsync(
        [FromRoute] Guid id)
    {
        var result = await _homeworkService.DeleteHomeworkAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion

    #region Assignments

    [HttpGet("assignments")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AssignmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<AssignmentDto>>>> GetAssignmentsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] Guid? studentId = null,
        [FromQuery] string? status = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _homeworkService.GetAssignmentsAsync(query, studentId, status);
        return Ok(result);
    }

    [HttpGet("assignments/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> GetAssignmentByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _homeworkService.GetAssignmentByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> SubmitHomeworkAsync(
        [FromRoute] Guid id,
        [FromBody] SubmitAssignmentDto dto)
    {
        dto.HomeworkId = id;
        var result = await _homeworkService.SubmitAssignmentAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/review")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> ReviewSubmissionAsync(
        [FromRoute] Guid id,
        [FromBody] GradeAssignmentDto dto)
    {
        var result = await _homeworkService.GradeAssignmentAsync(id, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<AssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> RejectSubmissionAsync(
        [FromRoute] Guid id,
        [FromBody] GradeAssignmentDto dto)
    {
        var result = await _homeworkService.RejectAssignmentAsync(id, dto.Remarks);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<HomeworkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<HomeworkDto>>> ApproveHomeworkAsync(
        [FromRoute] Guid id,
        [FromBody] ApproveHomeworkDto dto)
    {
        var result = await _homeworkService.ApproveHomeworkAsync(id, dto.Approved, dto.Reason);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/submit-for-approval")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> SubmitForApprovalAsync(
        [FromRoute] Guid id)
    {
        var result = await _homeworkService.RequestHomeworkApprovalAsync(id);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion
}
