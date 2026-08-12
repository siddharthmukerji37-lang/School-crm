using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.Exam;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamsController(IExamService examService)
    {
        _examService = examService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ExamDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ExamDto>>>> GetExamsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] Guid? classRoomId = null,
        [FromQuery] Guid? sectionId = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _examService.GetExamsAsync(query, classRoomId, sectionId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExamDto>>> GetExamByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _examService.GetExamByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ExamDto>>> CreateExamAsync(
        [FromBody] CreateExamDto dto)
    {
        var result = await _examService.CreateExamAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExamDto>>> UpdateExamAsync(
        [FromRoute] Guid id,
        [FromBody] CreateExamDto dto)
    {
        var result = await _examService.UpdateExamAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteExamAsync(
        [FromRoute] Guid id)
    {
        var result = await _examService.DeleteExamAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpGet("{id:guid}/schedules")]
    [ProducesResponseType(typeof(ApiResponse<List<ExamScheduleDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<List<ExamScheduleDto>>>> GetExamSchedulesAsync(
        [FromRoute] Guid id)
    {
        var result = await _examService.GetExamScheduleAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/schedules")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> CreateExamScheduleAsync(
        [FromRoute] Guid id,
        [FromBody] List<ExamScheduleDto> schedules)
    {
        var result = await _examService.UpdateExamScheduleAsync(id, schedules);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/marks")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> EnterMarksAsync(
        [FromRoute] Guid id,
        [FromBody] EnterMarksDto dto)
    {
        dto.ExamId = id;
        var result = await _examService.EnterMarksAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id:guid}/results")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ResultDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<ResultDto>>>> GetExamResultsAsync(
        [FromRoute] Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] Guid? classRoomId = null,
        [FromQuery] Guid? sectionId = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _examService.GetResultsAsync(query, id, classRoomId, sectionId);
        return Ok(result);
    }

    [HttpGet("student/{studentId:guid}/results")]
    [ProducesResponseType(typeof(ApiResponse<List<ResultDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ResultDto>>>> GetStudentResultsAsync(
        [FromRoute] Guid studentId)
    {
        var result = await _examService.GetStudentResultsAsync(studentId);
        return Ok(result);
    }

    [HttpPost("{id:guid}/publish")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> PublishResultsAsync(
        [FromRoute] Guid id)
    {
        var exam = await _examService.GetExamByIdAsync(id);
        if (!exam.Success)
            return NotFound(ApiResponse.FailResponse("Exam not found", 404));

        return Ok(ApiResponse.SuccessResponse("Results published successfully"));
    }

    [HttpGet("{id:guid}/questions")]
    [ProducesResponseType(typeof(ApiResponse<List<ExamQuestionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ExamQuestionDto>>>> GetExamQuestionsAsync(
        [FromRoute] Guid id)
    {
        var result = await _examService.GetExamQuestionsAsync(id);
        return Ok(result);
    }

    [HttpPost("{id:guid}/questions")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> AddExamQuestionsAsync(
        [FromRoute] Guid id,
        [FromBody] List<CreateExamQuestionDto> dtos)
    {
        var result = await _examService.AddExamQuestionsAsync(id, dtos);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id:guid}/questions/{questionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExamQuestionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ExamQuestionDto>>> UpdateExamQuestionAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid questionId,
        [FromBody] CreateExamQuestionDto dto)
    {
        var result = await _examService.UpdateExamQuestionAsync(id, questionId, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}/questions/{questionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> DeleteExamQuestionAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid questionId)
    {
        var result = await _examService.DeleteExamQuestionAsync(id, questionId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/question-paper")]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ExamDto>>> UploadQuestionPaperAsync(
        [FromRoute] Guid id,
        [FromBody] UploadQuestionPaperDto dto)
    {
        var result = await _examService.UploadQuestionPaperAsync(id, dto.FileUrl, dto.FileName);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<ExamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ExamDto>>> ApproveExamAsync(
        [FromRoute] Guid id,
        [FromBody] ApproveExamDto dto)
    {
        var result = await _examService.ApproveExamAsync(id, dto.Approved, dto.Reason);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("{id:guid}/submissions")]
    [ProducesResponseType(typeof(ApiResponse<List<ExamSubmissionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ExamSubmissionDto>>>> GetExamSubmissionsAsync(
        [FromRoute] Guid id)
    {
        var result = await _examService.GetSubmissionsByExamAsync(id);
        return Ok(result);
    }

    [HttpGet("submissions/mine")]
    [ProducesResponseType(typeof(ApiResponse<List<ExamSubmissionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ExamSubmissionDto>>>> GetMySubmissionsAsync()
    {
        var result = await _examService.GetMySubmissionsAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}/submissions/student/{studentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExamSubmissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ExamSubmissionDto>>> GetStudentSubmissionAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid studentId)
    {
        var result = await _examService.GetSubmissionAsync(id, studentId);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<ExamSubmissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ExamSubmissionDto>>> SubmitExamAsync(
        [FromRoute] Guid id,
        [FromBody] SubmitExamDto dto)
    {
        dto.ExamId = id;
        var result = await _examService.SubmitExamAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("submissions/{submissionId:guid}/grade")]
    [ProducesResponseType(typeof(ApiResponse<ExamSubmissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ExamSubmissionDto>>> GradeSubmissionAsync(
        [FromRoute] Guid submissionId,
        [FromBody] GradeSubmissionDto dto)
    {
        var result = await _examService.GradeSubmissionAsync(submissionId, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("submissions/{submissionId:guid}/grade-approval")]
    [ProducesResponseType(typeof(ApiResponse<ExamSubmissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ExamSubmissionDto>>> ApproveSubmissionGradingAsync(
        [FromRoute] Guid submissionId,
        [FromBody] GradeApprovalDto dto)
    {
        var result = await _examService.ApproveSubmissionGradingAsync(submissionId, dto.Approved, dto.Reason);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
