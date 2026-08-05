using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Student;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class StudentRepository : GenericRepository<Student>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Student?> GetStudentWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.User)
            .Include(s => s.Section)
                .ThenInclude(sec => sec.ClassRoom)
            .Include(s => s.Parent)
                .ThenInclude(p => p!.User)
            .Include(s => s.School)
            .Include(s => s.Documents)
            .Include(s => s.HealthRecords)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Student?> GetStudentByAdmissionNumberAsync(string admissionNumber)
    {
        return await _dbSet
            .Include(s => s.User)
            .Include(s => s.Section)
            .FirstOrDefaultAsync(s => s.AdmissionNumber == admissionNumber);
    }

    public async Task<Student?> GetStudentByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(s => s.User)
            .Include(s => s.Section)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<IReadOnlyList<Student>> GetBySectionAsync(Guid sectionId)
    {
        return await _dbSet
            .Include(s => s.User)
            .Where(s => s.SectionId == sectionId)
            .OrderBy(s => s.RollNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Student>> GetBySchoolAsync(Guid schoolId)
    {
        return await _dbSet
            .Include(s => s.User)
            .Include(s => s.Section)
            .Where(s => s.SchoolId == schoolId)
            .OrderBy(s => s.AdmissionNumber)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Student> Items, int TotalCount)> GetPagedStudentsAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortOrder,
        Guid? sectionId, Guid? classRoomId, Guid? schoolId, string? status)
    {
        IQueryable<Student> query = _dbSet
            .Include(s => s.User)
            .Include(s => s.Parent).ThenInclude(p => p.User)
            .Include(s => s.Section)
                .ThenInclude(sec => sec.ClassRoom);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.AdmissionNumber.ToLower().Contains(term) ||
                s.User.FirstName.ToLower().Contains(term) ||
                s.User.LastName.ToLower().Contains(term) ||
                s.User.Email.ToLower().Contains(term));
        }

        if (sectionId.HasValue)
            query = query.Where(s => s.SectionId == sectionId.Value);

        if (classRoomId.HasValue)
            query = query.Where(s => s.Section.ClassRoomId == classRoomId.Value);

        if (schoolId.HasValue)
            query = query.Where(s => s.SchoolId == schoolId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.StudentStatus>(status, true, out var statusEnum))
            query = query.Where(s => s.Status == statusEnum);

        var totalCount = await query.CountAsync();

        query = sortOrder?.ToLower() == "desc"
            ? query.OrderByDescending(s => sortColumn == "name"
                ? s.User.FirstName
                : sortColumn == "admissionNumber"
                    ? s.AdmissionNumber
                    : s.CreatedAt.ToString())
            : query.OrderBy(s => sortColumn == "name"
                ? s.User.FirstName
                : sortColumn == "admissionNumber"
                    ? s.AdmissionNumber
                    : s.CreatedAt.ToString());

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<string> GenerateNextAdmissionNumberAsync(Guid schoolId)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"ADM-{year}-";

        var lastAdmission = await _dbSet
            .Where(s => s.SchoolId == schoolId && s.AdmissionNumber.StartsWith(prefix))
            .OrderByDescending(s => s.AdmissionNumber)
            .Select(s => s.AdmissionNumber)
            .FirstOrDefaultAsync();

        if (lastAdmission is null)
            return $"{prefix}0001";

        var lastNumber = int.Parse(lastAdmission.Split('-').Last());
        return $"{prefix}{(lastNumber + 1):D4}";
    }
}

public class StudentDocumentRepository : GenericRepository<StudentDocument>, IStudentDocumentRepository
{
    public StudentDocumentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<StudentDocument>> GetByStudentAsync(Guid studentId)
    {
        return await _dbSet
            .Where(d => d.StudentId == studentId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }
}

public class StudentHealthRecordRepository : GenericRepository<StudentHealthRecord>, IStudentHealthRecordRepository
{
    public StudentHealthRecordRepository(ApplicationDbContext context) : base(context) { }

    public async Task<StudentHealthRecord?> GetLatestByStudentAsync(Guid studentId)
    {
        return await _dbSet
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.RecordDate)
            .FirstOrDefaultAsync();
    }
}

public class StudentLeaveRepository : GenericRepository<StudentLeave>, IStudentLeaveRepository
{
    public StudentLeaveRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<StudentLeave>> GetByStudentAsync(Guid studentId)
    {
        return await _dbSet
            .Where(l => l.StudentId == studentId)
            .OrderByDescending(l => l.FromDate)
            .ToListAsync();
    }
}
