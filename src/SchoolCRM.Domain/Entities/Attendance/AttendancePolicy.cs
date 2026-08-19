using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Attendance;

public class AttendancePolicy : BaseEntity
{
    public Guid SchoolId { get; set; }
    public int AllowedLateArrivals { get; set; } = 6;
    public bool SalaryDeductionEnabled { get; set; } = true;
    public DeductionType DeductionType { get; set; } = DeductionType.FixedAmount;
    public decimal DeductionAmount { get; set; }
    public bool RequireAdminApproval { get; set; } = true;
    public TimeSpan SchoolStartTime { get; set; } = new TimeSpan(9, 30, 0);
    public TimeSpan SchoolEndTime { get; set; } = new TimeSpan(18, 30, 0);
    public bool IsActive { get; set; } = true;

    public School.School School { get; set; } = null!;
}
