using SchoolCRM.Domain.Entities.School;

namespace SchoolCRM.Application.Interfaces.Repositories;

public interface ISchoolRepository : IGenericRepository<Domain.Entities.School.School>
{
    Task<Domain.Entities.School.School?> GetSchoolWithDetailsAsync(Guid id);
    Task<Domain.Entities.School.School?> GetSchoolByCodeAsync(string code);
}

public interface IAcademicYearRepository : IGenericRepository<AcademicYear>
{
    Task<AcademicYear?> GetCurrentAcademicYearAsync(Guid schoolId);
    Task<IReadOnlyList<AcademicYear>> GetBySchoolIdAsync(Guid schoolId);
}

public interface IClassRoomRepository : IGenericRepository<ClassRoom>
{
    Task<ClassRoom?> GetClassRoomWithSectionsAsync(Guid id);
    Task<IReadOnlyList<ClassRoom>> GetByAcademicYearAsync(Guid academicYearId);
}

public interface ISectionRepository : IGenericRepository<Section>
{
    Task<Section?> GetSectionWithStudentsAsync(Guid id);
    Task<IReadOnlyList<Section>> GetByClassRoomAsync(Guid classRoomId);
}

public interface ISubjectRepository : IGenericRepository<Subject>
{
    Task<IReadOnlyList<Subject>> GetByClassRoomAsync(Guid classRoomId);
}

public interface ITimetableRepository : IGenericRepository<Timetable>
{
    Task<IReadOnlyList<Timetable>> GetBySectionAndDayAsync(Guid sectionId, DayOfWeek dayOfWeek);
    Task<IReadOnlyList<Timetable>> GetByTeacherAsync(Guid teacherId);
}
