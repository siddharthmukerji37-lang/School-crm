using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SchoolCRM.Application.DTOs.Auth;
using SchoolCRM.Application.DTOs.Employee;
using SchoolCRM.Application.DTOs.Student;
using SchoolCRM.Application.DTOs.Teacher;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;

namespace SchoolCRM.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _currentUserService = currentUserService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || user.IsDeleted)
            throw new UnauthorizedAccessException(ApplicationMessages.InvalidCredentials);

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedAccessException(ApplicationMessages.AccountLocked);

        if (!user.IsActive)
            throw new UnauthorizedAccessException(ApplicationMessages.AccountDeactivated);

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            user.FailedLoginAttempts++;
            await _userManager.UpdateAsync(user);
            throw new UnauthorizedAccessException(ApplicationMessages.InvalidCredentials);
        }

        user.FailedLoginAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        await MarkTeacherAttendanceOnLoginAsync(user);

        var token = await GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60")),
            User = new AuthUserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                Permissions = permissions
            }
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            throw new InvalidOperationException(ApplicationMessages.DuplicateRecord);

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Gender = Enum.Parse<Domain.Enums.Gender>(dto.Gender),
            DateOfBirth = dto.DateOfBirth,
            PhoneNumber = dto.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new InvalidOperationException(string.Join("; ", errors));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!roleResult.Succeeded)
        {
            var errors = roleResult.Errors.Select(e => e.Description).ToList();
            throw new InvalidOperationException(string.Join("; ", errors));
        }

        var token = await GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60")),
            User = new AuthUserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = new List<string> { dto.Role },
                Permissions = new List<string>()
            }
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string token, string ipAddress)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        if (principal is null)
            throw new SecurityTokenException(ApplicationMessages.TokenRefreshFailed);

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new SecurityTokenException(ApplicationMessages.TokenRefreshFailed);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted)
            throw new SecurityTokenException(ApplicationMessages.TokenRefreshFailed);

        if (user.RefreshToken != token || !user.RefreshTokenExpiry.HasValue || user.RefreshTokenExpiry < DateTime.UtcNow)
            throw new SecurityTokenException(ApplicationMessages.TokenRefreshFailed);

        var newToken = await GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            double.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7"));
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var permissions = claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

        return new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60")),
            User = new AuthUserDto
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                Permissions = permissions
            }
        };
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        if (principal is null)
            return false;

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return true;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _userManager.UpdateAsync(user);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            throw new InvalidOperationException(ApplicationMessages.NotFound);

        if (user.PasswordResetToken != dto.Token || !user.PasswordResetTokenExpiry.HasValue || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            throw new SecurityTokenException(ApplicationMessages.TokenRefreshFailed);

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new InvalidOperationException(string.Join("; ", errors));
        }

        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return true;
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException(ApplicationMessages.NotFound);

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            throw new InvalidOperationException(string.Join("; ", errors));
        }

        return true;
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.PhoneNumber,
            Gender = user.Gender.ToString(),
            DateOfBirth = user.DateOfBirth,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Roles = roles.ToList()
        };
    }

    public async Task<MyProfileDto?> GetMyProfileAsync(string userId)
    {
        var profile = await GetProfileAsync(userId);
        if (profile is null)
            return null;

        var myProfile = new MyProfileDto { User = profile };

        if (!Guid.TryParse(userId, out var userGuid))
            return myProfile;

        var student = await _unitOfWork.Students.GetStudentByUserIdAsync(userGuid);
        if (student is not null && !student.IsDeleted)
            myProfile.Student = MapStudentDto(student);

        var teacher = await _unitOfWork.Teachers.GetTeacherByUserIdAsync(userGuid);
        if (teacher is not null && !teacher.IsDeleted)
            myProfile.Teacher = MapTeacherDto(teacher);

        var employee = await _unitOfWork.Employees.GetEmployeeByUserIdAsync(userGuid);
        if (employee is not null && !employee.IsDeleted)
            myProfile.Employee = MapEmployeeDto(employee);

        return myProfile;
    }

    private static StudentDto MapStudentDto(Domain.Entities.Student.Student student)
    {
        return new StudentDto
        {
            Id = student.Id,
            AdmissionNumber = student.AdmissionNumber,
            RollNumber = int.TryParse(student.RollNumber, out var rn) ? rn : 0,
            FirstName = student.User.FirstName,
            LastName = student.User.LastName,
            Email = student.User.Email ?? string.Empty,
            Phone = student.User.PhoneNumber,
            Gender = student.User.Gender.ToString(),
            DateOfBirth = student.User.DateOfBirth ?? DateTime.MinValue,
            SectionId = student.SectionId,
            SectionName = student.Section?.Name ?? string.Empty,
            ClassRoomId = student.Section?.ClassRoomId ?? Guid.Empty,
            ClassName = student.Section?.ClassRoom?.Name ?? string.Empty,
            ParentId = student.ParentId,
            ParentName = !string.IsNullOrWhiteSpace(student.ParentName)
                ? student.ParentName
                : student.Parent?.User is not null
                    ? $"{student.Parent.User.FirstName} {student.Parent.User.LastName}"
                    : null,
            ParentPhone = student.ParentPhone,
            ParentEmail = student.ParentEmail,
            TransportRequired = student.TransportRequired,
            HostelRequired = student.HostelRequired,
            Notes = student.Notes,
            AdmissionDate = student.AdmissionDate,
            Status = student.Status.ToString(),
            ProfilePictureUrl = student.User.ProfilePictureUrl,
            Address = student.User.Address,
            BloodGroup = student.User.BloodGroup.ToDisplayString()
        };
    }

    private static TeacherDto MapTeacherDto(Domain.Entities.Teacher.Teacher teacher)
    {
        return new TeacherDto
        {
            Id = teacher.Id,
            EmployeeId = teacher.EmployeeCode,
            FirstName = teacher.User.FirstName,
            LastName = teacher.User.LastName,
            Email = teacher.User.Email ?? string.Empty,
            Phone = teacher.User.PhoneNumber,
            Gender = teacher.User.Gender.ToString(),
            DateOfBirth = teacher.User.DateOfBirth ?? DateTime.MinValue,
            JoiningDate = teacher.JoiningDate,
            DepartmentId = teacher.DepartmentId,
            DepartmentName = teacher.DepartmentName ?? teacher.Department?.Name ?? string.Empty,
            Designation = teacher.EmploymentType,
            Qualification = teacher.Qualification,
            Salary = teacher.BasicSalary,
            Address = teacher.User.Address,
            BloodGroup = teacher.User.BloodGroup?.ToString(),
            ProfilePictureUrl = teacher.User.ProfilePictureUrl,
            Status = teacher.Status.ToString(),
            Specialization = teacher.Specialization,
            Experience = teacher.ExperienceYears
        };
    }

    private static EmployeeDto MapEmployeeDto(Domain.Entities.Employee.Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.User.FirstName,
            LastName = employee.User.LastName,
            Email = employee.User.Email ?? string.Empty,
            Phone = employee.User.PhoneNumber,
            Gender = employee.User.Gender.ToString(),
            DateOfBirth = employee.User.DateOfBirth ?? DateTime.MinValue,
            JoiningDate = employee.JoiningDate,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name,
            Designation = employee.Designation?.Name,
            EmployeeType = employee.EmploymentType,
            Salary = employee.BasicSalary,
            Address = employee.User.Address,
            BloodGroup = employee.User.BloodGroup?.ToString(),
            ProfilePictureUrl = employee.User.ProfilePictureUrl,
            Status = employee.Status.ToString()
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new InvalidOperationException(ApplicationMessages.NotFound);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.Phone;
        user.DateOfBirth = dto.DateOfBirth;
        user.Address = dto.Address;
        user.City = dto.City;
        user.State = dto.State;
        user.Country = dto.Country;
        user.PostalCode = dto.PostalCode;
        user.ProfilePictureUrl = dto.ProfilePictureUrl;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(dto.Gender) && Enum.TryParse<Domain.Enums.Gender>(dto.Gender, out var gender))
            user.Gender = gender;

        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.PhoneNumber,
            Gender = user.Gender.ToString(),
            DateOfBirth = user.DateOfBirth,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Roles = roles.ToList()
        };
    }

    private async Task MarkTeacherAttendanceOnLoginAsync(ApplicationUser user)
    {
        try
        {
            var teacher = await _unitOfWork.Teachers.GetTeacherByUserIdAsync(user.Id);
            if (teacher is null || teacher.IsDeleted)
                return;

            var schoolId = teacher.SchoolId;
            if (schoolId is null || schoolId == Guid.Empty)
            {
                var schools = await _unitOfWork.Schools.GetAllAsync();
                schoolId = schools.FirstOrDefault()?.Id;
            }
            if (schoolId is null || schoolId == Guid.Empty)
                return;

            var today = DateTime.Now.Date;
            var existing = (await _unitOfWork.Attendances.FindAsync(a =>
                a.Date.Date == today && a.TeacherId == teacher.Id && !a.IsDeleted)).FirstOrDefault();

            if (existing is not null)
            {
                existing.Status = AttendanceStatus.Present;
                existing.CheckInTime ??= DateTime.Now.TimeOfDay;
                existing.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Attendances.UpdateAsync(existing);
            }
            else
            {
                await _unitOfWork.Attendances.AddAsync(new Domain.Entities.Attendance.Attendance
                {
                    Date = today,
                    Status = AttendanceStatus.Present,
                    CheckInTime = DateTime.Now.TimeOfDay,
                    TeacherId = teacher.Id,
                    SchoolId = schoolId.Value,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // Auto-marking must never block a successful login.
        }
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var userClaims = await _userManager.GetClaimsAsync(user);
        foreach (var claim in userClaims)
            claims.Add(claim);

        var secret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var secret = _configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(secret))
            return null;

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = false,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
