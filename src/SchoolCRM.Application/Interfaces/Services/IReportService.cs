using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IReportService
{
    Task<ApiResponse<byte[]>> GenerateStudentReportAsync(Guid schoolId, Guid? classRoomId, Guid? sectionId, string format);

    Task<ApiResponse<byte[]>> GenerateAttendanceReportAsync(
        Guid schoolId, DateTime fromDate, DateTime toDate, Guid? classRoomId, string format);

    Task<ApiResponse<byte[]>> GenerateFeeReportAsync(
        Guid schoolId, DateTime fromDate, DateTime toDate, Guid? classRoomId, string format);

    Task<ApiResponse<byte[]>> GenerateExamReportAsync(
        Guid schoolId, Guid examId, Guid? classRoomId, string format);

    Task<ApiResponse<byte[]>> GenerateStudentReportCardAsync(Guid studentId, Guid examId, string format);

    Task<ApiResponse<byte[]>> GenerateEmployeeReportAsync(Guid schoolId, string format);

    Task<ApiResponse<byte[]>> GenerateInventoryReportAsync(Guid schoolId, string format);

    Task<ApiResponse<byte[]>> GenerateAccountReportAsync(
        Guid schoolId, DateTime fromDate, DateTime toDate, string format);

    Task<ApiResponse<List<ReportTemplateDto>>> GetReportTemplatesAsync();

    Task<ApiResponse<byte[]>> GenerateCustomReportAsync(CustomReportRequestDto dto);

    public sealed class ReportTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
    }

    public sealed class CustomReportRequestDto
    {
        public string ReportType { get; set; } = string.Empty;
        public Guid SchoolId { get; set; }
        public Dictionary<string, string>? Filters { get; set; }
        public List<string>? Columns { get; set; }
        public string Format { get; set; } = "pdf";
    }
}
