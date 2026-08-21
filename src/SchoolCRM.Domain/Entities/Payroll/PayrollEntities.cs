using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Payroll;

public class PayrollSetting : BaseEntity
{
    public int AllowedLateCount { get; set; } = 6;
    public bool LateDeductionEnabled { get; set; }
    public DeductionType LateDeductionType { get; set; }
    public decimal LateDeductionAmount { get; set; }
    public int PayrollDivisor { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public bool RequireAccountApproval { get; set; }
    public Guid SchoolId { get; set; }

    public School.School School { get; set; } = null!;
}

public class SalaryProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public int? PayrollDivisor { get; set; }
    public bool IsActive { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIFSC { get; set; }
    public Guid SchoolId { get; set; }

    public School.School School { get; set; } = null!;
    public ICollection<SalaryComponent> Components { get; set; } = new List<SalaryComponent>();
}

public class SalaryComponent : BaseEntity
{
    public Guid SalaryProfileId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }

    public SalaryProfile SalaryProfile { get; set; } = null!;
}

public class Payroll : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal GrossSalary { get; set; }
    public int LateCount { get; set; }
    public int AllowedLateCount { get; set; }
    public decimal LateDeduction { get; set; }
    public int PaidLeaveDays { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public DateTime? CalculatedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? PaidBy { get; set; }
    public DateTime? PaidAt { get; set; }
    public decimal DailySalary { get; set; }
    public int PayrollDivisor { get; set; }
    public Guid SchoolId { get; set; }

    public School.School School { get; set; } = null!;
    public ICollection<PayrollDeduction> Deductions { get; set; } = new List<PayrollDeduction>();
}

public class PayrollDeduction : BaseEntity
{
    public Guid PayrollId { get; set; }
    public string DeductionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Days { get; set; }
    public decimal Amount { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceType { get; set; }

    public Payroll Payroll { get; set; } = null!;
}

public class Payslip : BaseEntity
{
    public Guid PayrollId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PayslipNumber { get; set; } = string.Empty;
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string? PdfPath { get; set; }
    public string Status { get; set; } = "Generated";
    public string? GeneratedBy { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public Guid SchoolId { get; set; }

    public Payroll Payroll { get; set; } = null!;
    public School.School School { get; set; } = null!;
}
