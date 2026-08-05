using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Employee;

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public Guid SchoolId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
    public DateTime JoiningDate { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public string? EmploymentType { get; set; }
    public decimal? BasicSalary { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? PFAccountNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? ContractEndDate { get; set; }

    public Identity.ApplicationUser User { get; set; } = null!;
    public School.School School { get; set; } = null!;
    public School.Department? Department { get; set; }
    public Designation? Designation { get; set; }
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<Attendance.Attendance> Attendances { get; set; } = new List<Attendance.Attendance>();
    public ICollection<EmployeeLeave> Leaves { get; set; } = new List<EmployeeLeave>();
    public ICollection<EmployeeSalary> Salaries { get; set; } = new List<EmployeeSalary>();
}

public class Designation : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public Guid SchoolId { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

public class EmployeeDocument : BaseEntity
{
    public string DocumentName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public Guid EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;
}

public class EmployeeLeave : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DocumentUrl { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Remarks { get; set; }

    public Employee Employee { get; set; } = null!;
}

public class EmployeeSalary : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetSalary { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionReference { get; set; }
    public string? Remarks { get; set; }

    public Employee Employee { get; set; } = null!;
}
