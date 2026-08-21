using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Application.DTOs.Leave;

public sealed class LeaveCalendarDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateLeaveCalendarDto
{
    public string Name { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class LeaveTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresApproval { get; set; }
    public bool RequiresAttachment { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateLeaveTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool RequiresApproval { get; set; } = true;
    public bool RequiresAttachment { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public sealed class LeaveTypeConfigDto
{
    public Guid Id { get; set; }
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public int TotalDays { get; set; }
    public bool IsPaid { get; set; }
    public Gender ApplicableGender { get; set; }
    public string ApplicableUserType { get; set; } = "Both";
    public int MinimumDays { get; set; }
    public int MaximumDays { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateLeaveTypeConfigDto
{
    public Guid LeaveTypeId { get; set; }
    public int TotalDays { get; set; }
    public bool IsPaid { get; set; } = true;
    public Gender ApplicableGender { get; set; } = Gender.Male;
    public string ApplicableUserType { get; set; } = "Both";
    public int MinimumDays { get; set; } = 1;
    public int MaximumDays { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateLeaveTypeConfigDto
{
    public int TotalDays { get; set; }
    public bool IsPaid { get; set; }
    public Gender ApplicableGender { get; set; }
    public string ApplicableUserType { get; set; } = "Both";
    public int MinimumDays { get; set; }
    public int MaximumDays { get; set; }
    public bool IsActive { get; set; }
}

public sealed class LeaveBalanceDto
{
    public Guid Id { get; set; }
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public int AllocatedDays { get; set; }
    public int UsedDays { get; set; }
    public int PendingDays { get; set; }
    public int RemainingDays { get; set; }
}

public sealed class ApplyLeaveDto
{
    public Guid LeaveTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
}

public sealed class LeaveRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentPath { get; set; }
    public LeaveStatus Status { get; set; }
    public string StatusName => Status switch
    {
        LeaveStatus.Pending => "Pending",
        LeaveStatus.Approved => "Approved",
        LeaveStatus.Rejected => "Rejected",
        LeaveStatus.Cancelled => "Cancelled",
        _ => "Unknown"
    };
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? AdminReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ApproveLeaveDto
{
    public string? AdminReason { get; set; }
}

public sealed class RejectLeaveDto
{
    public string Reason { get; set; } = string.Empty;
}

public sealed class LeaveCalendarViewDto
{
    public int Year { get; set; }
    public List<LeaveCalendarDayDto> Days { get; set; } = new();
}

public sealed class LeaveCalendarDayDto
{
    public DateTime Date { get; set; }
    public List<LeaveCalendarDayUserDto> UsersOnLeave { get; set; } = new();
}

public sealed class LeaveCalendarDayUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public sealed class LeaveReportDto
{
    public int Year { get; set; }
    public List<LeaveReportUserDto> Users { get; set; } = new();
}

public sealed class LeaveReportUserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public List<LeaveReportTypeDto> LeaveTypes { get; set; } = new();
}

public sealed class LeaveReportTypeDto
{
    public string LeaveTypeName { get; set; } = string.Empty;
    public int Allocated { get; set; }
    public int Used { get; set; }
    public int Pending { get; set; }
    public int Remaining { get; set; }
}
