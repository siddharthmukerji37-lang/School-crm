using System.Security.Claims;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.API.Middleware;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? FullName
    {
        get
        {
            var firstName = _httpContextAccessor.HttpContext?.User?.FindFirst("FirstName")?.Value;
            var lastName = _httpContextAccessor.HttpContext?.User?.FindFirst("LastName")?.Value;
            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName)) return null;
            return $"{firstName} {lastName}".Trim();
        }
    }

    public Guid? SchoolId
    {
        get
        {
            var schoolIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("SchoolId")?.Value;
            return string.IsNullOrEmpty(schoolIdClaim) ? null : Guid.Parse(schoolIdClaim);
        }
    }
}
