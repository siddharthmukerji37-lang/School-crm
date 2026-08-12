using System.ComponentModel.DataAnnotations;

namespace SchoolCRM.Application.DTOs.Student;

public sealed class StudentDto
{
    public Guid Id { get; set; }
    public string AdmissionNumber { get; set; } = string.Empty;
    public int RollNumber { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public Guid ClassRoomId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? ParentPhone { get; set; }
    public string? ParentEmail { get; set; }
    public bool TransportRequired { get; set; }
    public bool HostelRequired { get; set; }
    public string? Notes { get; set; }
    public DateTime AdmissionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? Address { get; set; }
    public string? BloodGroup { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public Guid? AcademicYearId { get; set; }
    public string? AcademicYearName { get; set; }
}

public sealed class CreateStudentDto
{
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [MaxLength(100)]
    public string? Password { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Section is required")]
    public Guid SectionId { get; set; }

    [Required(ErrorMessage = "Class is required")]
    public Guid ClassRoomId { get; set; }

    public Guid? ParentId { get; set; }

    [Required(ErrorMessage = "Admission date is required")]
    public DateTime AdmissionDate { get; set; }

    [MaxLength(50)]
    public string? AdmissionNumber { get; set; }

    [MaxLength(200)]
    public string? ParentName { get; set; }

    [MaxLength(50)]
    public string? ParentPhone { get; set; }

    [MaxLength(200)]
    public string? ParentEmail { get; set; }

    public bool TransportRequired { get; set; }

    public bool HostelRequired { get; set; }

    [MaxLength(5000)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public string? BloodGroup { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public Guid? AcademicYearId { get; set; }
}

public sealed class UpdateStudentDto
{
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Section is required")]
    public Guid SectionId { get; set; }

    [Required(ErrorMessage = "Class is required")]
    public Guid ClassRoomId { get; set; }

    public Guid? ParentId { get; set; }

    [Required(ErrorMessage = "Admission date is required")]
    public DateTime AdmissionDate { get; set; }

    [MaxLength(200)]
    public string? ParentName { get; set; }

    [MaxLength(50)]
    public string? ParentPhone { get; set; }

    [MaxLength(200)]
    public string? ParentEmail { get; set; }

    public bool TransportRequired { get; set; }

    public bool HostelRequired { get; set; }

    [MaxLength(5000)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public string? BloodGroup { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public string? ProfilePictureUrl { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? AdmissionNumber { get; set; }
}

public sealed class PromoteStudentDto
{
    [Required(ErrorMessage = "Target section is required")]
    public Guid ToSectionId { get; set; }

    [Required(ErrorMessage = "Target academic year is required")]
    public Guid ToAcademicYearId { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public int? NewRollNumber { get; set; }
}

public sealed class TransferStudentDto
{
    [Required(ErrorMessage = "Transfer reason is required")]
    [MaxLength(500)]
    public string TransferReason { get; set; } = string.Empty;

    [Required(ErrorMessage = "Transfer date is required")]
    public DateTime TransferDate { get; set; }

    public string? TransferCertificateNumber { get; set; }

    [Required(ErrorMessage = "Receiving school name is required")]
    [MaxLength(200)]
    public string ReceivingSchoolName { get; set; } = string.Empty;

    public string? Remarks { get; set; }
}

public sealed class StudentDocumentDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public DateTime? UploadedAt { get; set; }
}
