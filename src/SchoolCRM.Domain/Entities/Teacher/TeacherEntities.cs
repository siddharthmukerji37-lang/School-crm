using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Teacher;

public class Teacher : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid? SchoolId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime JoiningDate { get; set; }
    public TeacherStatus Status { get; set; } = TeacherStatus.Active;
    public string? Qualification { get; set; }
    public string? Specialization { get; set; }
    public int ExperienceYears { get; set; }
    public string? Designation { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? BasicSalary { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? PFAccountNumber { get; set; }
    public string? PANNumber { get; set; }

    public Identity.ApplicationUser User { get; set; } = null!;
    public School.School? School { get; set; }
    public School.Department? Department { get; set; }
    public ICollection<TeacherDocument> Documents { get; set; } = new List<TeacherDocument>();
    public ICollection<Attendance.Attendance> Attendances { get; set; } = new List<Attendance.Attendance>();
    public ICollection<TeacherLeave> Leaves { get; set; } = new List<TeacherLeave>();
    public ICollection<TeacherSalary> Salaries { get; set; } = new List<TeacherSalary>();
    public ICollection<School.Timetable> Timetables { get; set; } = new List<School.Timetable>();
    public ICollection<Exam.Mark> ExamMarks { get; set; } = new List<Exam.Mark>();
    public ICollection<Homework.Homework> Homeworks { get; set; } = new List<Homework.Homework>();
    public ICollection<Homework.Assignment> Assignments { get; set; } = new List<Homework.Assignment>();
}

public class TeacherDocument : BaseEntity
{
    public string DocumentName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public Guid TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;
}

public class TeacherLeave : BaseEntity
{
    public Guid TeacherId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DocumentUrl { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Remarks { get; set; }

    public Teacher Teacher { get; set; } = null!;
}

public class TeacherSalary : BaseEntity
{
    public Guid TeacherId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetSalary { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionReference { get; set; }
    public string? Remarks { get; set; }

    public Teacher Teacher { get; set; } = null!;
}

public class TeacherPerformance : BaseEntity
{
    public Guid TeacherId { get; set; }
    public string ReviewPeriod { get; set; } = string.Empty;
    public decimal TeachingQualityScore { get; set; }
    public decimal StudentFeedbackScore { get; set; }
    public decimal PunctualityScore { get; set; }
    public decimal OverallScore { get; set; }
    public string? ReviewedBy { get; set; }
    public string? Comments { get; set; }
    public string? Recommendations { get; set; }
    public DateTime ReviewDate { get; set; }

    public Teacher Teacher { get; set; } = null!;
}
