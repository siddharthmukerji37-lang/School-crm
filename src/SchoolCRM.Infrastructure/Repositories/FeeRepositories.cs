using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Fee;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class FeeStructureRepository : GenericRepository<FeeStructure>, IFeeStructureRepository
{
    public FeeStructureRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<FeeStructure>> GetByClassRoomAsync(Guid classRoomId, Guid academicYearId)
    {
        return await _dbSet
            .Include(fs => fs.FeeHead)
            .Include(fs => fs.ClassRoom)
            .Include(fs => fs.AcademicYear)
            .Where(fs => fs.ClassRoomId == classRoomId && fs.AcademicYearId == academicYearId)
            .OrderBy(fs => fs.FeeHead.Name)
            .ToListAsync();
    }
}

public class FeeInstallmentRepository : GenericRepository<FeeInstallment>, IFeeInstallmentRepository
{
    public FeeInstallmentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<FeeInstallment>> GetByStudentAsync(Guid studentId)
    {
        return await _dbSet
            .Include(fi => fi.FeeStructure)
                .ThenInclude(fs => fs!.FeeHead)
            .Where(fi => fi.StudentId == studentId)
            .OrderBy(fi => fi.DueDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<FeeInstallment>> GetPendingByStudentAsync(Guid studentId)
    {
        return await _dbSet
            .Include(fi => fi.FeeStructure)
                .ThenInclude(fs => fs!.FeeHead)
            .Where(fi => fi.StudentId == studentId
                && (fi.Status == Domain.Enums.FeeStatus.Pending
                    || fi.Status == Domain.Enums.FeeStatus.Partial
                    || fi.Status == Domain.Enums.FeeStatus.Overdue))
            .OrderBy(fi => fi.DueDate)
            .ToListAsync();
    }

    public async Task<(decimal TotalFees, decimal PaidAmount, decimal PendingAmount)> GetFeeSummaryAsync(Guid studentId)
    {
        var result = await _dbSet
            .Where(fi => fi.StudentId == studentId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalFees = g.Sum(fi => fi.Amount),
                PaidAmount = g.Sum(fi => fi.PaidAmount),
            })
            .FirstOrDefaultAsync();

        if (result is null)
            return (0m, 0m, 0m);

        var pending = result.TotalFees - result.PaidAmount;
        return (result.TotalFees, result.PaidAmount, pending < 0 ? 0m : pending);
    }

    public async Task<bool> HasOutstandingFeesAsync(Guid studentId)
    {
        return await _dbSet.AnyAsync(fi =>
            fi.StudentId == studentId && fi.PaidAmount < fi.Amount && !fi.IsDeleted);
    }
}

public class FeeReceiptRepository : GenericRepository<FeeReceipt>, IFeeReceiptRepository
{
    public FeeReceiptRepository(ApplicationDbContext context) : base(context) { }

    public async Task<FeeReceipt?> GetByReceiptNumberAsync(string receiptNumber)
    {
        return await _dbSet
            .Include(fr => fr.FeeInstallment)
                .ThenInclude(fi => fi!.Student)
            .FirstOrDefaultAsync(fr => fr.ReceiptNumber == receiptNumber);
    }

    public async Task<IReadOnlyList<FeeReceipt>> GetByInstallmentAsync(Guid installmentId)
    {
        return await _dbSet
            .Where(fr => fr.FeeInstallmentId == installmentId)
            .OrderBy(fr => fr.PaidAt)
            .ToListAsync();
    }

    public async Task<string> GenerateNextReceiptNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"RCT-{year}-";

        var lastReceipt = await _dbSet
            .Where(fr => fr.ReceiptNumber.StartsWith(prefix))
            .OrderByDescending(fr => fr.ReceiptNumber)
            .Select(fr => fr.ReceiptNumber)
            .FirstOrDefaultAsync();

        if (lastReceipt is null)
            return $"{prefix}00001";

        var lastNumber = int.Parse(lastReceipt.Split('-').Last());
        return $"{prefix}{(lastNumber + 1):D5}";
    }
}
