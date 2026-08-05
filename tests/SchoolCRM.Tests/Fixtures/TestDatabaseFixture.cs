using Microsoft.EntityFrameworkCore;
using SchoolCRM.Domain.Entities.Identity;
using SchoolCRM.Domain.Entities.School;
using SchoolCRM.Domain.Entities.Student;
using SchoolCRM.Domain.Enums;
using SchoolCRM.Infrastructure.Data;

namespace SchoolCRM.Tests.Fixtures;

public sealed class TestDatabaseFixture : IDisposable
{
    private bool _disposed;

    public Guid SchoolId { get; } = Guid.NewGuid();
    public Guid AcademicYearId { get; } = Guid.NewGuid();
    public Guid ClassRoomId { get; } = Guid.NewGuid();
    public Guid SectionId { get; } = Guid.NewGuid();
    public Guid StudentUserId { get; } = Guid.NewGuid();
    public Guid StudentId { get; } = Guid.NewGuid();

    public ApplicationDbContext CreateContext(string? databaseName = null)
    {
        var name = databaseName ?? $"SchoolCRM_Test_{Guid.NewGuid()}";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: name)
            .Options;

        return new ApplicationDbContext(options);
    }

    public ApplicationDbContext CreateSeededContext(string? databaseName = null)
    {
        var context = CreateContext(databaseName);
        SeedData(context);
        return context;
    }

    public void SeedData(ApplicationDbContext context)
    {
        var school = new Domain.Entities.School.School
        {
            Id = SchoolId,
            Name = "Test School",
            Code = "TS001",
            Email = "admin@testschool.com",
            Phone = "1234567890",
            Address = "123 Test Street",
            City = "Testville",
            State = "Testland",
            Country = "Testland",
            CreatedAt = DateTime.UtcNow
        };
        context.Schools.Add(school);

        var academicYear = new AcademicYear
        {
            Id = AcademicYearId,
            Name = "2025-2026",
            StartDate = new DateTime(2025, 4, 1),
            EndDate = new DateTime(2026, 3, 31),
            IsCurrent = true,
            SchoolId = SchoolId,
            CreatedAt = DateTime.UtcNow
        };
        context.AcademicYears.Add(academicYear);

        var classRoom = new ClassRoom
        {
            Id = ClassRoomId,
            Name = "Class 10",
            Code = "C10",
            Capacity = 40,
            SchoolId = SchoolId,
            AcademicYearId = AcademicYearId,
            CreatedAt = DateTime.UtcNow
        };
        context.ClassRooms.Add(classRoom);

        var section = new Section
        {
            Id = SectionId,
            Name = "Section A",
            Code = "S-A",
            Capacity = 40,
            ClassRoomId = ClassRoomId,
            CreatedAt = DateTime.UtcNow
        };
        context.Sections.Add(section);

        var studentUser = new ApplicationUser
        {
            Id = StudentUserId,
            UserName = "john.doe@test.com",
            Email = "john.doe@test.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "9876543210",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(2010, 5, 15),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(studentUser);

        var student = new Domain.Entities.Student.Student
        {
            Id = StudentId,
            AdmissionNumber = "ADM-2025-0001",
            RollNumber = "1",
            UserId = StudentUserId,
            SectionId = SectionId,
            SchoolId = SchoolId,
            AdmissionDate = new DateTime(2025, 4, 1),
            Status = StudentStatus.Active,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Students.Add(student);

        context.SaveChanges();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
