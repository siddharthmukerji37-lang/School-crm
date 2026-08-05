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
    public Guid? AcademicYearId { get; set; }

    public ExamType? ExamType { get; set; }
    public School.School School { get; set; } = null!;
    public School.ClassRoom? ClassRoom { get; set; }
    public School.AcademicYear? AcademicYear { get; set; }
    public ICollection<ExamSchedule> Schedules { get; set; } = new List<ExamSchedule>();
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
