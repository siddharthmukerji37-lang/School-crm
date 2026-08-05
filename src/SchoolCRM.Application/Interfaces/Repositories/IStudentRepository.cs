using SchoolCRM.Domain.Entities.Student;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface IStudentRepository : IGenericRepository<Student>
{
    Task<Student?> GetStudentWithDetailsAsync(Guid id);
    Task<Student?> GetStudentByAdmissionNumberAsync(string admissionNumber);
    Task<Student?> GetStudentByUserIdAsync(Guid userId);
    Task<IReadOnlyList<Student>> GetBySectionAsync(Guid sectionId);
    Task<IReadOnlyList<Student>> GetBySchoolAsync(Guid schoolId);
    Task<(IReadOnlyList<Student> Items, int TotalCount)> GetPagedStudentsAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortOrder,
        Guid? sectionId, Guid? classRoomId, Guid? schoolId, string? status);
    Task<string> GenerateNextAdmissionNumberAsync(Guid schoolId);
}

public interface IStudentDocumentRepository : IGenericRepository<StudentDocument>
{
    Task<IReadOnlyList<StudentDocument>> GetByStudentAsync(Guid studentId);
}

public interface IStudentHealthRecordRepository : IGenericRepository<StudentHealthRecord>
{
    Task<StudentHealthRecord?> GetLatestByStudentAsync(Guid studentId);
}

public interface IStudentLeaveRepository : IGenericRepository<StudentLeave>
{
    Task<IReadOnlyList<StudentLeave>> GetByStudentAsync(Guid studentId);
}
