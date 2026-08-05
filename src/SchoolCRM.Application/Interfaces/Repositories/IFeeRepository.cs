using SchoolCRM.Domain.Entities.Fee;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IFeeStructureRepository : IGenericRepository<FeeStructure>
{
    Task<IReadOnlyList<FeeStructure>> GetByClassRoomAsync(Guid classRoomId, Guid academicYearId);
}

public interface IFeeInstallmentRepository : IGenericRepository<FeeInstallment>
{
    Task<IReadOnlyList<FeeInstallment>> GetByStudentAsync(Guid studentId);
    Task<IReadOnlyList<FeeInstallment>> GetPendingByStudentAsync(Guid studentId);
    Task<(decimal TotalFees, decimal PaidAmount, decimal PendingAmount)> GetFeeSummaryAsync(Guid studentId);
}

public interface IFeeReceiptRepository : IGenericRepository<FeeReceipt>
{
    Task<FeeReceipt?> GetByReceiptNumberAsync(string receiptNumber);
    Task<IReadOnlyList<FeeReceipt>> GetByInstallmentAsync(Guid installmentId);
    Task<string> GenerateNextReceiptNumberAsync();
}
