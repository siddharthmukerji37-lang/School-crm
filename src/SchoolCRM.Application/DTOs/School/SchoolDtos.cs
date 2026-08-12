using System.ComponentModel.DataAnnotations;

namespace SchoolCRM.Application.DTOs.School;

public sealed class SchoolDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? EstablishedDate { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? AffiliationNumber { get; set; }
    public string? PrincipalName { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AcademicYearDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ClassRoomDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? ClassOrder { get; set; }
    public Guid? AcademicYearId { get; set; }
    public string? AcademicYearName { get; set; }
    public Guid? ClassTeacherId { get; set; }
    public string? ClassTeacherName { get; set; }
    public int TotalSections { get; set; }
    public int TotalStudents { get; set; }
    public bool IsActive { get; set; }
}

public sealed class SectionDto
{
    public Guid Id { get; set; }
    public Guid ClassRoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public int? Capacity { get; set; }
    public int CurrentStrength { get; set; }
    public Guid? SectionTeacherId { get; set; }
    public string? SectionTeacherName { get; set; }
    public bool IsActive { get; set; }
}

public sealed class SubjectDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int? SubjectOrder { get; set; }
    public Guid? ClassRoomId { get; set; }
    public decimal? MaxMarks { get; set; }
    public decimal? PassingMarks { get; set; }
    public bool IsElective { get; set; }
    public bool IsActive { get; set; }
}

public sealed class TimetableDto
{
    public Guid Id { get; set; }
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public Guid? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Room { get; set; }
    public Guid? AcademicYearId { get; set; }
    public int PeriodNumber { get; set; }
}

public sealed class DepartmentDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? HeadId { get; set; }
    public string? HeadName { get; set; }
    public int TotalTeachers { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AnnouncementDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TargetRole { get; set; }
    public Guid? ClassRoomId { get; set; }
    public string? ClassName { get; set; }
    public bool IsPublished { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class EventDto
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Location { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Color { get; set; }
    public bool IsHoliday { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateSchoolDto
{
    [Required(ErrorMessage = "School name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "School code is required")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Website { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    public string? LogoUrl { get; set; }

    [MaxLength(100)]
    public string? PrincipalName { get; set; }

    public string? EstablishedDate { get; set; }

    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(100)]
    public string? AffiliationNumber { get; set; }
}
