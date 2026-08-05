using Microsoft.AspNetCore.Identity;
using SchoolCRM.Application.DTOs.Teacher;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Entities.Teacher;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class TeacherService : ITeacherService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public TeacherService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<ApiResponse<PagedResult<TeacherDto>>> GetTeachersAsync(
        PaginationQuery query, Guid? departmentId, string? status)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.Teachers.GetPagedTeachersAsync(
                query.PageNumber, query.PageSize, query.SearchTerm, query.SortColumn, query.SortOrder,
                departmentId, null, status);

            var dtos = items.Select(MapToDto).ToList();

            var pagedResult = new PagedResult<TeacherDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm,
                SortColumn = query.SortColumn,
                SortOrder = query.SortOrder
            };

            return ApiResponse<PagedResult<TeacherDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<TeacherDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<TeacherDto>> GetTeacherByIdAsync(Guid id)
    {
        try
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(id);
            if (teacher is null)
                return ApiResponse<TeacherDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<TeacherDto>.SuccessResponse(MapToDto(teacher));
        }
        catch (Exception ex)
        {
            return ApiResponse<TeacherDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<TeacherDto>> CreateTeacherAsync(CreateTeacherDto dto)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
                return ApiResponse<TeacherDto>.FailResponse(ApplicationMessages.DuplicateRecord);

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

            var result = await _userManager.CreateAsync(user, dto.Password ?? "Teacher@123");
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<TeacherDto>.FailResponse(string.Join("; ", errors));
            }

            await _userManager.AddToRoleAsync(user, Roles.Teacher);

            var employeeCode = $"TCH-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            var teacher = new Domain.Entities.Teacher.Teacher
            {
                EmployeeCode = employeeCode,
                UserId = user.Id,
                SchoolId = null,
                DepartmentId = dto.DepartmentId,
                DepartmentName = dto.DepartmentName,
                JoiningDate = dto.JoiningDate,
                Status = TeacherStatus.Active,
                Qualification = dto.Qualification,
                Specialization = dto.Specialization,
                ExperienceYears = dto.Experience ?? 0,
                EmploymentType = "Full-Time",
                BasicSalary = dto.Salary,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Teachers.AddAsync(teacher);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(teacher.Id);
            return ApiResponse<TeacherDto>.SuccessResponse(MapToDto(created!), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<TeacherDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<TeacherDto>> UpdateTeacherAsync(Guid id, UpdateTeacherDto dto)
    {
        try
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(id);
            if (teacher is null)
                return ApiResponse<TeacherDto>.NotFoundResponse(ApplicationMessages.NotFound);

            teacher.DepartmentId = dto.DepartmentId;
            teacher.DepartmentName = dto.DepartmentName;
            teacher.Qualification = dto.Qualification;
            teacher.Specialization = dto.Specialization;
            teacher.ExperienceYears = dto.Experience ?? 0;
            teacher.BasicSalary = dto.Salary;
            teacher.Status = Enum.Parse<TeacherStatus>(dto.Status);
            teacher.UpdatedAt = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(teacher.UserId.ToString());
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

            await _unitOfWork.Teachers.UpdateAsync(teacher);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Teachers.GetTeacherWithDetailsAsync(id);
            return ApiResponse<TeacherDto>.SuccessResponse(MapToDto(updated!), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<TeacherDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteTeacherAsync(Guid id)
    {
        try
        {
            var teacher = await _unitOfWork.Teachers.GetByIdAsync(id);
            if (teacher is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            teacher.IsDeleted = true;
            teacher.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Teachers.UpdateAsync(teacher);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    private static TeacherDto MapToDto(Domain.Entities.Teacher.Teacher teacher)
    {
        return new TeacherDto
        {
            Id = teacher.Id,
            EmployeeId = teacher.EmployeeCode,
            FirstName = teacher.User.FirstName,
            LastName = teacher.User.LastName,
            Email = teacher.User.Email ?? string.Empty,
            Phone = teacher.User.PhoneNumber,
            Gender = teacher.User.Gender.ToString(),
            DateOfBirth = teacher.User.DateOfBirth ?? DateTime.MinValue,
            JoiningDate = teacher.JoiningDate,
            DepartmentId = teacher.DepartmentId,
            DepartmentName = teacher.DepartmentName ?? teacher.Department?.Name ?? string.Empty,
            Qualification = teacher.Qualification,
            Salary = teacher.BasicSalary,
            Address = teacher.User.Address,
            BloodGroup = teacher.User.BloodGroup?.ToString(),
            ProfilePictureUrl = teacher.User.ProfilePictureUrl,
            Status = teacher.Status.ToString(),
            Specialization = teacher.Specialization,
            Experience = teacher.ExperienceYears
        };
    }
}
