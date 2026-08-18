using Microsoft.AspNetCore.Identity;
using SchoolCRM.Application.DTOs.Employee;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public EmployeeService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<PagedResult<EmployeeDto>>> GetEmployeesAsync(
        PaginationQuery query, Guid? departmentId, string? status)
    {
        try
        {
            var (items, totalCount) = await _unitOfWork.Employees.GetPagedEmployeesAsync(
                query.PageNumber, query.PageSize, query.SearchTerm, query.SortColumn, query.SortOrder,
                departmentId, null, status);

            var dtos = items.Select(MapToDto).ToList();

            var pagedResult = new PagedResult<EmployeeDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                SearchTerm = query.SearchTerm,
                SortColumn = query.SortColumn,
                SortOrder = query.SortOrder
            };

            return ApiResponse<PagedResult<EmployeeDto>>.SuccessResponse(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<EmployeeDto>>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<EmployeeDto>> GetEmployeeByIdAsync(Guid id)
    {
        try
        {
            var employee = await _unitOfWork.Employees.GetEmployeeWithDetailsAsync(id);
            if (employee is null)
                return ApiResponse<EmployeeDto>.NotFoundResponse(ApplicationMessages.NotFound);

            return ApiResponse<EmployeeDto>.SuccessResponse(MapToDto(employee));
        }
        catch (Exception ex)
        {
            return ApiResponse<EmployeeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        try
        {
            var schoolId = _currentUserService.SchoolId;
            if (schoolId is null || schoolId == Guid.Empty)
            {
                var schools = await _unitOfWork.Schools.GetAllAsync();
                schoolId = schools.FirstOrDefault()?.Id;
            }

            if (schoolId is null || schoolId == Guid.Empty)
                return ApiResponse<EmployeeDto>.FailResponse("Unable to determine the current school context. Please sign in again.");

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser is not null)
                return ApiResponse<EmployeeDto>.FailResponse(ApplicationMessages.DuplicateRecord);

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

            var result = await _userManager.CreateAsync(user, dto.Password ?? "Employee@123");
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<EmployeeDto>.FailResponse(string.Join("; ", errors));
            }

            await _userManager.AddToRoleAsync(user, Roles.Receptionist);

            var employeeCode = $"EMP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";

            var employee = new Domain.Entities.Employee.Employee
            {
                EmployeeCode = employeeCode,
                UserId = user.Id,
                SchoolId = schoolId.Value,
                DepartmentId = dto.DepartmentId,
                DesignationName = dto.Designation,
                JoiningDate = dto.JoiningDate,
                Status = EmployeeStatus.Active,
                EmploymentType = dto.EmployeeType,
                BasicSalary = dto.Salary,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Employees.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Employees.GetEmployeeWithDetailsAsync(employee.Id);
            return ApiResponse<EmployeeDto>.SuccessResponse(MapToDto(created!), ApplicationMessages.CreateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<EmployeeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<EmployeeDto>> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto dto)
    {
        try
        {
            var employee = await _unitOfWork.Employees.GetEmployeeWithDetailsAsync(id);
            if (employee is null)
                return ApiResponse<EmployeeDto>.NotFoundResponse(ApplicationMessages.NotFound);

            employee.DepartmentId = dto.DepartmentId;
            employee.DesignationName = dto.Designation;
            employee.EmploymentType = dto.EmployeeType;
            employee.BasicSalary = dto.Salary;
            employee.Status = Enum.Parse<EmployeeStatus>(dto.Status);
            employee.UpdatedAt = DateTime.UtcNow;

            var user = await _userManager.FindByIdAsync(employee.UserId.ToString());
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

            await _unitOfWork.Employees.UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Employees.GetEmployeeWithDetailsAsync(id);
            return ApiResponse<EmployeeDto>.SuccessResponse(MapToDto(updated!), ApplicationMessages.UpdateSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse<EmployeeDto>.FailResponse(ex.Message);
        }
    }

    public async Task<ApiResponse> DeleteEmployeeAsync(Guid id)
    {
        try
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee is null)
                return ApiResponse.FailResponse(ApplicationMessages.NotFound);

            employee.IsDeleted = true;
            employee.DeletedAt = DateTime.UtcNow;
            await _unitOfWork.Employees.UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse.SuccessResponse(ApplicationMessages.DeleteSuccess);
        }
        catch (Exception ex)
        {
            return ApiResponse.FailResponse(ex.Message);
        }
    }

    private static EmployeeDto MapToDto(Domain.Entities.Employee.Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.User.FirstName,
            LastName = employee.User.LastName,
            Email = employee.User.Email ?? string.Empty,
            Phone = employee.User.PhoneNumber,
            Gender = employee.User.Gender.ToString(),
            DateOfBirth = employee.User.DateOfBirth ?? DateTime.MinValue,
            JoiningDate = employee.JoiningDate,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name,
            Designation = employee.DesignationName ?? employee.Designation?.Name,
            EmployeeType = employee.EmploymentType,
            Salary = employee.BasicSalary,
            Address = employee.User.Address,
            BloodGroup = employee.User.BloodGroup?.ToString(),
            ProfilePictureUrl = employee.User.ProfilePictureUrl,
            Status = employee.Status.ToString()
        };
    }
}


