using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SchoolCRM.Application.DTOs.Student;
using SchoolCRM.Application.Interfaces.Services;
using SchoolCRM.API.Controllers;
using SchoolCRM.Shared.Constants;
using SchoolCRM.Shared.Models;
using Xunit;

namespace SchoolCRM.Tests.Unit.Controllers;

public class StudentsControllerTests
{
    private readonly Mock<IStudentService> _studentServiceMock;
    private readonly StudentsController _sut;

    private readonly Guid _studentId = Guid.NewGuid();

    public StudentsControllerTests()
    {
        _studentServiceMock = new Mock<IStudentService>();
        _sut = new StudentsController(_studentServiceMock.Object);
    }

    [Fact]
    public async Task GetStudents_ReturnsOk_WithStudents()
    {
        var pagedResult = new PagedResult<StudentDto>
        {
            Items = new List<StudentDto>
            {
                new()
                {
                    Id = _studentId,
                    AdmissionNumber = "ADM-2025-0001",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@test.com",
                    Status = "Active"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    AdmissionNumber = "ADM-2025-0002",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane@test.com",
                    Status = "Active"
                }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20
        };

        var apiResponse = ApiResponse<PagedResult<StudentDto>>.SuccessResponse(pagedResult);

        _studentServiceMock
            .Setup(s => s.GetStudentsAsync(
                It.IsAny<PaginationQuery>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(apiResponse);

        var result = await _sut.GetStudentsAsync();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PagedResult<StudentDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Items.Should().HaveCount(2);
        response.Data.Items.First().FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetStudents_ReturnsOk_WithQueryParameters()
    {
        var pagedResult = new PagedResult<StudentDto>
        {
            Items = new List<StudentDto>(),
            TotalCount = 0,
            PageNumber = 2,
            PageSize = 5
        };

        var apiResponse = ApiResponse<PagedResult<StudentDto>>.SuccessResponse(pagedResult);

        _studentServiceMock
            .Setup(s => s.GetStudentsAsync(
                It.IsAny<PaginationQuery>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(apiResponse);

        var result = await _sut.GetStudentsAsync(
            pageNumber: 2,
            pageSize: 5,
            searchTerm: "test",
            sortColumn: "name",
            sortOrder: "asc");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<PagedResult<StudentDto>>>().Subject;
        response.Data!.PageNumber.Should().Be(2);
        response.Data.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetStudentById_ReturnsOk_WhenExists()
    {
        var studentDto = new StudentDto
        {
            Id = _studentId,
            AdmissionNumber = "ADM-2025-0001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Gender = "Male",
            Status = "Active"
        };

        var apiResponse = ApiResponse<StudentDto>.SuccessResponse(studentDto);

        _studentServiceMock
            .Setup(s => s.GetStudentByIdAsync(_studentId))
            .ReturnsAsync(apiResponse);

        var result = await _sut.GetStudentByIdAsync(_studentId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ApiResponse<StudentDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(_studentId);
        response.Data.AdmissionNumber.Should().Be("ADM-2025-0001");
    }

    [Fact]
    public async Task GetStudentById_ReturnsNotFound_WhenNotExists()
    {
        var apiResponse = ApiResponse<StudentDto>.NotFoundResponse("Resource not found.");

        _studentServiceMock
            .Setup(s => s.GetStudentByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(apiResponse);

        var result = await _sut.GetStudentByIdAsync(Guid.NewGuid());

        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        var response = notFoundResult.Value.Should().BeOfType<ApiResponse<StudentDto>>().Subject;
        response.Success.Should().BeFalse();
        response.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateStudent_ReturnsCreatedAtAction()
    {
        var dto = new CreateStudentDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Gender = "Male",
            DateOfBirth = new DateTime(2010, 5, 15),
            SectionId = Guid.NewGuid(),
            ClassRoomId = Guid.NewGuid(),
            AdmissionDate = DateTime.UtcNow
        };

        var studentDto = new StudentDto
        {
            Id = _studentId,
            AdmissionNumber = "ADM-2025-0001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Status = "Active"
        };

        var apiResponse = ApiResponse<StudentDto>.SuccessResponse(studentDto, ApplicationMessages.CreateSuccess);

        _studentServiceMock
            .Setup(s => s.CreateStudentAsync(It.IsAny<CreateStudentDto>()))
            .ReturnsAsync(apiResponse);

        var result = await _sut.CreateStudentAsync(dto);

        var createdResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var response = createdResult.Value.Should().BeOfType<ApiResponse<StudentDto>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.FirstName.Should().Be("John");
        response.Data.AdmissionNumber.Should().Be("ADM-2025-0001");
    }

    [Fact]
    public async Task CreateStudent_ReturnsBadRequest_WhenFails()
    {
        var dto = new CreateStudentDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            Gender = "Male",
            DateOfBirth = new DateTime(2010, 5, 15),
            SectionId = Guid.NewGuid(),
            ClassRoomId = Guid.NewGuid(),
            AdmissionDate = DateTime.UtcNow
        };

        var apiResponse = ApiResponse<StudentDto>.FailResponse(ApplicationMessages.DuplicateRecord);

        _studentServiceMock
            .Setup(s => s.CreateStudentAsync(It.IsAny<CreateStudentDto>()))
            .ReturnsAsync(apiResponse);

        var result = await _sut.CreateStudentAsync(dto);

        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = badRequestResult.Value.Should().BeOfType<ApiResponse<StudentDto>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Be(ApplicationMessages.DuplicateRecord);
    }
}
