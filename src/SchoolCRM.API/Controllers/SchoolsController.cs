using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.DTOs.School;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchoolsController : ControllerBase
{
    private readonly ISchoolService _schoolService;

    public SchoolsController(ISchoolService schoolService)
    {
        _schoolService = schoolService;
    }

    #region Schools

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SchoolDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<SchoolDto>>>> GetSchoolsAsync(
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

        var result = await _schoolService.GetSchoolsAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SchoolDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SchoolDto>>> GetSchoolByIdAsync(
        [FromRoute] Guid id)
    {
        var result = await _schoolService.GetSchoolByIdAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SchoolDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SchoolDto>>> CreateSchoolAsync(
        [FromBody] SchoolDto dto)
    {
        var result = await _schoolService.CreateSchoolAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SchoolDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SchoolDto>>> UpdateSchoolAsync(
        [FromRoute] Guid id,
        [FromBody] SchoolDto dto)
    {
        var result = await _schoolService.UpdateSchoolAsync(id, dto);
        if (!result.Success && result.StatusCode == 404)
            return NotFound(result);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteSchoolAsync(
        [FromRoute] Guid id)
    {
        var result = await _schoolService.DeleteSchoolAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion

    #region Academic Years

    [HttpGet("{schoolId:guid}/academic-years")]
    [ProducesResponseType(typeof(ApiResponse<List<AcademicYearDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AcademicYearDto>>>> GetAcademicYearsAsync(
        [FromRoute] Guid schoolId)
    {
        var result = await _schoolService.GetAcademicYearsAsync(schoolId);
        return Ok(result);
    }

    [HttpPost("{schoolId:guid}/academic-years")]
    [ProducesResponseType(typeof(ApiResponse<AcademicYearDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AcademicYearDto>>> CreateAcademicYearAsync(
        [FromRoute] Guid schoolId,
        [FromBody] AcademicYearDto dto)
    {
        dto.SchoolId = schoolId;
        var result = await _schoolService.CreateAcademicYearAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("academic-years/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AcademicYearDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AcademicYearDto>>> UpdateAcademicYearAsync(
        [FromRoute] Guid id,
        [FromBody] AcademicYearDto dto)
    {
        var result = await _schoolService.UpdateAcademicYearAsync(id, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("{schoolId:guid}/academic-years/{academicYearId:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> SetActiveAcademicYearAsync(
        [FromRoute] Guid schoolId,
        [FromRoute] Guid academicYearId)
    {
        var result = await _schoolService.SetActiveAcademicYearAsync(schoolId, academicYearId);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    #endregion

    #region Classes

    [HttpGet("{schoolId:guid}/classes")]
    [ProducesResponseType(typeof(ApiResponse<List<ClassRoomDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ClassRoomDto>>>> GetClassRoomsAsync(
        [FromRoute] Guid schoolId,
        [FromQuery] Guid? academicYearId = null)
    {
        var result = await _schoolService.GetClassRoomsAsync(schoolId, academicYearId);
        return Ok(result);
    }

    [HttpPost("{schoolId:guid}/classes")]
    [ProducesResponseType(typeof(ApiResponse<ClassRoomDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ClassRoomDto>>> CreateClassRoomAsync(
        [FromRoute] Guid schoolId,
        [FromBody] ClassRoomDto dto)
    {
        dto.SchoolId = schoolId;
        var result = await _schoolService.CreateClassRoomAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("classes/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ClassRoomDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ClassRoomDto>>> UpdateClassRoomAsync(
        [FromRoute] Guid id,
        [FromBody] ClassRoomDto dto)
    {
        var result = await _schoolService.UpdateClassRoomAsync(id, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("classes/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteClassRoomAsync(
        [FromRoute] Guid id)
    {
        var result = await _schoolService.DeleteClassRoomAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion

    #region Sections

    [HttpGet("classes/{classRoomId:guid}/sections")]
    [ProducesResponseType(typeof(ApiResponse<List<SectionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<SectionDto>>>> GetSectionsAsync(
        [FromRoute] Guid classRoomId)
    {
        var result = await _schoolService.GetSectionsAsync(classRoomId);
        return Ok(result);
    }

    [HttpPost("classes/{classRoomId:guid}/sections")]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SectionDto>>> CreateSectionAsync(
        [FromRoute] Guid classRoomId,
        [FromBody] SectionDto dto)
    {
        dto.ClassRoomId = classRoomId;
        var result = await _schoolService.CreateSectionAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("sections/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SectionDto>>> UpdateSectionAsync(
        [FromRoute] Guid id,
        [FromBody] SectionDto dto)
    {
        var result = await _schoolService.UpdateSectionAsync(id, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("sections/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteSectionAsync(
        [FromRoute] Guid id)
    {
        var result = await _schoolService.DeleteSectionAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion

    #region Subjects

    [HttpGet("{schoolId:guid}/subjects")]
    [ProducesResponseType(typeof(ApiResponse<List<SubjectDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<SubjectDto>>>> GetSubjectsAsync(
        [FromRoute] Guid schoolId,
        [FromQuery] Guid? classRoomId = null)
    {
        var result = await _schoolService.GetSubjectsAsync(schoolId, classRoomId);
        return Ok(result);
    }

    [HttpPost("{schoolId:guid}/subjects")]
    [ProducesResponseType(typeof(ApiResponse<SubjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SubjectDto>>> CreateSubjectAsync(
        [FromRoute] Guid schoolId,
        [FromBody] SubjectDto dto)
    {
        dto.SchoolId = schoolId;
        var result = await _schoolService.CreateSubjectAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("subjects/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SubjectDto>>> UpdateSubjectAsync(
        [FromRoute] Guid id,
        [FromBody] SubjectDto dto)
    {
        var result = await _schoolService.UpdateSubjectAsync(id, dto);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("subjects/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteSubjectAsync(
        [FromRoute] Guid id)
    {
        var result = await _schoolService.DeleteSubjectAsync(id);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    #endregion

    #region Departments

    [HttpGet("{schoolId:guid}/departments")]
    [ProducesResponseType(typeof(ApiResponse<List<DepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<DepartmentDto>>>> GetDepartmentsAsync(
        [FromRoute] Guid schoolId)
    {
        var result = await _schoolService.GetDepartmentsAsync(schoolId);
        return Ok(result);
    }

    #endregion

    [HttpGet("timetable/my")]
    [ProducesResponseType(typeof(ApiResponse<List<TimetableDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TimetableDto>>>> GetMyTeacherTimetableAsync()
    {
        var result = await _schoolService.GetMyTeacherTimetableAsync();
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("timetable/my-section")]
    [ProducesResponseType(typeof(ApiResponse<List<TimetableDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TimetableDto>>>> GetMySectionTimetableAsync()
    {
        var result = await _schoolService.GetMySectionTimetableAsync();
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("sections/{sectionId:guid}/timetable")]
    [ProducesResponseType(typeof(ApiResponse<List<TimetableDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<TimetableDto>>>> GetTimetableAsync(
        [FromRoute] Guid sectionId,
        [FromQuery] DateOnly? date = null)
    {
        var result = await _schoolService.GetTimetableAsync(sectionId, date);
        return Ok(result);
    }

    [HttpPost("sections/{sectionId:guid}/timetable")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> SaveTimetableAsync(
        [FromRoute] Guid sectionId,
        [FromBody] List<TimetableDto> dtos)
    {
        foreach (var dto in dtos)
        {
            dto.SectionId = sectionId;
        }

        var result = await _schoolService.SaveTimetableAsync(dtos);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
