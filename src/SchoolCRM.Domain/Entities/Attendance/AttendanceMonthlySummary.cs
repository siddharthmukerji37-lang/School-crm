using SchoolCRM.Domain.Common;

namespace SchoolCRM.Domain.Entities.Attendance;

public class AttendanceMonthlySummary : BaseEntity
{
    public Guid UserId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalLateCount { get; set; }
    public int AllowedLateCount { get; set; } = 6;
    public bool PolicyExceeded { get; set; }
    public bool WarningSent { get; set; }
    public int SalaryDeductionCount { get; set; }
    public decimal SalaryDeductionAmount { get; set; }

    public Identity.ApplicationUser User { get; set; } = null!;
}
