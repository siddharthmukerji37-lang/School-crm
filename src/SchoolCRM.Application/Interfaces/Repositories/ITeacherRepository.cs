using SchoolCRM.Domain.Entities.Teacher;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface ITeacherRepository : IGenericRepository<Teacher>
{
    Task<Teacher?> GetTeacherWithDetailsAsync(Guid id);
    Task<Teacher?> GetTeacherByUserIdAsync(Guid userId);
    Task<Teacher?> GetTeacherByEmployeeCodeAsync(string employeeCode);
    Task<(IReadOnlyList<Teacher> Items, int TotalCount)> GetPagedTeachersAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortOrder,
        Guid? departmentId, Guid? schoolId, string? status);
}

public interface ITeacherLeaveRepository : IGenericRepository<TeacherLeave>
{
    Task<IReadOnlyList<TeacherLeave>> GetByTeacherAsync(Guid teacherId);
}

public interface ITeacherSalaryRepository : IGenericRepository<TeacherSalary>
{
    Task<IReadOnlyList<TeacherSalary>> GetByTeacherAndMonthAsync(Guid teacherId, int month, int year);
}
