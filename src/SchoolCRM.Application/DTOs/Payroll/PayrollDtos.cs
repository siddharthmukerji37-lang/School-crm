using System.ComponentModel.DataAnnotations.Schema;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Application.DTOs.Payroll;

public sealed class PayrollSettingDto
{
    public Guid Id { get; set; }
    public int AllowedLateCount { get; set; }
    public bool LateDeductionEnabled { get; set; }
    public int LateDeductionType { get; set; }
    public decimal LateDeductionAmount { get; set; }
    public int PayrollDivisor { get; set; }
    public bool RequireAccountApproval { get; set; }
}

public sealed class CreatePayrollSettingDto
{
    public int AllowedLateCount { get; set; }
    public bool LateDeductionEnabled { get; set; }
    public int LateDeductionType { get; set; }
    public decimal LateDeductionAmount { get; set; }
    public int PayrollDivisor { get; set; }
    public bool RequireAccountApproval { get; set; }
}

public sealed class SalaryProfileDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }
    [NotMapped]
    public decimal GrossSalary => BasicSalary + Allowances;
    public DateTime EffectiveFrom { get; set; }
    public int? PayrollDivisor { get; set; }
    public bool IsActive { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIFSC { get; set; }
}

public sealed class CreateSalaryProfileDto
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
}

public sealed class SalaryComponentDto
{
    public Guid Id { get; set; }
    public Guid SalaryProfileId { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateSalaryComponentDto
{
    public string ComponentName { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsActive { get; set; }
}

public sealed class PayrollDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal GrossSalary { get; set; }
    public int LateCount { get; set; }
    public int AllowedLateCount { get; set; }
    public decimal LateDeduction { get; set; }
    public decimal DailySalary { get; set; }
    public int PayrollDivisor { get; set; }
    public int PaidLeaveDays { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public decimal UnpaidLeaveDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public int Status { get; set; }
    [NotMapped]
    public string StatusName => Status switch
    {
        1 => "Draft",
        2 => "Calculated",
        3 => "Under Review",
        4 => "Approved",
        5 => "Payslip Generated",
        6 => "Paid",
        7 => "Cancelled",
        _ => "Unknown"
    };
    public DateTime? CalculatedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? PaidBy { get; set; }
    public DateTime? PaidAt { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
}

public sealed class GeneratePayrollDto
{
    public int Month { get; set; }
    public int Year { get; set; }
}

public sealed class PayrollDeductionDto
{
    public Guid Id { get; set; }
    public Guid PayrollId { get; set; }
    public string DeductionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Days { get; set; }
    public decimal Amount { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceType { get; set; }
}

public sealed class PayslipDto
{
    public Guid Id { get; set; }
    public Guid PayrollId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PayslipNumber { get; set; } = string.Empty;
    public int PayrollMonth { get; set; }
    public int PayrollYear { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string? PdfPath { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? GeneratedBy { get; set; }
    public DateTime? GeneratedAt { get; set; }
}

public sealed class PayrollReportDto
{
    public int TotalEmployees { get; set; }
    public int PayrollGenerated { get; set; }
    public int PayrollApproved { get; set; }
    public int PayrollPending { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalLateDeductions { get; set; }
    public decimal TotalUnpaidLeaveDeductions { get; set; }
    public decimal TotalOtherDeductions { get; set; }
    public decimal TotalNetPayroll { get; set; }
}

public sealed class PayrollDashboardDto
{
    public PayrollReportDto Summary { get; set; } = new();
    public List<PayrollDto> RecentPayrolls { get; set; } = new();
    public List<PayrollDto> EmployeesWithLate { get; set; } = new();
    public List<PayrollDto> EmployeesWithUnpaidLeave { get; set; } = new();
}
