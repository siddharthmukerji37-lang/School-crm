using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Payroll;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class PayrollSettingRepository : GenericRepository<PayrollSetting>, IPayrollSettingRepository
{
    public PayrollSettingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PayrollSetting?> GetActiveAsync(Guid schoolId)
    {
        return await _dbSet.FirstOrDefaultAsync(s =>
            s.SchoolId == schoolId && !s.IsDeleted);
    }
}

public class SalaryProfileRepository : GenericRepository<SalaryProfile>, ISalaryProfileRepository
{
    public SalaryProfileRepository(ApplicationDbContext context) : base(context) { }

    public async Task<SalaryProfile?> GetByUserIdAsync(string userId)
    {
        return await _dbSet.FirstOrDefaultAsync(p =>
            p.UserId == userId && p.IsActive && !p.IsDeleted);
    }

    public async Task<IReadOnlyList<SalaryProfile>> GetBySchoolAsync(Guid schoolId)
    {
        return await _dbSet
            .Where(p => p.SchoolId == schoolId && p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.UserId).ToListAsync();
    }
}

public class SalaryComponentRepository : GenericRepository<SalaryComponent>, ISalaryComponentRepository
{
    public SalaryComponentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<SalaryComponent>> GetByProfileAsync(Guid profileId)
    {
        return await _dbSet
            .Where(c => c.SalaryProfileId == profileId && c.IsActive && !c.IsDeleted)
            .ToListAsync();
    }
}

public class PayrollRepository : GenericRepository<Payroll>, IPayrollRepository
{
    public PayrollRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Payroll?> GetByUserAndMonthAsync(string userId, int month, int year)
    {
        return await _dbSet.FirstOrDefaultAsync(p =>
            p.UserId == userId && p.PayrollMonth == month && p.PayrollYear == year && !p.IsDeleted);
    }

    public async Task<IReadOnlyList<Payroll>> GetByMonthAsync(int month, int year, Guid schoolId)
    {
        return await _dbSet
            .Where(p => p.PayrollMonth == month && p.PayrollYear == year && p.SchoolId == schoolId && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Payroll>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.PayrollYear).ThenByDescending(p => p.PayrollMonth)
            .ToListAsync();
    }
}

public class PayrollDeductionRepository : GenericRepository<PayrollDeduction>, IPayrollDeductionRepository
{
    public PayrollDeductionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<PayrollDeduction>> GetByPayrollIdAsync(Guid payrollId)
    {
        return await _dbSet
            .Where(d => d.PayrollId == payrollId && !d.IsDeleted)
            .ToListAsync();
    }
}

public class PayslipRepository : GenericRepository<Payslip>, IPayslipRepository
{
    public PayslipRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Payslip?> GetByPayrollIdAsync(Guid payrollId)
    {
        return await _dbSet.FirstOrDefaultAsync(p =>
            p.PayrollId == payrollId && !p.IsDeleted);
    }

    public async Task<IReadOnlyList<Payslip>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.PayrollYear).ThenByDescending(p => p.PayrollMonth)
            .ToListAsync();
    }
}
