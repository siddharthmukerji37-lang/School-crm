using System.ComponentModel.DataAnnotations;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Application.DTOs.Exam;

public sealed class ExamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ExamType { get; set; } = string.Empty;
    public Guid ClassRoomId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid? AcademicYearId { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? MaxMarks { get; set; }
    public decimal? PassingMarks { get; set; }
    public bool IsPublished { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ResultDate { get; set; }
    public string? TeacherName { get; set; }
    public string? QuestionPaperUrl { get; set; }
    public string? QuestionPaperFileName { get; set; }
    public string ApprovalStatus { get; set; } = SchoolCRM.Domain.Enums.ApprovalStatus.Pending.ToString();
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public int QuestionCount { get; set; }
    public decimal TotalMarks { get; set; }
}

public sealed class CreateExamDto
{
    [Required(ErrorMessage = "Exam name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Exam type is required")]
    public string ExamType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Class is required")]
    public Guid ClassRoomId { get; set; }

    public Guid? AcademicYearId { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    public decimal? MaxMarks { get; set; }

    public decimal? PassingMarks { get; set; }

    public DateTime? ResultDate { get; set; }
}

public sealed class ExamScheduleDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Room { get; set; }
    public decimal? MaxMarks { get; set; }
    public decimal? PassingMarks { get; set; }
    public string? Instructions { get; set; }
    public Guid? TeacherId { get; set; }
    public string? TeacherName { get; set; }
}

public sealed class MarkDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public decimal? MarksObtained { get; set; }
    public decimal MaxMarks { get; set; }
    public bool? IsPass { get; set; }
    public string? Remarks { get; set; }
    public string? GradedBy { get; set; }
    public DateTime? GradedDate { get; set; }
}

public sealed class EnterMarksDto
{
    [Required(ErrorMessage = "Exam ID is required")]
    public Guid ExamId { get; set; }

    [Required(ErrorMessage = "Subject ID is required")]
    public Guid SubjectId { get; set; }

    [Required(ErrorMessage = "Marks are required")]
    [MinLength(1, ErrorMessage = "At least one mark entry is required")]
    public List<MarkEntryDto> Marks { get; set; } = new();
}

public sealed class MarkEntryDto
{
    [Required(ErrorMessage = "Student ID is required")]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Marks obtained is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Marks must be non-negative")]
    public decimal MarksObtained { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

public sealed class ResultDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public List<SubjectResultDto> SubjectResults { get; set; } = new();
    public decimal TotalMarksObtained { get; set; }
    public decimal TotalMaxMarks { get; set; }
    public decimal Percentage { get; set; }
    public string Grade { get; set; } = string.Empty;
    public bool IsPassed { get; set; }
    public int Rank { get; set; }
}

public sealed class SubjectResultDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public decimal MarksObtained { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal PassingMarks { get; set; }
    public bool IsPass { get; set; }
    public string? Remarks { get; set; }
}

public sealed class ReportCardDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public ExamDto Exam { get; set; } = new();
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public List<SubjectResultDto> SubjectResults { get; set; } = new();
    public decimal TotalMarksObtained { get; set; }
    public decimal TotalMaxMarks { get; set; }
    public decimal Percentage { get; set; }
    public string Grade { get; set; } = string.Empty;
    public bool IsPassed { get; set; }
    public int Rank { get; set; }
    public decimal AttendancePercentage { get; set; }
    public string? TeacherRemarks { get; set; }
    public string? PrincipalRemarks { get; set; }
}
