using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Shared.Models;
using static SchoolCRM.Application.Interfaces.Services.IReportService;

namespace SchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("templates")]
    [ProducesResponseType(typeof(ApiResponse<List<ReportTemplateDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<ReportTemplateDto>>>> GetReportTemplatesAsync()
    {
        var result = await _reportService.GetReportTemplatesAsync();
        return Ok(result);
    }

    [HttpGet("{type}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateReportAsync(
        [FromRoute] string type,
        [FromQuery] Guid? schoolId = null,
        [FromQuery] Guid? classRoomId = null,
        [FromQuery] Guid? sectionId = null,
        [FromQuery] Guid? examId = null,
        [FromQuery] Guid? studentId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string format = "pdf")
    {
        var resolvedSchoolId = schoolId ?? Guid.Empty;

        ApiResponse<byte[]> result = type.ToLowerInvariant() switch
        {
            "students" => await _reportService.GenerateStudentReportAsync(
                resolvedSchoolId, classRoomId, sectionId, format),

            "attendance" when fromDate.HasValue && toDate.HasValue =>
                await _reportService.GenerateAttendanceReportAsync(
                    resolvedSchoolId, fromDate.Value, toDate.Value, classRoomId, format),

            "fee" when fromDate.HasValue && toDate.HasValue =>
                await _reportService.GenerateFeeReportAsync(
                    resolvedSchoolId, fromDate.Value, toDate.Value, classRoomId, format),

            "exam" when examId.HasValue =>
                await _reportService.GenerateExamReportAsync(
                    resolvedSchoolId, examId.Value, classRoomId, format),

            "report-card" when studentId.HasValue && examId.HasValue =>
                await _reportService.GenerateStudentReportCardAsync(
                    studentId.Value, examId.Value, format),

            "employee" => await _reportService.GenerateEmployeeReportAsync(
                resolvedSchoolId, format),

            "inventory" => await _reportService.GenerateInventoryReportAsync(
                resolvedSchoolId, format),

            "account" when fromDate.HasValue && toDate.HasValue =>
                await _reportService.GenerateAccountReportAsync(
                    resolvedSchoolId, fromDate.Value, toDate.Value, format),

            _ => ApiResponse<byte[]>.FailResponse(
                $"Invalid report type '{type}' or missing required parameters", 400)
        };

        if (!result.Success)
            return BadRequest(result);

        var contentType = format.ToLowerInvariant() switch
        {
            "pdf" => "application/pdf",
            "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "csv" => "text/csv",
            _ => "application/octet-stream"
        };

        var fileName = $"{type}-report.{format}";
        return File(result.Data!, contentType, fileName);
    }

    [HttpPost("custom")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateCustomReportAsync(
        [FromBody] CustomReportRequestDto dto)
    {
        var result = await _reportService.GenerateCustomReportAsync(dto);
        if (!result.Success)
            return BadRequest(result);

        var contentType = dto.Format.ToLowerInvariant() switch
        {
            "pdf" => "application/pdf",
            "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "csv" => "text/csv",
            _ => "application/octet-stream"
        };

        var fileName = $"{dto.ReportType}-custom-report.{dto.Format}";
        return File(result.Data!, contentType, fileName);
    }
}
