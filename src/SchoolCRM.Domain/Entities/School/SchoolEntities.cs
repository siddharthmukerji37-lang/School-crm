using SchoolCRM.Domain.Common;

namespace SchoolCRM.Domain.Entities.School;

public class School : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrincipalName { get; set; }
    public string? EstablishedDate { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? AffiliationNumber { get; set; }
    public Guid? CurrentAcademicYearId { get; set; }

    public AcademicYear? CurrentAcademicYear { get; set; }
    public ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<ClassRoom> ClassRooms { get; set; } = new List<ClassRoom>();
}

public class AcademicYear : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public Guid SchoolId { get; set; }

    public School School { get; set; } = null!;
    public ICollection<ClassRoom> ClassRooms { get; set; } = new List<ClassRoom>();
}

public class Branch : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? InChargeName { get; set; }
    public Guid SchoolId { get; set; }

    public School School { get; set; } = null!;
}

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid SchoolId { get; set; }
    public Guid? HeadOfDepartmentId { get; set; }

    public School School { get; set; } = null!;
    public ICollection<Teacher.Teacher> Teachers { get; set; } = new List<Teacher.Teacher>();
}

public class ClassRoom : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? RoomNumber { get; set; }
    public string? Building { get; set; }
    public Guid SchoolId { get; set; }
    public Guid AcademicYearId { get; set; }
    public Guid? DepartmentId { get; set; }

    public School School { get; set; } = null!;
    public AcademicYear AcademicYear { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
}

public class Section : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public Guid ClassRoomId { get; set; }
    public Guid? ClassTeacherId { get; set; }

    public ClassRoom ClassRoom { get; set; } = null!;
    public Teacher.Teacher? ClassTeacher { get; set; }
    public ICollection<Student.Student> Students { get; set; } = new List<Student.Student>();
}

public class Subject : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Credits { get; set; }
    public decimal PassMarks { get; set; }
    public decimal TotalMarks { get; set; }
    public bool IsElective { get; set; }
    public int SortOrder { get; set; }
    public Guid ClassRoomId { get; set; }

    public ClassRoom ClassRoom { get; set; } = null!;
    public ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
    public ICollection<Exam.ExamSchedule> ExamSchedules { get; set; } = new List<Exam.ExamSchedule>();
}

public class Timetable : BaseEntity
{
    public DayOfWeek DayOfWeek { get; set; }
    public int PeriodNumber { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Guid ClassRoomId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid TeacherId { get; set; }

    public ClassRoom ClassRoom { get; set; } = null!;
    public Section Section { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public Teacher.Teacher Teacher { get; set; } = null!;
}

public class Period : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int PeriodNumber { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsBreak { get; set; }
    public Guid SchoolId { get; set; }
}

public class Holiday : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? HolidayType { get; set; }
    public Guid SchoolId { get; set; }
}

public class SchoolEvent : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string? EventType { get; set; }
    public bool IsPublic { get; set; }
    public Guid SchoolId { get; set; }
}
