using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SchoolCRM.Application.DTOs.Student;
using SchoolCRM.Application.Interfaces.Repositories;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Services;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using Xunit;

namespace SchoolCRM.Tests.Unit.Services;

public class StudentServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IStudentRepository> _studentRepositoryMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly StudentService _sut;

    private readonly Guid _schoolId = Guid.NewGuid();
    private readonly Guid _sectionId = Guid.NewGuid();
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _studentUserId = Guid.NewGuid();

    public StudentServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _studentRepositoryMock = new Mock<IStudentRepository>();

        _unitOfWorkMock.Setup(u => u.Students).Returns(_studentRepositoryMock.Object);

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

        _sut = new StudentService(_unitOfWorkMock.Object, _userManagerMock.Object);
    }

    [Fact]
    public async Task GetStudentsAsync_ReturnsPagedResults()
    {
        var students = new List<Domain.Entities.Student.Student>
        {
            CreateTestStudent("ADM-001", "John Doe", "john@test.com"),
            CreateTestStudent("ADM-002", "Jane Smith", "jane@test.com")
        };

        _studentRepositoryMock
            .Setup(r => r.GetPagedStudentsAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<Guid?>(), It.IsAny<Guid?>(),
                It.IsAny<Guid?>(), It.IsAny<string?>()))
            .ReturnsAsync((students, 2));

        var query = new PaginationQuery(1, 10);

        var result = await _sut.GetStudentsAsync(query, null, null, null, null);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
        result.Data.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetStudentByIdAsync_ReturnsStudent_WhenExists()
    {
        var student = CreateTestStudent("ADM-001", "John Doe", "john@test.com");

        _studentRepositoryMock
            .Setup(r => r.GetStudentWithDetailsAsync(_studentId))
            .ReturnsAsync(student);

        var result = await _sut.GetStudentByIdAsync(_studentId);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(_studentId);
        result.Data.AdmissionNumber.Should().Be("ADM-001");
    }

    [Fact]
    public async Task GetStudentByIdAsync_ReturnsNotFound_WhenNotExists()
    {
        _studentRepositoryMock
            .Setup(r => r.GetStudentWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Entities.Student.Student?)null);

        var result = await _sut.GetStudentByIdAsync(Guid.NewGuid());

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task CreateStudentAsync_CreatesStudentSuccessfully()
    {
        var dto = new CreateStudentDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Phone = "1234567890",
            Gender = "Male",
            DateOfBirth = new DateTime(2010, 5, 15),
            SectionId = _sectionId,
            ClassRoomId = _schoolId,
            AdmissionDate = DateTime.UtcNow,
            Address = "123 Test Street"
        };

        var createdStudent = CreateTestStudent("ADM-2025-0001", "John Doe", "john.doe@test.com");

        _studentRepositoryMock
            .Setup(r => r.GetStudentByAdmissionNumberAsync(It.IsAny<string>()))
            .ReturnsAsync((Domain.Entities.Student.Student?)null);

        _userManagerMock
            .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _studentRepositoryMock
            .Setup(r => r.GenerateNextAdmissionNumberAsync(It.IsAny<Guid>()))
            .ReturnsAsync("ADM-2025-0001");

        _studentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Domain.Entities.Student.Student>()))
            .ReturnsAsync(createdStudent);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _studentRepositoryMock
            .Setup(r => r.GetStudentWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(createdStudent);

        var result = await _sut.CreateStudentAsync(dto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AdmissionNumber.Should().Be("ADM-2025-0001");
        _studentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Student.Student>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteStudentAsync_DeletesStudent()
    {
        var student = CreateTestStudent("ADM-001", "John Doe", "john@test.com");

        _studentRepositoryMock
            .Setup(r => r.GetByIdAsync(_studentId))
            .ReturnsAsync(student);

        _studentRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.Student.Student>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.DeleteStudentAsync(_studentId);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be(ApplicationMessages.DeleteSuccess);
        student.IsDeleted.Should().BeTrue();
        student.DeletedAt.Should().NotBeNull();
        _studentRepositoryMock.Verify(r => r.UpdateAsync(student), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteStudentAsync_ReturnsNotFound_WhenNotExists()
    {
        _studentRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Domain.Entities.Student.Student?)null);

        var result = await _sut.DeleteStudentAsync(Guid.NewGuid());

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Be(ApplicationMessages.NotFound);
    }

    private Domain.Entities.Student.Student CreateTestStudent(
        string admissionNumber, string fullName, string email)
    {
        var firstName = fullName.Split(' ')[0];
        var lastName = fullName.Split(' ')[1];

        return new Domain.Entities.Student.Student
        {
            Id = _studentId,
            AdmissionNumber = admissionNumber,
            RollNumber = "1",
            UserId = _studentUserId,
            SectionId = _sectionId,
            SchoolId = _schoolId,
            AdmissionDate = new DateTime(2025, 4, 1),
            Status = StudentStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            User = new ApplicationUser
            {
                Id = _studentUserId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = "1234567890",
                Gender = Gender.Male,
                IsActive = true,
                IsDeleted = false
            },
            Section = new Domain.Entities.School.Section
            {
                Id = _sectionId,
                Name = "Section A",
                Code = "S-A",
                ClassRoomId = _schoolId,
                ClassRoom = new Domain.Entities.School.ClassRoom
                {
                    Id = _schoolId,
                    Name = "Class 10",
                    Code = "C10",
                    SchoolId = _schoolId,
                    AcademicYearId = Guid.NewGuid()
                }
            },
            School = new Domain.Entities.School.School
            {
                Id = _schoolId,
                Name = "Test School",
                Code = "TS001"
            }
        };
    }
}
