using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.ILibraryService;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    public LibraryController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    #region Books

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookDto>>>> GetBooksAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? category = null,
        [FromQuery] string? author = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _libraryService.GetBooksAsync(query, category, author);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookDto>>> GetBookByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _libraryService.GetBookByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BookDto>>> CreateBookAsync(
        [FromBody] CreateBookDto dto)
    {
        var result = await _libraryService.CreateBookAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookDto>>> UpdateBookAsync(
        [FromRoute] Guid id,
        [FromBody] CreateBookDto dto)
    {
        var result = await _libraryService.UpdateBookAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteBookAsync(
        [FromRoute] Guid id)
    {
        var result = await _libraryService.DeleteBookAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion

    #region Book Issues

    [HttpPost("issue")]
    [ProducesResponseType(typeof(ApiResponse<BookIssueDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BookIssueDto>>> IssueBookAsync(
        [FromBody] BookIssueDto dto)
    {
        var result = await _libraryService.IssueBookAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("return/{issueId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BookIssueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BookIssueDto>>> ReturnBookAsync(
        [FromRoute] Guid issueId,
        [FromQuery] DateTime? returnedDate = null)
    {
        var date = returnedDate ?? DateTime.UtcNow;
        var result = await _libraryService.ReturnBookAsync(issueId, date);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("issued")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookIssueDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<BookIssueDto>>>> GetIssuedBooksAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] Guid? studentId = null,
        [FromQuery] Guid? teacherId = null,
        [FromQuery] bool? overdue = null)
    {
        var query = new PaginationQuery(pageNumber, pageSize, searchTerm)
        {
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _libraryService.GetIssuedBooksAsync(query, studentId, teacherId, overdue);
        return Ok(result);
    }

    [HttpGet("student/{studentId:guid}/issues")]
    [ProducesResponseType(typeof(ApiResponse<List<BookIssueDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<BookIssueDto>>>> GetStudentIssuesAsync(
        [FromRoute] Guid studentId)
    {
        var result = await _libraryService.GetStudentIssuesAsync(studentId);
        return Ok(result);
    }

    [HttpGet("teacher/{teacherId:guid}/issues")]
    [ProducesResponseType(typeof(ApiResponse<List<BookIssueDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<BookIssueDto>>>> GetTeacherIssuesAsync(
        [FromRoute] Guid teacherId)
    {
        var result = await _libraryService.GetTeacherIssuesAsync(teacherId);
        return Ok(result);
    }

    [HttpGet("my-issues")]
    [ProducesResponseType(typeof(ApiResponse<List<BookIssueDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<List<BookIssueDto>>>> GetMyIssuesAsync()
    {
        var result = await _libraryService.GetMyIssuesAsync();
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion
}
