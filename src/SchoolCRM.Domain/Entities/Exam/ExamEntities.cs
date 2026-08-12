using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Exam;

public class ExamType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Weightage { get; set; }
    public Guid SchoolId { get; set; }

    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
}

public class Exam : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ExamStatus Status { get; set; } = ExamStatus.Scheduled;
    public Guid? ExamTypeId { get; set; }
    public Guid SchoolId { get; set; }
    public Guid? ClassRoomId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? AcademicYearId { get; set; }
    public Guid? TeacherId { get; set; }
    public string? QuestionPaperUrl { get; set; }
    public string? QuestionPaperFileName { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public ExamType? ExamType { get; set; }
    public School.School School { get; set; } = null!;
    public School.ClassRoom? ClassRoom { get; set; }
    public School.Section? Section { get; set; }
    public School.AcademicYear? AcademicYear { get; set; }
    public Teacher.Teacher? Teacher { get; set; }
    public ICollection<ExamSchedule> Schedules { get; set; } = new List<ExamSchedule>();
    public ICollection<ExamQuestion> Questions { get; set; } = new List<ExamQuestion>();
    public ICollection<ExamSubmission> Submissions { get; set; } = new List<ExamSubmission>();
}

public class ExamQuestion : BaseEntity
{
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string? OptionA { get; set; }
    public string? OptionB { get; set; }
    public string? OptionC { get; set; }
    public string? OptionD { get; set; }
    public string? CorrectAnswer { get; set; }
    public decimal Marks { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageFileName { get; set; }
    public int OrderIndex { get; set; }
    public Guid ExamId { get; set; }
    public Guid? SubjectId { get; set; }

    public Exam Exam { get; set; } = null!;
    public School.Subject? Subject { get; set; }
    public ICollection<ExamAnswer> Answers { get; set; } = new List<ExamAnswer>();
}

public class ExamSubmission : BaseEntity
{
    public DateTime SubmittedAt { get; set; }
    public decimal TotalMarksObtained { get; set; }
    public decimal TotalMaxMarks { get; set; }
    public bool IsGraded { get; set; }
    public string? GradedBy { get; set; }
    public DateTime? GradedAt { get; set; }
    public GradingStatus GradingStatus { get; set; } = GradingStatus.Pending;
    public string? GradingApprovedBy { get; set; }
    public DateTime? GradingApprovedAt { get; set; }
    public string? GradingRejectionReason { get; set; }
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }

    public Exam Exam { get; set; } = null!;
    public Student.Student Student { get; set; } = null!;
    public ICollection<ExamAnswer> Answers { get; set; } = new List<ExamAnswer>();
}

public class ExamAnswer : BaseEntity
{
    public string? SelectedOption { get; set; }
    public string? AnswerText { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsCorrect { get; set; }
    public decimal MarksObtained { get; set; }
    public string? Remarks { get; set; }
    public Guid ExamSubmissionId { get; set; }
    public Guid ExamQuestionId { get; set; }

    public ExamSubmission ExamSubmission { get; set; } = null!;
    public ExamQuestion ExamQuestion { get; set; } = null!;
}

public class ExamSchedule : BaseEntity
{
    public DateTime ExamDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal PassMarks { get; set; }
    public string? HallName { get; set; }
    public string? Instructions { get; set; }
    public Guid ExamId { get; set; }
    public Guid SubjectId { get; set; }

    public Exam Exam { get; set; } = null!;
    public School.Subject Subject { get; set; } = null!;
    public ICollection<Mark> Marks { get; set; } = new List<Mark>();
}

public class Mark : BaseEntity
{
    public decimal MarksObtained { get; set; }
    public string? Grade { get; set; }
    public string? Remarks { get; set; }
    public string? EnteredBy { get; set; }
    public DateTime? EnteredAt { get; set; }
    public bool IsAbsent { get; set; }
    public bool IsPublished { get; set; }
    public Guid ExamScheduleId { get; set; }
    public Guid StudentId { get; set; }
    public Guid? TeacherId { get; set; }

    public ExamSchedule ExamSchedule { get; set; } = null!;
    public Student.Student Student { get; set; } = null!;
    public Teacher.Teacher? Teacher { get; set; }
}

public class GradeSystem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public decimal MinMarks { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal GPA { get; set; }
    public string? Description { get; set; }
    public Guid SchoolId { get; set; }
}

public class ReportCard : BaseEntity
{
    public Guid StudentId { get; set; }
    public Guid ExamId { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal MarksObtained { get; set; }
    public decimal Percentage { get; set; }
    public string? Grade { get; set; }
    public int Rank { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Remarks { get; set; }
    public string? GeneratedBy { get; set; }

    public Student.Student Student { get; set; } = null!;
    public Exam Exam { get; set; } = null!;
}
