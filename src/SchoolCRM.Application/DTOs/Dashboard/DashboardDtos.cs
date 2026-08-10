namespace SchoolCRM.Application.DTOs.Dashboard;

public sealed class DashboardDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalStaff { get; set; }
    public int TotalClasses { get; set; }
    public int TotalParents { get; set; }
    public AttendanceOverviewDto TodayAttendance { get; set; } = new();
    public FeeOverviewDto FeesCollected { get; set; } = new();
    public decimal PendingFees { get; set; }
    public int UpcomingExams { get; set; }
    public List<BirthdayDto> TodayBirthdays { get; set; } = new();
    public List<AnnouncementDto> LatestAnnouncements { get; set; } = new();
    public List<StudentDto> RecentAdmissions { get; set; } = new();
    public List<ExamDto> UpcomingExamsList { get; set; } = new();
}

public sealed class AttendanceOverviewDto
{
    public int Total { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public decimal AttendancePercentage { get; set; }
}

public sealed class FeeOverviewDto
{
    public decimal TotalCollected { get; set; }
    public decimal TodayCollected { get; set; }
    public decimal MonthlyCollected { get; set; }
    public decimal TotalPending { get; set; }
    public decimal OverdueFees { get; set; }
}

public sealed class BirthdayDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public sealed class AnnouncementDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public sealed class StudentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public sealed class ExamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ClassName { get; set; } = string.Empty;
}

public sealed class ChartDataDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string? Category { get; set; }
    public DateTime? Date { get; set; }
}
