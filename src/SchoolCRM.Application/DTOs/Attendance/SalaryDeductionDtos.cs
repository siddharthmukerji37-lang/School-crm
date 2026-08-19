using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Application.DTOs.Attendance;

public sealed class SalaryDeductionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public Guid AttendanceId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public int LateMinutes { get; set; }
    public int LateCountMonth { get; set; }
    public int AllowedLateCount { get; set; }
    public DeductionType DeductionType { get; set; }
    public decimal DeductionAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public SalaryDeductionStatus Status { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ApproveDeductionDto
{
    public string? Note { get; set; }
}
