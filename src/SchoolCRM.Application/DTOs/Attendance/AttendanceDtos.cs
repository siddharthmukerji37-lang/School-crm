using System.ComponentModel.DataAnnotations;

namespace SchoolCRM.Application.DTOs.Attendance;

public sealed class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
    public Guid ClassRoomId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? MarkedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public sealed class MarkAttendanceDto
{
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Section is required")]
    public Guid SectionId { get; set; }

    [Required(ErrorMessage = "Class is required")]
    public Guid ClassRoomId { get; set; }

    [Required(ErrorMessage = "Attendance records are required")]
    [MinLength(1, ErrorMessage = "At least one attendance record is required")]
    public List<AttendanceRecordDto> Records { get; set; } = new();
}

public sealed class AttendanceRecordDto
{
    [Required(ErrorMessage = "Student ID is required")]
    public Guid StudentId { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(Present|Absent|Late|Excused)$",
        ErrorMessage = "Status must be Present, Absent, Late, or Excused")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

public sealed class AttendanceStatsDto
{
    public DateTime Date { get; set; }
    public Guid? ClassRoomId { get; set; }
    public string? ClassName { get; set; }
    public Guid? SectionId { get; set; }
    public string? SectionName { get; set; }
    public int TotalStudents { get; set; }
    public int Present { get; set; }
    public int Absent { get; set; }
    public int Late { get; set; }
    public int Excused { get; set; }
    public decimal AttendancePercentage { get; set; }
}

public sealed class BulkMarkAttendanceDto
{
    [Required(ErrorMessage = "Start date is required")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Student IDs are required")]
    [MinLength(1, ErrorMessage = "At least one student is required")]
    public List<Guid> StudentIds { get; set; } = new();

    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(Present|Absent|Late|Excused)$",
        ErrorMessage = "Status must be Present, Absent, Late, or Excused")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public bool SkipWeekends { get; set; } = true;
}

public sealed class MarkStaffAttendanceDto
{
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Attendance records are required")]
    [MinLength(1, ErrorMessage = "At least one attendance record is required")]
    public List<StaffAttendanceRecordDto> Records { get; set; } = new();
}

public sealed class StaffAttendanceRecordDto
{
    public Guid? TeacherId { get; set; }
    public Guid? EmployeeId { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [RegularExpression("^(Present|Absent|Late|Excused)$",
        ErrorMessage = "Status must be Present, Absent, Late, or Excused")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

public sealed class StaffAttendanceDto
{
    public Guid Id { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
}

public sealed class MyAttendanceDto
{
    public Guid? Id { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string? Remarks { get; set; }
    public bool IsCheckedIn { get; set; }
    public bool IsCheckedOut { get; set; }
    public int LateMinutes { get; set; }
    public string? LateReason { get; set; }
    public int LateCount { get; set; }
    public int AllowedLateCount { get; set; }
    public bool PolicyExceeded { get; set; }
    public bool SalaryDeductionRequired { get; set; }
    public string? Warning { get; set; }
    public int EarlyMinutes { get; set; }
    public string? EarlyReason { get; set; }
    public bool EarlyDeparture { get; set; }
    public string? EarlyWarning { get; set; }
}

public sealed class StaffAttendanceStatsDto
{
    public DateTime Date { get; set; }
    public int TotalTeachers { get; set; }
    public int TeachersPresent { get; set; }
    public int TeachersAbsent { get; set; }
    public int TotalEmployees { get; set; }
    public int EmployeesPresent { get; set; }
    public int EmployeesAbsent { get; set; }
}

public sealed class ClockInDto
{
    [MaxLength(500)]
    public string? LateReason { get; set; }
}

public sealed class ClockOutDto
{
    [MaxLength(500)]
    public string? EarlyReason { get; set; }
}

public sealed class LateStaffDto
{
    public Guid Id { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public int LateMinutes { get; set; }
    public int LateCountMonth { get; set; }
    public int AllowedLateCount { get; set; }
    public string? LateReason { get; set; }
    public bool LatePolicyExceeded { get; set; }
    public bool SalaryDeductionRequired { get; set; }
    public Domain.Enums.SalaryDeductionStatus? SalaryDeductionStatus { get; set; }
    public decimal? SalaryDeductionAmount { get; set; }
}
