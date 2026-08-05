using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SchoolCRM.Domain.Entities.School;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;
using SchoolCRM.Infrastructure.Repositories;
using SchoolCRM.Tests.Fixtures;
using Xunit;

namespace SchoolCRM.Tests.Unit.Repositories;

public class GenericRepositoryTests : IDisposable
{
    private readonly TestDatabaseFixture _fixture;
    private readonly ApplicationDbContext _context;
    private readonly GenericRepository<Domain.Entities.Student.Student> _repository;
    private readonly string _dbName;

    public GenericRepositoryTests()
    {
        _fixture = new TestDatabaseFixture();
        _dbName = $"GenericRepoTest_{Guid.NewGuid()}";
        _context = _fixture.CreateSeededContext(_dbName);
        _repository = new GenericRepository<Domain.Entities.Student.Student>(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntity_WhenExists()
    {
        var result = await _repository.GetByIdAsync(_fixture.StudentId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(_fixture.StudentId);
        result.AdmissionNumber.Should().Be("ADM-2025-0001");
        result.Status.Should().Be(StudentStatus.Active);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _repository.GetByIdAsync(nonExistentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsEntityCorrectly()
    {
        var newStudent = new Domain.Entities.Student.Student
        {
            AdmissionNumber = "ADM-2025-0002",
            RollNumber = "2",
            UserId = Guid.NewGuid(),
            SectionId = _fixture.SectionId,
            SchoolId = _fixture.SchoolId,
            AdmissionDate = DateTime.UtcNow,
            Status = StudentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var added = await _repository.AddAsync(newStudent);

        added.Should().NotBeNull();
        added.Id.Should().NotBeEmpty();
        added.AdmissionNumber.Should().Be("ADM-2025-0002");

        await _context.SaveChangesAsync();

        var fromDb = await _repository.GetByIdAsync(added.Id);
        fromDb.Should().NotBeNull();
        fromDb!.AdmissionNumber.Should().Be("ADM-2025-0002");
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesEntity()
    {
        var student = await _repository.GetByIdAsync(_fixture.StudentId);
        student.Should().NotBeNull();

        await _repository.DeleteAsync(student!);
        await _context.SaveChangesAsync();

        var softDeleted = await _context.Students
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == _fixture.StudentId);

        softDeleted.Should().NotBeNull();
        softDeleted!.IsDeleted.Should().BeTrue();
        softDeleted.DeletedAt.Should().NotBeNull();

        var queried = await _repository.GetByIdAsync(_fixture.StudentId);
        queried.Should().BeNull();
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        var countBefore = await _repository.CountAsync();

        countBefore.Should().Be(1);

        var newStudent = new Domain.Entities.Student.Student
        {
            AdmissionNumber = "ADM-2025-0099",
            RollNumber = "99",
            UserId = Guid.NewGuid(),
            SectionId = _fixture.SectionId,
            SchoolId = _fixture.SchoolId,
            AdmissionDate = DateTime.UtcNow,
            Status = StudentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(newStudent);
        await _context.SaveChangesAsync();

        var countAfter = await _repository.CountAsync();

        countAfter.Should().Be(2);
    }

    [Fact]
    public async Task AnyAsync_ReturnsTrue_WhenExists()
    {
        var result = await _repository.AnyAsync(s => s.Id == _fixture.StudentId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_ReturnsFalse_WhenNotExists()
    {
        var result = await _repository.AnyAsync(s => s.Id == Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPage()
    {
        var students = new List<Domain.Entities.Student.Student>();
        for (int i = 0; i < 15; i++)
        {
            students.Add(new Domain.Entities.Student.Student
            {
                AdmissionNumber = $"ADM-2025-{(i + 100):D4}",
                RollNumber = (i + 100).ToString(),
                UserId = Guid.NewGuid(),
                SectionId = _fixture.SectionId,
                SchoolId = _fixture.SchoolId,
                AdmissionDate = DateTime.UtcNow,
                Status = StudentStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
        }
        _context.Students.AddRange(students);
        await _context.SaveChangesAsync();

        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber: 2,
            pageSize: 5);

        totalCount.Should().Be(16);
        items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPagedAsync_WithFilter_ReturnsFilteredResults()
    {
        var students = new List<Domain.Entities.Student.Student>
        {
            new()
            {
                AdmissionNumber = "ADM-2025-0010",
                RollNumber = "10",
                UserId = Guid.NewGuid(),
                SectionId = _fixture.SectionId,
                SchoolId = _fixture.SchoolId,
                AdmissionDate = DateTime.UtcNow,
                Status = StudentStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                AdmissionNumber = "ADM-2025-0011",
                RollNumber = "11",
                UserId = Guid.NewGuid(),
                SectionId = _fixture.SectionId,
                SchoolId = _fixture.SchoolId,
                AdmissionDate = DateTime.UtcNow,
                Status = StudentStatus.Inactive,
                CreatedAt = DateTime.UtcNow
            }
        };
        _context.Students.AddRange(students);
        await _context.SaveChangesAsync();

        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 10,
            filter: s => s.Status == StudentStatus.Inactive);

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items.First().AdmissionNumber.Should().Be("ADM-2025-0011");
    }

    [Fact]
    public async Task GetPagedAsync_WithOrderBy_ReturnsOrderedResults()
    {
        var students = new List<Domain.Entities.Student.Student>
        {
            new()
            {
                AdmissionNumber = "ADM-2025-0030",
                RollNumber = "30",
                UserId = Guid.NewGuid(),
                SectionId = _fixture.SectionId,
                SchoolId = _fixture.SchoolId,
                AdmissionDate = DateTime.UtcNow,
                Status = StudentStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new()
            {
                AdmissionNumber = "ADM-2025-0020",
                RollNumber = "20",
                UserId = Guid.NewGuid(),
                SectionId = _fixture.SectionId,
                SchoolId = _fixture.SchoolId,
                AdmissionDate = DateTime.UtcNow,
                Status = StudentStatus.Active,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };
        _context.Students.AddRange(students);
        await _context.SaveChangesAsync();

        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 10,
            orderBy: q => q.OrderBy(s => s.AdmissionNumber));

        totalCount.Should().Be(3);
        items.Should().HaveCount(3);
        items.First().AdmissionNumber.Should().Be("ADM-2025-0001");
        items.Last().AdmissionNumber.Should().Be("ADM-2025-0030");
    }

    public void Dispose()
    {
        _context.Dispose();
        _fixture.Dispose();
    }
}
