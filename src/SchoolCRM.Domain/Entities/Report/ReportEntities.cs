using SchoolCRM.Domain.Common;

namespace SchoolCRM.Domain.Entities.Report;

public class StudentReport : BaseEntity
{
    public Guid StudentId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal? TotalMarks { get; set; }
    public decimal? MarksObtained { get; set; }
    public decimal? Percentage { get; set; }
    public int? Rank { get; set; }
    public int? TotalStudents { get; set; }
    public decimal? AttendancePercentage { get; set; }
    public string? GeneratedBy { get; set; }
    public string? GeneratedAt { get; set; }
    public string? ReportData { get; set; }

    public Student.Student Student { get; set; } = null!;
}
