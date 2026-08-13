using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.DTOs.Fee;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Fee;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class FeeService : IFeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;

    public FeeService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IEmailService emailService,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _emailService = emailService;
        _currentUserService = currentUserService;
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
                query.PageNumber, query.PageSize, filter,
                include: q => q
                    .Include(fs => fs.ClassRoom)
                    .Include(fs => fs.AcademicYear)
                    .Include(fs => fs.FeeHead));

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
                FeeType = dto.FeeType,
                Description = dto.Description,
                Amount = dto.TotalAmount,
                FeeHeadId = dto.Components.FirstOrDefault()?.Id,
                ClassRoomId = dto.ClassRoomId,
                AcademicYearId = dto.AcademicYearId,
                IsRequired = dto.IsActive,
                FineAfterDays = dto.FineAfterDays,
                FineAmount = dto.FineAmount,
                FineStartDate = dto.FineStartDate,
                FineEndDate = dto.FineEndDate,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.FeeStructures.AddAsync(structure);
            await _unitOfWork.SaveChangesAsync();

            await NotifyAndEmailStudentsOnFeeCreatedAsync(structure);

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
            structure.FeeType = dto.FeeType;
            structure.Description = dto.Description;
            structure.Amount = dto.TotalAmount;
            structure.ClassRoomId = dto.ClassRoomId;
            structure.AcademicYearId = dto.AcademicYearId;
            structure.FineAfterDays = dto.FineAfterDays;
            structure.FineAmount = dto.FineAmount;
            structure.FineStartDate = dto.FineStartDate;
            structure.FineEndDate = dto.FineEndDate;
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
            var structure = await _unitOfWork.FeeStructures.GetByIdAsync(dto.FeeStructureId);
            if (structure is null)
                return ApiResponse<FeeReceiptDto>.NotFoundResponse("Fee structure not found.");

            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(dto.StudentId);
            if (student is null)
                return ApiResponse<FeeReceiptDto>.NotFoundResponse("Student not found.");

            var studentClassRoomId = student.Section?.ClassRoomId;
            if (studentClassRoomId.HasValue && studentClassRoomId.Value != structure.ClassRoomId)
                return ApiResponse<FeeReceiptDto>.FailResponse(
                    "Student is not enrolled in the class for this fee structure.");

            var installments = (await _unitOfWork.FeeInstallments.FindAsync(i =>
                    i.StudentId == dto.StudentId && i.FeeStructureId == dto.FeeStructureId && !i.IsDeleted))
                .OrderBy(i => i.InstallmentNumber)
                .ToList();

            if (installments.Count == 0)
                installments = await CreateInstallmentsForStudentAsync(structure, dto.StudentId);

            FeeInstallment? target = null;
            if (dto.InstallmentId.HasValue)
            {
                target = installments.FirstOrDefault(i => i.Id == dto.InstallmentId.Value);
                if (target is null)
                    return ApiResponse<FeeReceiptDto>.FailResponse(
                        "Selected installment does not belong to this student and fee structure.");
            }

            var sequence = target is not null
                ? installments.Where(i => i.InstallmentNumber >= target.InstallmentNumber).ToList()
                : installments;

            var remaining = dto.Amount;
            var allocations = new List<(FeeInstallment Installment, decimal Applied, decimal Fine)>();

            foreach (var installment in sequence)
            {
                if (target is not null && installment.Id != target.Id)
                    continue;

                var pending = installment.Amount - installment.PaidAmount;
                if (pending <= 0)
                    continue;

                if (remaining <= 0)
                    break;

                var applied = Math.Min(remaining, pending);
                var isFirstPaid = allocations.Count == 0;
                var fine = ComputeOverdueFine(installment, dto.PaymentDate,
                    isFirstPaid ? dto.FineAmount : 0m, structure.FineAfterDays, structure.FineAmount,
                    structure.FineStartDate, structure.FineEndDate);

                installment.PaidAmount += applied;
                installment.Fine += fine;
                installment.Status = installment.PaidAmount >= installment.Amount
                    ? FeeStatus.Paid
                    : FeeStatus.Partial;
                installment.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.FeeInstallments.UpdateAsync(installment);
                allocations.Add((installment, applied, fine));
                remaining -= applied;
            }

            if (target is not null && allocations.Count == 0)
                return ApiResponse<FeeReceiptDto>.FailResponse("Selected installment is already paid.");

            if (allocations.Count == 0)
                return ApiResponse<FeeReceiptDto>.FailResponse(
                    "All installments for this fee structure are already paid.");

            if (remaining > 0)
                return ApiResponse<FeeReceiptDto>.FailResponse(
                    $"Payment amount exceeds the outstanding balance of {(dto.Amount - remaining):C}.");

            var receivedBy = string.IsNullOrWhiteSpace(dto.ReceivedBy)
                ? _currentUserService.FullName
                : dto.ReceivedBy;

            FeeReceipt? primaryReceipt = null;
            foreach (var (installment, applied, fine) in allocations)
            {
                var receiptNumber = await _unitOfWork.FeeReceipts.GenerateNextReceiptNumberAsync();
                var receipt = new FeeReceipt
                {
                    ReceiptNumber = receiptNumber,
                    Amount = applied,
                    Fine = fine,
                    TotalPaid = applied + fine,
                    PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod.Replace(" ", ""), ignoreCase: true),
                    TransactionReference = dto.TransactionReference,
                    PaymentNotes = dto.Remarks,
                    PaidAt = dto.PaymentDate,
                    FeeInstallmentId = installment.Id,
                    ReceivedBy = receivedBy,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.FeeReceipts.AddAsync(receipt);
                primaryReceipt ??= receipt;
            }

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<FeeReceiptDto>.SuccessResponse(
                MapReceiptToDto(primaryReceipt!), ApplicationMessages.CreateSuccess);
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
                q => q.OrderByDescending(r => r.PaidAt),
                include: q => q
                    .Include(r => r.FeeInstallment)
                        .ThenInclude(fi => fi.Student)
                            .ThenInclude(s => s.User)
                    .Include(r => r.FeeInstallment)
                        .ThenInclude(fi => fi.FeeStructure)
                            .ThenInclude(fs => fs.ClassRoom));

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

    public async Task<ApiResponse<PagedResult<FeeReceiptDto>>> GetMyFeeReceiptsAsync(PaginationQuery query)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentUserService.UserId) ||
                !Guid.TryParse(_currentUserService.UserId, out var userId))
                return ApiResponse<PagedResult<FeeReceiptDto>>.FailResponse("Unable to identify current user.");

            var student = await _unitOfWork.Students.GetStudentByUserIdAsync(userId);
            if (student is null)
                return ApiResponse<PagedResult<FeeReceiptDto>>.FailResponse("No student profile linked to this account.");

            return await GetFeeReceiptsAsync(query, student.Id, null, null);
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
                    FeeStructureId = i.FeeStructureId,
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

    private async Task<List<Domain.Entities.Fee.FeeInstallment>> CreateInstallmentsForStudentAsync(
        Domain.Entities.Fee.FeeStructure structure, Guid studentId)
    {
        var templates = (await _unitOfWork.FeeInstallments.FindAsync(i =>
                i.FeeStructureId == structure.Id && i.StudentId == Guid.Empty && !i.IsDeleted))
            .OrderBy(i => i.InstallmentNumber)
            .ToList();

        var created = new List<Domain.Entities.Fee.FeeInstallment>();

        if (templates.Count > 0)
        {
            foreach (var template in templates)
            {
                var installment = new Domain.Entities.Fee.FeeInstallment
                {
                    InstallmentNumber = template.InstallmentNumber,
                    Amount = template.Amount,
                    DueDate = template.DueDate,
                    FeeStructureId = structure.Id,
                    StudentId = studentId,
                    Status = FeeStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.FeeInstallments.AddAsync(installment);
                created.Add(installment);
            }

            return created;
        }

        var single = new Domain.Entities.Fee.FeeInstallment
        {
            InstallmentNumber = 1,
            Amount = structure.Amount,
            DueDate = DateTime.UtcNow,
            FeeStructureId = structure.Id,
            StudentId = studentId,
            Status = FeeStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.FeeInstallments.AddAsync(single);
        created.Add(single);
        return created;
    }

    private static decimal ComputeOverdueFine(
        Domain.Entities.Fee.FeeInstallment installment, DateTime paymentDate, decimal manualFine,
        int fineAfterDays, decimal fineAmount, DateTime? fineStartDate, DateTime? fineEndDate)
    {
        if (manualFine > 0)
            return manualFine;

        if (fineStartDate.HasValue && fineEndDate.HasValue)
        {
            if (paymentDate.Date >= fineStartDate.Value.Date && paymentDate.Date <= fineEndDate.Value.Date)
                return 0m;

            if (paymentDate.Date > fineEndDate.Value.Date && fineAmount > 0)
                return fineAmount;
        }

        var daysLate = (paymentDate.Date - installment.DueDate.Date).Days;
        if (daysLate <= fineAfterDays || fineAmount <= 0)
            return 0m;

        return fineAmount;
    }

    private static FeeStructureDto MapFeeStructureToDto(Domain.Entities.Fee.FeeStructure structure)
    {
        return new FeeStructureDto
        {
            Id = structure.Id,
            Name = structure.Name,
            Description = structure.Description,
            FeeType = structure.FeeType,
            ClassRoomId = structure.ClassRoomId,
            ClassName = structure.ClassRoom?.Name ?? string.Empty,
            AcademicYearId = structure.AcademicYearId,
            AcademicYearName = structure.AcademicYear?.Name ?? string.Empty,
            TotalAmount = structure.Amount,
            FineAfterDays = structure.FineAfterDays,
            FineAmount = structure.FineAmount,
            FineStartDate = structure.FineStartDate,
            FineEndDate = structure.FineEndDate,
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
            FeeType = receipt.FeeInstallment?.FeeStructure?.FeeType ?? string.Empty,
            InstallmentId = receipt.FeeInstallmentId,
            InstallmentName = receipt.FeeInstallment is not null
                ? $"Installment {receipt.FeeInstallment.InstallmentNumber}"
                : string.Empty,
            Amount = receipt.Amount,
            FineAmount = receipt.Fine,
            TotalPaid = receipt.TotalPaid,
            PaymentMethod = receipt.PaymentMethod.ToString(),
            TransactionReference = receipt.TransactionReference,
            PaymentDate = receipt.PaidAt,
            Remarks = receipt.PaymentNotes,
            PaidBy = receipt.ReceivedBy ?? string.Empty
        };
    }

    private async Task NotifyAndEmailStudentsOnFeeCreatedAsync(Domain.Entities.Fee.FeeStructure structure)
    {
        try
        {
            var title = "New fee posted";
            var message = $"A new fee '{structure.Name}' of {structure.Amount:C} has been posted for your class.";
            var link = "/fees";

            await _notificationService.NotifyStudentsOfClassAsync(
                structure.ClassRoomId, title, message, NotificationType.Info, link: link);

            var sections = (await _unitOfWork.Sections.FindAsync(s =>
                s.ClassRoomId == structure.ClassRoomId && !s.IsDeleted)).ToList();

            var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var section in sections)
            {
                var students = await _unitOfWork.Students.GetBySectionAsync(section.Id);
                foreach (var student in students.Where(s => !s.IsDeleted && !string.IsNullOrWhiteSpace(s.ParentEmail)))
                    emails.Add(student.ParentEmail!);
            }

            var subject = $"New fee: {structure.Name}";
            var body = $@"
                <h3>New fee posted</h3>
                <p><strong>{structure.Name}</strong> ({structure.Amount:C})</p>
                <p>{structure.Description}</p>
                <p>Please complete the payment before the due date.</p>";

            foreach (var email in emails)
                await _emailService.SendEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to notify on fee created: {ex.Message}");
        }
    }
}
