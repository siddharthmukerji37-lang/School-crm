using System.ComponentModel.DataAnnotations;

namespace SchoolCRM.Application.DTOs.Exam;

public sealed class ExamQuestionDto
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = nameof(SchoolCRM.Domain.Enums.QuestionType.MCQ);
    public string? OptionA { get; set; }
    public string? OptionB { get; set; }
    public string? OptionC { get; set; }
    public string? OptionD { get; set; }
    public string? CorrectAnswer { get; set; }
    public decimal Marks { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageFileName { get; set; }
    public int OrderIndex { get; set; }
    public Guid? SubjectId { get; set; }
    public string? SubjectName { get; set; }
}

public sealed class CreateExamQuestionDto
{
    [Required(ErrorMessage = "Question text is required")]
    public string QuestionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Question type is required")]
    public string QuestionType { get; set; } = nameof(SchoolCRM.Domain.Enums.QuestionType.MCQ);

    public string? OptionA { get; set; }
    public string? OptionB { get; set; }
    public string? OptionC { get; set; }
    public string? OptionD { get; set; }

    public string? CorrectAnswer { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Marks must be non-negative")]
    public decimal Marks { get; set; }

    public string? ImageUrl { get; set; }
    public string? ImageFileName { get; set; }
    public int OrderIndex { get; set; }
    public Guid? SubjectId { get; set; }
}

public sealed class ExamSubmissionDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public decimal? TotalMarksObtained { get; set; }
    public decimal TotalMaxMarks { get; set; }
    public bool IsGraded { get; set; }
    public string? GradedBy { get; set; }
    public DateTime? GradedAt { get; set; }
    public string GradingStatus { get; set; } = nameof(SchoolCRM.Domain.Enums.GradingStatus.Pending);
    public string? GradingApprovedBy { get; set; }
    public DateTime? GradingApprovedAt { get; set; }
    public string? GradingRejectionReason { get; set; }
    public List<ExamAnswerDto> Answers { get; set; } = new();
}

public sealed class ExamAnswerDto
{
    public Guid Id { get; set; }
    public Guid ExamQuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public string? SelectedOption { get; set; }
    public string? AnswerText { get; set; }
    public string? ImageUrl { get; set; }
    public string? CorrectAnswer { get; set; }
    public string? OptionA { get; set; }
    public string? OptionB { get; set; }
    public string? OptionC { get; set; }
    public string? OptionD { get; set; }
    public decimal Marks { get; set; }
    public bool? IsCorrect { get; set; }
    public decimal? MarksObtained { get; set; }
    public string? Remarks { get; set; }
    public int OrderIndex { get; set; }
}

public sealed class SubmitExamDto
{
    [Required(ErrorMessage = "Exam ID is required")]
    public Guid ExamId { get; set; }

    public List<SubmitAnswerDto> Answers { get; set; } = new();
}

public sealed class SubmitAnswerDto
{
    [Required(ErrorMessage = "Question ID is required")]
    public Guid ExamQuestionId { get; set; }

    public string? SelectedOption { get; set; }
    public string? AnswerText { get; set; }
    public string? ImageUrl { get; set; }
}

public sealed class GradeSubmissionDto
{
    [Required(ErrorMessage = "Answers are required")]
    public List<GradeAnswerDto> Answers { get; set; } = new();
}

public sealed class GradeAnswerDto
{
    [Required(ErrorMessage = "Answer ID is required")]
    public Guid AnswerId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Marks must be non-negative")]
    public decimal MarksObtained { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

public sealed class ApproveExamDto
{
    [Required(ErrorMessage = "Approved is required")]
    public bool Approved { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }
}

public sealed class GradeApprovalDto
{
    [Required(ErrorMessage = "Approved is required")]
    public bool Approved { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }
}

public sealed class UploadQuestionPaperDto
{
    [Required(ErrorMessage = "File URL is required")]
    public string FileUrl { get; set; } = string.Empty;

    public string? FileName { get; set; }
}
