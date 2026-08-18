using Microsoft.AspNetCore.Identity;
using SchoolCRM.Application.DTOs.Student;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Entities.Student;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public StudentService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<ApiResponse<PagedResult<StudentDto>>> GetStudentsAsync(
        PaginationQuery query, Guid? sectionId, Guid? classRoomId, Guid? schoolId, string? status)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.Students.GetPagedStudentsAsync(
                query.PageNumber, query.PageSize, query.SearchTerm, query.SortColumn, query.SortOrder,
                sectionId, classRoomId, schoolId, status);

            var dtos = items.Select(MapToDto).ToList();

            var pagedResult = new PagedResult<StudentDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm,
                SortColumn = query.SortColumn,
                SortOrder = query.SortOrder
            };

            return ApiResponse<PagedResult<StudentDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<StudentDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentDto>> GetStudentByIdAsync(Guid id)
    {
        try
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(id);
            if (student is null)
                return ApiResponse<StudentDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<StudentDto>.SuccessResponse(MapToDto(student));
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentDto>> CreateStudentAsync(CreateStudentDto dto)
    {
        try
        {
            var classRoom = await _unitOfWork.ClassRooms.GetByIdAsync(dto.ClassRoomId);
            if (classRoom is null)
                return ApiResponse<StudentDto>.FailResponse("Selected class not found.");

            var section = await _unitOfWork.Sections.GetByIdAsync(dto.SectionId);
            if (section is null)
                return ApiResponse<StudentDto>.FailResponse("Selected section not found.");

            if (section.ClassRoomId != dto.ClassRoomId)
                return ApiResponse<StudentDto>.FailResponse("Selected section does not belong to the chosen class.");

            var admissionNumber = !string.IsNullOrWhiteSpace(dto.AdmissionNumber)
                ? dto.AdmissionNumber.Trim()
                : await _unitOfWork.Students.GenerateNextAdmissionNumberAsync(classRoom.SchoolId);

            if (await _unitOfWork.Students.GetStudentByAdmissionNumberAsync(admissionNumber) is not null)
                return ApiResponse<StudentDto>.FailResponse("A student with this admission number already exists.");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.Phone,
                Gender = Enum.Parse<Gender>(dto.Gender),
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                BloodGroup = BloodGroupExtensions.ParseBloodGroup(dto.BloodGroup),
                ProfilePictureUrl = dto.ProfilePictureUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, string.IsNullOrWhiteSpace(dto.Password) ? "Student@123" : dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<StudentDto>.FailResponse(string.Join("; ", errors));
            }

            await _userManager.AddToRoleAsync(user, Roles.Student);

            var student = new Domain.Entities.Student.Student
            {
                AdmissionNumber = admissionNumber,
                RollNumber = GetRollNumberFromAdmissionNumber(admissionNumber),
                UserId = user.Id,
                SectionId = dto.SectionId,
                SchoolId = classRoom.SchoolId,
                AdmissionDate = dto.AdmissionDate,
                Status = StudentStatus.Active,
                ParentId = dto.ParentId,
                ParentName = dto.ParentName,
                ParentPhone = dto.ParentPhone,
                ParentEmail = dto.ParentEmail,
                TransportRequired = dto.TransportRequired,
                HostelRequired = dto.HostelRequired,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.SaveChangesAsync();

            var password = string.IsNullOrWhiteSpace(dto.Password) ? "Student@123" : dto.Password;
            var studentName = $"{dto.FirstName} {dto.LastName}";
            var className = classRoom.Name;
            var sectionName = section.Name;

            await _notificationService.NotifyUsersAsync(
                new[] { user.Id },
                "Welcome to the school",
                $"Your student account has been created. Email: {dto.Email}",
                Domain.Enums.NotificationType.Success,
                "/profile");

            await _emailService.SendEmailAsync(dto.Email, "Your student account has been created",
                $@"<h3>Welcome, {studentName}!</h3>
                   <p>Your student account has been created.</p>
                   <p><strong>Email:</strong> {dto.Email}</p>
                   <p><strong>Password:</strong> {password}</p>
                   <p><strong>Class:</strong> {className} — {sectionName}</p>
                   <p><strong>Admission No:</strong> {admissionNumber}</p>
                   <p>Please log in and change your password after first login.</p>");

            if (!string.IsNullOrWhiteSpace(dto.ParentEmail))
            {
                await _emailService.SendEmailAsync(dto.ParentEmail,
                    $"Your child {studentName} has been enrolled",
                    $@"<h3>Your child has been enrolled</h3>
                       <p><strong>Student:</strong> {studentName}</p>
                       <p><strong>Email:</strong> {dto.Email}</p>
                       <p><strong>Password:</strong> {password}</p>
                       <p><strong>Class:</strong> {className} — {sectionName}</p>
                       <p><strong>Admission No:</strong> {admissionNumber}</p>
                       <p>Please share these credentials with your child.</p>");
            }

            var created = await _unitOfWork.Students.GetStudentWithDetailsAsync(student.Id);
            return ApiResponse<StudentDto>.SuccessResponse(MapToDto(created!), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return ApiResponse<StudentDto>.FailResponse(message);
        }
    }

    public async Task<ApiResponse<StudentDto>> UpdateStudentAsync(Guid id, UpdateStudentDto dto)
    {
        try
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(id);
            if (student is null)
                return ApiResponse<StudentDto>.NotFoundResponse(ApplicationMessages.NotFound);

            if (!string.IsNullOrWhiteSpace(dto.AdmissionNumber) &&
                dto.AdmissionNumber.Trim() != student.AdmissionNumber &&
                await _unitOfWork.Students.GetStudentByAdmissionNumberAsync(dto.AdmissionNumber.Trim()) is not null)
            {
                return ApiResponse<StudentDto>.FailResponse("A student with this admission number already exists.");
            }

            student.SectionId = dto.SectionId;
            student.AdmissionDate = dto.AdmissionDate;
            if (!string.IsNullOrWhiteSpace(dto.AdmissionNumber))
            {
                student.AdmissionNumber = dto.AdmissionNumber.Trim();
                student.RollNumber = GetRollNumberFromAdmissionNumber(student.AdmissionNumber);
            }
            student.Status = Enum.Parse<StudentStatus>(dto.Status);
            student.ParentName = dto.ParentName;
            student.ParentPhone = dto.ParentPhone;
            student.ParentEmail = dto.ParentEmail;
            student.TransportRequired = dto.TransportRequired;
            student.HostelRequired = dto.HostelRequired;
            student.Notes = dto.Notes;
            student.UpdatedAt = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(student.UserId.ToString());
            if (user is not null)
            {
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Email = dto.Email;
                user.UserName = dto.Email;
                user.PhoneNumber = dto.Phone;
                user.DateOfBirth = dto.DateOfBirth;
                user.Address = dto.Address;
                user.ProfilePictureUrl = dto.ProfilePictureUrl;
                user.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(dto.BloodGroup))
                    user.BloodGroup = BloodGroupExtensions.ParseBloodGroup(dto.BloodGroup);

                if (!string.IsNullOrEmpty(dto.Gender))
                    user.Gender = Enum.Parse<Gender>(dto.Gender);

                await _userManager.UpdateAsync(user);
            }

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Students.GetStudentWithDetailsAsync(id);
            return ApiResponse<StudentDto>.SuccessResponse(MapToDto(updated!), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteStudentAsync(Guid id)
    {
        try
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            student.IsDeleted = true;
            student.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<StudentDto>> PromoteStudentAsync(Guid id, PromoteStudentDto dto)
    {
        try
        {
            var student = await _unitOfWork.Students.GetStudentWithDetailsAsync(id);
            if (student is null)
                return ApiResponse<StudentDto>.NotFoundResponse(ApplicationMessages.NotFound);

            var promotion = new StudentPromotion
            {
                StudentId = id,
                FromSectionId = student.SectionId,
                ToSectionId = dto.ToSectionId,
                FromAcademicYearId = student.Section.ClassRoom.AcademicYearId,
                ToAcademicYearId = dto.ToAcademicYearId,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<StudentPromotion>().AddAsync(promotion);

            student.SectionId = dto.ToSectionId;
            if (dto.NewRollNumber.HasValue)
                student.RollNumber = dto.NewRollNumber.Value.ToString();

            student.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Students.GetStudentWithDetailsAsync(id);
            return ApiResponse<StudentDto>.SuccessResponse(MapToDto(updated!), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<StudentDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<PagedResult<StudentDto>>> SearchStudentsAsync(string searchTerm, PaginationQuery query)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.Students.GetPagedStudentsAsync(
                query.PageNumber, query.PageSize, searchTerm, query.SortColumn, query.SortOrder,
                null, null, null, null);

            var dtos = items.Select(MapToDto).ToList();

            var pagedResult = new PagedResult<StudentDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchTerm = searchTerm
            };

            return ApiResponse<PagedResult<StudentDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<StudentDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<List<StudentDocumentDto>>> GetStudentDocumentsAsync(Guid studentId)
    {
        try
        {
            var documents = await _unitOfWork.StudentDocuments.GetByStudentAsync(studentId);

            var dtos = documents.Select(d => new StudentDocumentDto
            {
                Id = d.Id,
                StudentId = d.StudentId,
                DocumentName = d.DocumentName,
                DocumentType = d.DocumentType.ToString(),
                FileUrl = d.FileUrl,
                FileName = d.FileName,
                FileSize = d.FileSize,
                UploadedAt = d.CreatedAt
            }).ToList();

            return ApiResponse<List<StudentDocumentDto>>.SuccessResponse(dtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<List<StudentDocumentDto>>.FailResponse(ex.Message);
        }
    }

    private static string GetRollNumberFromAdmissionNumber(string admissionNumber)
    {
        if (string.IsNullOrWhiteSpace(admissionNumber))
            return string.Empty;

        var index = admissionNumber.Length;
        while (index > 0 && char.IsDigit(admissionNumber[index - 1]))
            index--;

        var trailingDigits = admissionNumber[index..];
        if (trailingDigits.Length == 0)
            return string.Empty;

        return trailingDigits.Length >= 2 ? trailingDigits[^2..] : trailingDigits;
    }

    private static StudentDto MapToDto(Domain.Entities.Student.Student student)
    {
        return new StudentDto
        {
            Id = student.Id,
            AdmissionNumber = student.AdmissionNumber,
            RollNumber = int.TryParse(student.RollNumber, out var rn) ? rn : 0,
            FirstName = student.User.FirstName,
            LastName = student.User.LastName,
            Email = student.User.Email ?? string.Empty,
            Phone = student.User.PhoneNumber,
            Gender = student.User.Gender.ToString(),
            DateOfBirth = student.User.DateOfBirth ?? DateTime.MinValue,
            SectionId = student.SectionId,
            SectionName = student.Section?.Name ?? string.Empty,
            ClassRoomId = student.Section?.ClassRoomId ?? Guid.Empty,
            ClassName = student.Section?.ClassRoom?.Name ?? string.Empty,
            ParentId = student.ParentId,
            ParentName = !string.IsNullOrWhiteSpace(student.ParentName)
                ? student.ParentName
                : student.Parent?.User is not null
                    ? $"{student.Parent.User.FirstName} {student.Parent.User.LastName}"
                    : null,
            ParentPhone = student.ParentPhone,
            ParentEmail = student.ParentEmail,
            TransportRequired = student.TransportRequired,
            HostelRequired = student.HostelRequired,
            Notes = student.Notes,
            AdmissionDate = student.AdmissionDate,
            Status = student.Status.ToString(),
            ProfilePictureUrl = student.User.ProfilePictureUrl,
            Address = student.User.Address,
            BloodGroup = student.User.BloodGroup.ToDisplayString()
        };
    }
}
