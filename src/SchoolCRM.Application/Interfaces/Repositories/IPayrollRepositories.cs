using SchoolCRM.Domain.Entities.Payroll;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IPayrollSettingRepository : IGenericRepository<PayrollSetting>
{
    Task<PayrollSetting?> GetActiveAsync(Guid schoolId);
}

public interface ISalaryProfileRepository : IGenericRepository<SalaryProfile>
{
    Task<SalaryProfile?> GetByUserIdAsync(string userId);
    Task<IReadOnlyList<SalaryProfile>> GetBySchoolAsync(Guid schoolId);
}

public interface ISalaryComponentRepository : IGenericRepository<SalaryComponent>
{
    Task<IReadOnlyList<SalaryComponent>> GetByProfileAsync(Guid profileId);
}

public interface IPayrollRepository : IGenericRepository<Payroll>
{
    Task<Payroll?> GetByUserAndMonthAsync(string userId, int month, int year);
    Task<IReadOnlyList<Payroll>> GetByMonthAsync(int month, int year, Guid schoolId);
    Task<IReadOnlyList<Payroll>> GetByUserIdAsync(string userId);
}

public interface IPayrollDeductionRepository : IGenericRepository<PayrollDeduction>
{
    Task<IReadOnlyList<PayrollDeduction>> GetByPayrollIdAsync(Guid payrollId);
}

public interface IPayslipRepository : IGenericRepository<Payslip>
{
    Task<Payslip?> GetByPayrollIdAsync(Guid payrollId);
    Task<IReadOnlyList<Payslip>> GetByUserIdAsync(string userId);
}
