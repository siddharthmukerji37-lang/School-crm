using SchoolCRM.Domain.Entities.Employee;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<Employee?> GetEmployeeWithDetailsAsync(Guid id);
    Task<Employee?> GetEmployeeByUserIdAsync(Guid userId);
    Task<(IReadOnlyList<Employee> Items, int TotalCount)> GetPagedEmployeesAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortOrder,
        Guid? departmentId, Guid? designationId, string? status);
}

public interface IEmployeeLeaveRepository : IGenericRepository<EmployeeLeave>
{
    Task<IReadOnlyList<EmployeeLeave>> GetByEmployeeAsync(Guid employeeId);
}

public interface IEmployeeSalaryRepository : IGenericRepository<EmployeeSalary>
{
    Task<IReadOnlyList<EmployeeSalary>> GetByEmployeeAndMonthAsync(Guid employeeId, int month, int year);
}

public interface IDesignationRepository : IGenericRepository<Designation>
{
    new Task<IReadOnlyList<Designation>> GetAllAsync();
}
