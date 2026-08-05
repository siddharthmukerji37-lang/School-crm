using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SchoolCRM.Application.DTOs.Auth;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Services;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Shared.Constants;
using Xunit;

namespace SchoolCRM.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AuthService _sut;

    private readonly Guid _userId = Guid.NewGuid();
    private const string JwtSecret = "SuperSecretKeyForTesting12345678901234567890";
    private const string JwtIssuer = "TestIssuer";
    private const string JwtAudience = "TestAudience";

    public AuthServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            new List<IUserValidator<ApplicationUser>>(),
            new List<IPasswordValidator<ApplicationUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            contextAccessor.Object,
            claimsFactory.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<ILogger<SignInManager<ApplicationUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<ApplicationUser>>().Object);

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _configurationMock = new Mock<IConfiguration>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        SetupJwtConfiguration();

        _sut = new AuthService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _unitOfWorkMock.Object,
            _configurationMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenValidCredentials()
    {
        var user = CreateTestUser();
        var loginDto = new LoginDto
        {
            Email = "john.doe@test.com",
            Password = "Password@123"
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(user, loginDto.Password, true))
            .ReturnsAsync(SignInResult.Success);

        _userManagerMock
            .Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { Roles.Student });

        _userManagerMock
            .Setup(u => u.GetClaimsAsync(user))
            .ReturnsAsync(new List<System.Security.Claims.Claim>());

        _userManagerMock
            .Setup(u => u.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.LoginAsync(loginDto, "127.0.0.1");

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.Expiration.Should().BeAfter(DateTime.UtcNow);
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("john.doe@test.com");
        result.User.FirstName.Should().Be("John");
        result.User.LastName.Should().Be("Doe");
        result.User.Roles.Should().Contain(Roles.Student);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenInvalidCredentials()
    {
        var loginDto = new LoginDto
        {
            Email = "nonexistent@test.com",
            Password = "WrongPassword"
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var act = () => _sut.LoginAsync(loginDto, "127.0.0.1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(ApplicationMessages.InvalidCredentials);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenAccountDeactivated()
    {
        var user = CreateTestUser();
        user.IsActive = false;

        var loginDto = new LoginDto
        {
            Email = "john.doe@test.com",
            Password = "Password@123"
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        var act = () => _sut.LoginAsync(loginDto, "127.0.0.1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(ApplicationMessages.AccountDeactivated);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenAccountLocked()
    {
        var user = CreateTestUser();
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(30);

        var loginDto = new LoginDto
        {
            Email = "john.doe@test.com",
            Password = "Password@123"
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        var act = () => _sut.LoginAsync(loginDto, "127.0.0.1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(ApplicationMessages.AccountLocked);
    }

    [Fact]
    public async Task LoginAsync_ThrowsUnauthorized_WhenPasswordIncorrect()
    {
        var user = CreateTestUser();

        var loginDto = new LoginDto
        {
            Email = "john.doe@test.com",
            Password = "WrongPassword"
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        _signInManagerMock
            .Setup(s => s.CheckPasswordSignInAsync(user, loginDto.Password, true))
            .ReturnsAsync(SignInResult.Failed);

        _userManagerMock
            .Setup(u => u.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var act = () => _sut.LoginAsync(loginDto, "127.0.0.1");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage(ApplicationMessages.InvalidCredentials);

        user.FailedLoginAttempts.Should().Be(1);
        _userManagerMock.Verify(u => u.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUserSuccessfully()
    {
        var registerDto = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Password = "Password@123",
            ConfirmPassword = "Password@123",
            Gender = "Male",
            Phone = "1234567890",
            DateOfBirth = new DateTime(2010, 5, 15),
            Role = Roles.Student
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), registerDto.Role))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(u => u.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { registerDto.Role });

        _userManagerMock
            .Setup(u => u.GetClaimsAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<System.Security.Claims.Claim>());

        var result = await _sut.RegisterAsync(registerDto);

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.FirstName.Should().Be("John");
        result.User.LastName.Should().Be("Doe");
        result.User.Roles.Should().Contain(Roles.Student);

        _userManagerMock.Verify(
            u => u.CreateAsync(It.IsAny<ApplicationUser>(), registerDto.Password), Times.Once);
        _userManagerMock.Verify(
            u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), registerDto.Role), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenEmailAlreadyExists()
    {
        var existingUser = CreateTestUser();

        var registerDto = new RegisterDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Password = "Password@123",
            ConfirmPassword = "Password@123",
            Gender = "Male",
            Role = Roles.Student
        };

        _userManagerMock
            .Setup(u => u.FindByEmailAsync(registerDto.Email))
            .ReturnsAsync(existingUser);

        var act = () => _sut.RegisterAsync(registerDto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(ApplicationMessages.DuplicateRecord);
    }

    private void SetupJwtConfiguration()
    {
        _configurationMock.Setup(c => c["Jwt:Secret"]).Returns(JwtSecret);
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns(JwtIssuer);
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns(JwtAudience);
        _configurationMock.Setup(c => c["Jwt:AccessTokenExpirationMinutes"]).Returns("60");
        _configurationMock.Setup(c => c["Jwt:RefreshTokenExpirationDays"]).Returns("7");
    }

    private ApplicationUser CreateTestUser()
    {
        return new ApplicationUser
        {
            Id = _userId,
            UserName = "john.doe@test.com",
            Email = "john.doe@test.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "1234567890",
            Gender = Gender.Male,
            IsActive = true,
            IsDeleted = false,
            FailedLoginAttempts = 0,
            CreatedAt = DateTime.UtcNow
        };
    }
}
