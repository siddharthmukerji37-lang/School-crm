using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Attendance;

public class SalaryDeduction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid AttendanceId { get; set; }
    public DeductionType DeductionType { get; set; }
    public decimal DeductionAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public SalaryDeductionStatus Status { get; set; } = SalaryDeductionStatus.Pending;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }

    public Identity.ApplicationUser User { get; set; } = null!;
    public Attendance Attendance { get; set; } = null!;
}
