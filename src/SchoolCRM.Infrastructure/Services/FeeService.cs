using System.Linq.Expressions;
using SchoolCRM.Application.DTOs.Fee;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class FeeService : IFeeService
{
    private readonly IUnitOfWork _unitOfWork;

    public FeeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<FeeStructureDto>>> GetFeeStructuresAsync(
        PaginationQuery query, Guid? classRoomId)
    {
        try
        {
            Expression<Func<Domain.Entities.Fee.FeeStructure, bool>>? filter = fs => !fs.IsDeleted;
            if (classRoomId.HasValue)
                filter = fs => !fs.IsDeleted && fs.ClassRoomId == classRoomId.Value;

            var (items, totalCount) = await _unitOfWork.FeeStructures.GetPagedAsync(
                query.PageNumber, query.PageSize, filter);

            var dtos = items.Select(MapFeeStructureToDto).ToList();

            var pagedResult = new PagedResult<FeeStructureDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm
            };

            return ApiResponse<PagedResult<FeeStructureDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<FeeStructureDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeStructureDto>> GetFeeStructureByIdAsync(Guid id)
    {
        try
        {
            var structure = await _unitOfWork.FeeStructures.GetByIdAsync(id);
            if (structure is null)
                return ApiResponse<FeeStructureDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<FeeStructureDto>.SuccessResponse(MapFeeStructureToDto(structure));
        }
        catch (Exception ex)
        {
            return ApiResponse<FeeStructureDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeStructureDto>> CreateFeeStructureAsync(FeeStructureDto dto)
    {
        try
        {
            if (dto.ClassRoomId == Guid.Empty)
                return ApiResponse<FeeStructureDto>.FailResponse("ClassRoomId is required");
            if (dto.AcademicYearId == Guid.Empty)
                return ApiResponse<FeeStructureDto>.FailResponse("AcademicYearId is required");

            var structure = new Domain.Entities.Fee.FeeStructure
            {
                Name = dto.Name,
                Description = dto.Description,
                Amount = dto.TotalAmount,
                FeeHeadId = dto.Components.FirstOrDefault()?.Id,
                ClassRoomId = dto.ClassRoomId,
                AcademicYearId = dto.AcademicYearId,
                IsRequired = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.FeeStructures.AddAsync(structure);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<FeeStructureDto>.SuccessResponse(MapFeeStructureToDto(structure), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            return ApiResponse<FeeStructureDto>.FailResponse(inner);
        }
    }

    public async Task<ApiResponse<FeeStructureDto>> UpdateFeeStructureAsync(Guid id, FeeStructureDto dto)
    {
        try
        {
            var structure = await _unitOfWork.FeeStructures.GetByIdAsync(id);
            if (structure is null)
                return ApiResponse<FeeStructureDto>.NotFoundResponse(ApplicationMessages.NotFound);

            structure.Name = dto.Name;
            structure.Description = dto.Description;
            structure.Amount = dto.TotalAmount;
            structure.ClassRoomId = dto.ClassRoomId;
            structure.AcademicYearId = dto.AcademicYearId;
            structure.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.FeeStructures.UpdateAsync(structure);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<FeeStructureDto>.SuccessResponse(MapFeeStructureToDto(structure), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<FeeStructureDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteFeeStructureAsync(Guid id)
    {
        try
        {
            var structure = await _unitOfWork.FeeStructures.GetByIdAsync(id);
            if (structure is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            structure.IsDeleted = true;
            structure.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.FeeStructures.UpdateAsync(structure);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<FeeInstallmentDto>>> GetInstallmentsAsync(Guid feeStructureId)
    {
        try
        {
            var installments = await _unitOfWork.FeeInstallments.FindAsync(i => i.FeeStructureId == feeStructureId && !i.IsDeleted);
            var dtos = installments.Select(MapInstallmentToDto).ToList();
            return ApiResponse<List<FeeInstallmentDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<FeeInstallmentDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeInstallmentDto>> CreateInstallmentAsync(FeeInstallmentDto dto)
    {
        try
        {
            var installment = new Domain.Entities.Fee.FeeInstallment
            {
                InstallmentNumber = dto.InstallmentNumber,
                Amount = dto.Amount,
                DueDate = dto.DueDate,
                FeeStructureId = dto.FeeStructureId,
                Status = FeeStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.FeeInstallments.AddAsync(installment);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<FeeInstallmentDto>.SuccessResponse(MapInstallmentToDto(installment), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<FeeInstallmentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeInstallmentDto>> UpdateInstallmentAsync(Guid id, FeeInstallmentDto dto)
    {
        try
        {
            var installment = await _unitOfWork.FeeInstallments.GetByIdAsync(id);
            if (installment is null)
                return ApiResponse<FeeInstallmentDto>.NotFoundResponse(ApplicationMessages.NotFound);

            installment.InstallmentNumber = dto.InstallmentNumber;
            installment.Amount = dto.Amount;
            installment.DueDate = dto.DueDate;
            installment.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.FeeInstallments.UpdateAsync(installment);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<FeeInstallmentDto>.SuccessResponse(MapInstallmentToDto(installment), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<FeeInstallmentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteInstallmentAsync(Guid id)
    {
        try
        {
            var installment = await _unitOfWork.FeeInstallments.GetByIdAsync(id);
            if (installment is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            installment.IsDeleted = true;
            installment.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.FeeInstallments.UpdateAsync(installment);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeReceiptDto>> CollectFeeAsync(CollectFeeDto dto)
    {
        try
        {
            var installment = dto.InstallmentId.HasValue
                ? await _unitOfWork.FeeInstallments.GetByIdAsync(dto.InstallmentId.Value)
                : (await _unitOfWork.FeeInstallments.FindAsync(i =>
                    i.StudentId == dto.StudentId && i.FeeStructureId == dto.FeeStructureId && !i.IsDeleted)).FirstOrDefault();

            if (installment is null)
            {
                var structure = await _unitOfWork.FeeStructures.GetByIdAsync(dto.FeeStructureId);
                if (structure is null)
                    return ApiResponse<FeeReceiptDto>.NotFoundResponse("Fee structure not found.");

                installment = new Domain.Entities.Fee.FeeInstallment
                {
                    InstallmentNumber = 1,
                    Amount = dto.Amount,
                    DueDate = DateTime.UtcNow,
                    FeeStructureId = dto.FeeStructureId,
                    StudentId = dto.StudentId,
                    Status = FeeStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.FeeInstallments.AddAsync(installment);
            }

            installment.PaidAmount += dto.Amount;
            installment.Fine += dto.FineAmount;
            installment.Status = installment.PaidAmount >= installment.Amount
                ? FeeStatus.Paid
                : FeeStatus.Partial;
            installment.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.FeeInstallments.UpdateAsync(installment);

            var receiptNumber = await _unitOfWork.FeeReceipts.GenerateNextReceiptNumberAsync();
            var receipt = new Domain.Entities.Fee.FeeReceipt
            {
                ReceiptNumber = receiptNumber,
                Amount = dto.Amount,
                Fine = dto.FineAmount,
                TotalPaid = dto.Amount + dto.FineAmount,
                PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod.Replace(" ", ""), ignoreCase: true),
                TransactionReference = dto.TransactionReference,
                PaymentNotes = dto.Remarks,
                PaidAt = dto.PaymentDate,
                FeeInstallmentId = installment.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.FeeReceipts.AddAsync(receipt);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<FeeReceiptDto>.SuccessResponse(MapReceiptToDto(receipt), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<FeeReceiptDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<FeeReceiptDto>>> GetFeeReceiptsAsync(
        PaginationQuery query, Guid? studentId, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            Expression<Func<Domain.Entities.Fee.FeeReceipt, bool>>? filter = r => !r.IsDeleted;
            if (studentId.HasValue)
                filter = r => !r.IsDeleted && r.FeeInstallment.StudentId == studentId.Value;

            var (items, totalCount) = await _unitOfWork.FeeReceipts.GetPagedAsync(
                query.PageNumber, query.PageSize, filter,
                q => q.OrderByDescending(r => r.PaidAt));

            var dtos = items.Select(MapReceiptToDto).ToList();

            var pagedResult = new PagedResult<FeeReceiptDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return ApiResponse<PagedResult<FeeReceiptDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<FeeReceiptDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeReceiptDto>> GetFeeReceiptByIdAsync(Guid id)
    {
        try
        {
            var receipt = await _unitOfWork.FeeReceipts.GetByIdAsync(id);
            if (receipt is null)
                return ApiResponse<FeeReceiptDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<FeeReceiptDto>.SuccessResponse(MapReceiptToDto(receipt));
        }
        catch (Exception ex)
        {
            return ApiResponse<FeeReceiptDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeSummaryDto>> GetFeeSummaryAsync(Guid studentId)
    {
        try
        {
            var (totalFees, paidAmount, pendingAmount) = await _unitOfWork.FeeInstallments.GetFeeSummaryAsync(studentId);
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(studentId);
            var installments = await _unitOfWork.FeeInstallments.GetByStudentAsync(studentId);

            var result = new FeeSummaryDto
            {
                StudentId = studentId,
                StudentName = student is not null ? $"{student.User.FirstName} {student.User.LastName}" : string.Empty,
                AdmissionNumber = student?.AdmissionNumber ?? string.Empty,
                ClassName = student?.Section?.ClassRoom?.Name ?? string.Empty,
                TotalFeeAmount = totalFees,
                TotalPaidAmount = paidAmount,
                TotalPendingAmount = pendingAmount,
                Installments = installments.Select(i => new FeeInstallmentStatusDto
                {
                    InstallmentId = i.Id,
                    Name = $"Installment {i.InstallmentNumber}",
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    PaidAmount = i.PaidAmount,
                    PendingAmount = i.Amount - i.PaidAmount,
                    FineAmount = i.Fine,
                    Status = i.Status.ToString()
                }).ToList()
            };

            return ApiResponse<FeeSummaryDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<FeeSummaryDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<FeeSummaryDto>> GetClassFeeSummaryAsync(Guid classRoomId)
    {
        try
        {
            var students = await _unitOfWork.Students.GetPagedStudentsAsync(
                1, 1000, null, null, null, null, classRoomId, null, null);

            var totalFees = 0m;
            var totalPaid = 0m;
            var totalPending = 0m;

            foreach (var student in students.Items)
            {
                var (fees, paid, pending) = await _unitOfWork.FeeInstallments.GetFeeSummaryAsync(student.Id);
                totalFees += fees;
                totalPaid += paid;
                totalPending += pending;
            }

            var result = new FeeSummaryDto
            {
                ClassName = students.Items.FirstOrDefault()?.Section?.ClassRoom?.Name ?? string.Empty,
                TotalFeeAmount = totalFees,
                TotalPaidAmount = totalPaid,
                TotalPendingAmount = totalPending
            };

            return ApiResponse<FeeSummaryDto>.SuccessResponse(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<FeeSummaryDto>.FailResponse(ex.Message);
        }
    }

    private static FeeStructureDto MapFeeStructureToDto(Domain.Entities.Fee.FeeStructure structure)
    {
        return new FeeStructureDto
        {
            Id = structure.Id,
            Name = structure.Name,
            Description = structure.Description,
            ClassRoomId = structure.ClassRoomId,
            ClassName = structure.ClassRoom?.Name ?? string.Empty,
            AcademicYearId = structure.AcademicYearId,
            AcademicYearName = structure.AcademicYear?.Name ?? string.Empty,
            TotalAmount = structure.Amount,
            IsActive = structure.IsRequired,
            Components = new List<FeeComponentDto>(),
            Installments = structure.Installments?.Select(MapInstallmentToDto).ToList() ?? new List<FeeInstallmentDto>()
        };
    }

    private static FeeInstallmentDto MapInstallmentToDto(Domain.Entities.Fee.FeeInstallment installment)
    {
        return new FeeInstallmentDto
        {
            Id = installment.Id,
            FeeStructureId = installment.FeeStructureId,
            Name = $"Installment {installment.InstallmentNumber}",
            Amount = installment.Amount,
            DueDate = installment.DueDate,
            InstallmentNumber = installment.InstallmentNumber
        };
    }

    private static FeeReceiptDto MapReceiptToDto(Domain.Entities.Fee.FeeReceipt receipt)
    {
        return new FeeReceiptDto
        {
            Id = receipt.Id,
            ReceiptNumber = receipt.ReceiptNumber,
            StudentId = receipt.FeeInstallment?.StudentId ?? Guid.Empty,
            StudentName = receipt.FeeInstallment?.Student?.User is not null
                ? $"{receipt.FeeInstallment.Student.User.FirstName} {receipt.FeeInstallment.Student.User.LastName}"
                : string.Empty,
            AdmissionNumber = receipt.FeeInstallment?.Student?.AdmissionNumber ?? string.Empty,
            ClassName = receipt.FeeInstallment?.FeeStructure?.ClassRoom?.Name ?? string.Empty,
            FeeStructureId = receipt.FeeInstallment?.FeeStructureId ?? Guid.Empty,
            FeeStructureName = receipt.FeeInstallment?.FeeStructure?.Name ?? string.Empty,
            InstallmentId = receipt.FeeInstallmentId,
            Amount = receipt.Amount,
            FineAmount = receipt.Fine,
            TotalPaid = receipt.TotalPaid,
            PaymentMethod = receipt.PaymentMethod.ToString(),
            TransactionReference = receipt.TransactionReference,
            PaymentDate = receipt.PaidAt,
            Remarks = receipt.PaymentNotes
        };
    }
}
