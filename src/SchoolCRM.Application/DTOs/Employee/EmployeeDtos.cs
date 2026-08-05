using System.ComponentModel.DataAnnotations;

namespace SchoolCRM.Application.DTOs.Employee;

public sealed class EmployeeDto
{
    public Guid Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime JoiningDate { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? Designation { get; set; }
    public string? EmployeeType { get; set; }
    public decimal? Salary { get; set; }
    public string? Address { get; set; }
    public string? BloodGroup { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

public sealed class CreateEmployeeDto
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

    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Joining date is required")]
    public DateTime JoiningDate { get; set; }

    public Guid? DepartmentId { get; set; }

    [MaxLength(100)]
    public string? Designation { get; set; }

    [Required(ErrorMessage = "Employee type is required")]
    public string EmployeeType { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Salary must be positive")]
    public decimal? Salary { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public string? BloodGroup { get; set; }

    public string? ProfilePictureUrl { get; set; }

    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }

    [Phone(ErrorMessage = "Invalid emergency contact phone")]
    public string? EmergencyContactPhone { get; set; }

    public string? Password { get; set; }
}

public sealed class UpdateEmployeeDto
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

    public Guid? DepartmentId { get; set; }

    [MaxLength(100)]
    public string? Designation { get; set; }

    public string? EmployeeType { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Salary must be positive")]
    public decimal? Salary { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public string? BloodGroup { get; set; }

    public string? ProfilePictureUrl { get; set; }

    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }

    [Phone(ErrorMessage = "Invalid emergency contact phone")]
    public string? EmergencyContactPhone { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;
}
