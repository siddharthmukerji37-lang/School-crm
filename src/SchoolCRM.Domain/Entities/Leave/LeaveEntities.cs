using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Leave;

public class LeaveCalendar : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid SchoolId { get; set; }

    public School.School School { get; set; } = null!;
    public ICollection<LeaveTypeConfig> LeaveTypeConfigs { get; set; } = new List<LeaveTypeConfig>();
}

public class LeaveType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public bool RequiresAttachment { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public Guid SchoolId { get; set; }

    public School.School School { get; set; } = null!;
    public ICollection<LeaveTypeConfig> LeaveTypeConfigs { get; set; } = new List<LeaveTypeConfig>();
}

public class LeaveTypeConfig : BaseEntity
{
    public Guid LeaveCalendarId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public int TotalDays { get; set; }
    public bool IsPaid { get; set; } = true;
    public Gender ApplicableGender { get; set; } = Gender.Male;
    public string ApplicableUserType { get; set; } = "Both";
    public int MinimumDays { get; set; } = 1;
    public int MaximumDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;

    public LeaveCalendar LeaveCalendar { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}

public class LeaveBalance : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public Guid LeaveCalendarId { get; set; }
    public int AllocatedDays { get; set; }
    public int UsedDays { get; set; }
    public int PendingDays { get; set; }
    public int RemainingDays => AllocatedDays - UsedDays;

    public LeaveType LeaveType { get; set; } = null!;
    public LeaveCalendar LeaveCalendar { get; set; } = null!;
}

public class LeaveRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public Guid LeaveCalendarId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? AdminReason { get; set; }

    public LeaveType LeaveType { get; set; } = null!;
    public LeaveCalendar LeaveCalendar { get; set; } = null!;
    public ICollection<LeaveRequestDay> LeaveRequestDays { get; set; } = new List<LeaveRequestDay>();
}

public class LeaveRequestDay : BaseEntity
{
    public Guid LeaveRequestId { get; set; }
    public DateTime LeaveDate { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public LeaveRequest LeaveRequest { get; set; } = null!;
}
