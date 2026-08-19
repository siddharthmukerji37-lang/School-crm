using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Application.DTOs.Attendance;

public sealed class AttendancePolicyDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public int AllowedLateArrivals { get; set; }
    public bool SalaryDeductionEnabled { get; set; }
    public DeductionType DeductionType { get; set; }
    public decimal DeductionAmount { get; set; }
    public bool RequireAdminApproval { get; set; }
    public TimeSpan SchoolStartTime { get; set; }
    public TimeSpan SchoolEndTime { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UpdateAttendancePolicyDto
{
    public int AllowedLateArrivals { get; set; } = 6;
    public bool SalaryDeductionEnabled { get; set; } = true;
    public DeductionType DeductionType { get; set; } = DeductionType.FixedAmount;
    public decimal DeductionAmount { get; set; }
    public bool RequireAdminApproval { get; set; } = true;
    public TimeSpan SchoolStartTime { get; set; } = new TimeSpan(9, 30, 0);
    public TimeSpan SchoolEndTime { get; set; } = new TimeSpan(18, 30, 0);
}

public sealed class AttendanceMonthlySummaryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalLateCount { get; set; }
    public int AllowedLateCount { get; set; }
    public bool PolicyExceeded { get; set; }
    public int SalaryDeductionCount { get; set; }
    public decimal SalaryDeductionAmount { get; set; }
}
