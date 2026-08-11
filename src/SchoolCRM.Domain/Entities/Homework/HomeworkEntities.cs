using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Homework;

public class Homework : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public decimal? MaxMarks { get; set; }
    public Guid ClassRoomId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SchoolId { get; set; }
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public School.ClassRoom ClassRoom { get; set; } = null!;
    public School.Section? Section { get; set; }
    public School.Subject Subject { get; set; } = null!;
    public Teacher.Teacher Teacher { get; set; } = null!;
    public ICollection<HomeworkSubmission> Submissions { get; set; } = new List<HomeworkSubmission>();
}

public class HomeworkSubmission : BaseEntity
{
    public string? SubmittedText { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public DateTime SubmittedAt { get; set; }
    public HomeworkStatus Status { get; set; } = HomeworkStatus.Submitted;
    public decimal? MarksObtained { get; set; }
    public string? TeacherRemarks { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid HomeworkId { get; set; }
    public Guid StudentId { get; set; }

    public Homework Homework { get; set; } = null!;
    public Student.Student Student { get; set; } = null!;
}

public class Assignment : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal? MaxMarks { get; set; }
    public Guid ClassRoomId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SchoolId { get; set; }

    public School.ClassRoom ClassRoom { get; set; } = null!;
    public School.Subject Subject { get; set; } = null!;
    public Teacher.Teacher Teacher { get; set; } = null!;
    public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
}

public class AssignmentSubmission : BaseEntity
{
    public string? SubmittedText { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime SubmittedAt { get; set; }
    public HomeworkStatus Status { get; set; } = HomeworkStatus.Submitted;
    public decimal? MarksObtained { get; set; }
    public string? Remarks { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }

    public Assignment Assignment { get; set; } = null!;
    public Student.Student Student { get; set; } = null!;
}
