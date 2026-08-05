using Microsoft.AspNetCore.Identity;
using SchoolCRM.Domain.Common;
using SchoolCRM.Domain.Enums;

namespace SchoolCRM.Domain.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public Gender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? PhoneNumber2 { get; set; }
    public string? Nationality { get; set; }
    public string? Religion { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    public ICollection<ApplicationUserClaim> UserClaims { get; set; } = new List<ApplicationUserClaim>();
    public ICollection<ApplicationUserLogin> UserLogins { get; set; } = new List<ApplicationUserLogin>();
    public ICollection<ApplicationUserToken> UserTokens { get; set; } = new List<ApplicationUserToken>();
}

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public RoleType RoleType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    public ICollection<ApplicationRoleClaim> RoleClaims { get; set; } = new List<ApplicationRoleClaim>();
}

public class ApplicationUserRole : IdentityUserRole<Guid>
{
    public ApplicationUser User { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;
}

public class ApplicationUserClaim : IdentityUserClaim<Guid>
{
    public ApplicationUser User { get; set; } = null!;
}

public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
{
    public ApplicationRole Role { get; set; } = null!;
}

public class ApplicationUserLogin : IdentityUserLogin<Guid>
{
    public ApplicationUser User { get; set; } = null!;
}

public class ApplicationUserToken : IdentityUserToken<Guid>
{
    public ApplicationUser User { get; set; } = null!;
}
