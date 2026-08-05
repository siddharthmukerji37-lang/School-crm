using System.ComponentModel.DataAnnotations;

namespace SchoolCRM.Application.DTOs.Parent;

public sealed class ParentDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AlternativePhone { get; set; }
    public string? Occupation { get; set; }
    public string? Relationship { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public List<StudentChildDto> Children { get; set; } = new();
    public bool IsActive { get; set; }
}

public sealed class StudentChildDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string AdmissionNumber { get; set; } = string.Empty;
}

public sealed class CreateParentDto
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

    public string? Password { get; set; }

    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string Phone { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid alternative phone number")]
    public string? AlternativePhone { get; set; }

    [MaxLength(100)]
    public string? Occupation { get; set; }

    [Required(ErrorMessage = "Relationship is required")]
    public string Relationship { get; set; } = string.Empty;

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

    public string? ProfilePictureUrl { get; set; }

    public List<Guid>? ChildrenStudentIds { get; set; }
}

public sealed class UpdateParentDto
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

    [Phone(ErrorMessage = "Invalid alternative phone number")]
    public string? AlternativePhone { get; set; }

    [MaxLength(100)]
    public string? Occupation { get; set; }

    [Required(ErrorMessage = "Relationship is required")]
    public string Relationship { get; set; } = string.Empty;

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

    public string? ProfilePictureUrl { get; set; }

    public List<Guid>? ChildrenStudentIds { get; set; }

    public bool IsActive { get; set; } = true;
}
