using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Parent;

public class Parent : BaseEntity
{
    public string ParentCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? Occupation { get; set; }
    public decimal? AnnualIncome { get; set; }
    public string? Relationship { get; set; }
    public bool IsEmergencyContact { get; set; }
    public string? AlternatePhone { get; set; }

    public Identity.ApplicationUser User { get; set; } = null!;
    public ICollection<Student.Student> Students { get; set; } = new List<Student.Student>();
    public ICollection<GuardianDetail> GuardianDetails { get; set; } = new List<GuardianDetail>();
}

public class GuardianDetail : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Occupation { get; set; }
    public bool IsPrimaryGuardian { get; set; }
    public bool IsEmergencyContact { get; set; }
    public Guid ParentId { get; set; }

    public Parent Parent { get; set; } = null!;
}
