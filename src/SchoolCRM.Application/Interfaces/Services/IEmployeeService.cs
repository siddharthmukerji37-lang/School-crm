using SchoolCRM.Application.DTOs.Employee;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Application.Interfaces.Services;

public interface IEmployeeService
{
    Task<ApiResponse<PagedResult<EmployeeDto>>> GetEmployeesAsync(
        PaginationQuery query, Guid? departmentId, string? status);

    Task<ApiResponse<EmployeeDto>> GetEmployeeByIdAsync(Guid id);

    Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto dto);

    Task<ApiResponse<EmployeeDto>> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto dto);

    Task<ApiResponse> DeleteEmployeeAsync(Guid id);
}
