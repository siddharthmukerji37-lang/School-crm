using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Attendance;

public class Attendance : BaseEntity
{
    public DateTime Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string? QrCode { get; set; }
    public string? BiometricId { get; set; }
    public string? Remarks { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid SchoolId { get; set; }

    public Student.Student? Student { get; set; }
    public Teacher.Teacher? Teacher { get; set; }
    public Employee.Employee? Employee { get; set; }
    public School.School School { get; set; } = null!;
}

public class AttendanceSummary : BaseEntity
{
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int HalfDays { get; set; }
    public int ExcusedDays { get; set; }
    public decimal AttendancePercentage { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? EmployeeId { get; set; }

    public Student.Student? Student { get; set; }
    public Teacher.Teacher? Teacher { get; set; }
    public Employee.Employee? Employee { get; set; }
}
