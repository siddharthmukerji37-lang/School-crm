using Microsoft.EntityFrameworkCore;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.School;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Infrastructure.Repositories;

public class SchoolRepository : GenericRepository<Domain.Entities.School.School>, ISchoolRepository
{
    public SchoolRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Domain.Entities.School.School?> GetSchoolWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.CurrentAcademicYear)
            .Include(s => s.Branches)
            .Include(s => s.Departments)
            .Include(s => s.ClassRooms)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Domain.Entities.School.School?> GetSchoolByCodeAsync(string code)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Code == code);
    }
}

public class AcademicYearRepository : GenericRepository<AcademicYear>, IAcademicYearRepository
{
    public AcademicYearRepository(ApplicationDbContext context) : base(context) { }

    public async Task<AcademicYear?> GetCurrentAcademicYearAsync(Guid schoolId)
    {
        return await _dbSet.FirstOrDefaultAsync(ay => ay.SchoolId == schoolId && ay.IsCurrent);
    }

    public async Task<IReadOnlyList<AcademicYear>> GetBySchoolIdAsync(Guid schoolId)
    {
        return await _dbSet
            .Where(ay => ay.SchoolId == schoolId)
            .OrderByDescending(ay => ay.StartDate)
            .ToListAsync();
    }
}

public class ClassRoomRepository : GenericRepository<ClassRoom>, IClassRoomRepository
{
    public ClassRoomRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ClassRoom?> GetClassRoomWithSectionsAsync(Guid id)
    {
        return await _dbSet
            .Include(cr => cr.Sections)
            .Include(cr => cr.AcademicYear)
            .Include(cr => cr.Department)
            .FirstOrDefaultAsync(cr => cr.Id == id);
    }

    public async Task<IReadOnlyList<ClassRoom>> GetByAcademicYearAsync(Guid academicYearId)
    {
        return await _dbSet
            .Where(cr => cr.AcademicYearId == academicYearId)
            .OrderBy(cr => cr.Name)
            .ToListAsync();
    }
}

public class SectionRepository : GenericRepository<Section>, ISectionRepository
{
    public SectionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Section?> GetSectionWithStudentsAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.Students)
                .ThenInclude(st => st.User)
            .Include(s => s.ClassTeacher)
            .Include(s => s.ClassRoom)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IReadOnlyList<Section>> GetByClassRoomAsync(Guid classRoomId)
    {
        return await _dbSet
            .Where(s => s.ClassRoomId == classRoomId)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }
}

public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
{
    public SubjectRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Subject>> GetByClassRoomAsync(Guid classRoomId)
    {
        return await _dbSet
            .Where(s => s.ClassRoomId == classRoomId)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();
    }
}

public class TimetableRepository : GenericRepository<Timetable>, ITimetableRepository
{
    public TimetableRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Timetable>> GetBySectionAndDayAsync(Guid sectionId, DayOfWeek dayOfWeek)
    {
        return await _dbSet
            .Include(t => t.Subject)
            .Include(t => t.Teacher)
                .ThenInclude(t => t.User)
            .Include(t => t.Section)
                .ThenInclude(s => s.ClassRoom)
            .Where(t => t.SectionId == sectionId && t.DayOfWeek == dayOfWeek)
            .OrderBy(t => t.PeriodNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Timetable>> GetByTeacherAsync(Guid teacherId)
    {
        return await _dbSet
            .Include(t => t.Subject)
            .Include(t => t.Teacher)
                .ThenInclude(t => t.User)
            .Include(t => t.Section)
                .ThenInclude(s => s.ClassRoom)
            .Where(t => t.TeacherId == teacherId)
            .OrderBy(t => t.DayOfWeek)
            .ThenBy(t => t.PeriodNumber)
            .ToListAsync();
    }
}
