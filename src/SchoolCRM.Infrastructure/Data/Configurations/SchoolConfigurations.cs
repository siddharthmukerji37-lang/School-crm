using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolCRM.Domain.Entities.School;

namespace SchoolCRM.Infrastructure.Data.Configurations;

public class SchoolConfiguration : IEntityTypeConfiguration<Domain.Entities.School.School>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.School.School> builder)
    {
        builder.ToTable("Schools");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Email).HasMaxLength(200);
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.Website).HasMaxLength(200);
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.LogoUrl).HasMaxLength(500);

        builder.HasIndex(s => s.Code).IsUnique();

        builder.HasOne(s => s.CurrentAcademicYear)
            .WithMany()
            .HasForeignKey(s => s.CurrentAcademicYearId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable("AcademicYears");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(50);

        builder.HasOne(a => a.School)
            .WithMany(s => s.AcademicYears)
            .HasForeignKey(a => a.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(50);

        builder.HasOne(b => b.School)
            .WithMany(s => s.Branches)
            .HasForeignKey(b => b.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Code).IsRequired().HasMaxLength(50);

        builder.HasOne(d => d.School)
            .WithMany(s => s.Departments)
            .HasForeignKey(d => d.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ClassRoomConfiguration : IEntityTypeConfiguration<ClassRoom>
{
    public void Configure(EntityTypeBuilder<ClassRoom> builder)
    {
        builder.ToTable("ClassRooms");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);

        builder.HasOne(c => c.School)
            .WithMany(s => s.ClassRooms)
            .HasForeignKey(c => c.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.AcademicYear)
            .WithMany(a => a.ClassRooms)
            .HasForeignKey(c => c.AcademicYearId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Department)
            .WithMany()
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("Sections");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);

        builder.HasOne(s => s.ClassRoom)
            .WithMany(c => c.Sections)
            .HasForeignKey(s => s.ClassRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.ClassTeacher)
            .WithMany()
            .HasForeignKey(s => s.ClassTeacherId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);

        builder.HasOne(s => s.ClassRoom)
            .WithMany(c => c.Subjects)
            .HasForeignKey(s => s.ClassRoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TimetableConfiguration : IEntityTypeConfiguration<Timetable>
{
    public void Configure(EntityTypeBuilder<Timetable> builder)
    {
        builder.ToTable("Timetables");

        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.ClassRoom).WithMany(c => c.Timetables).HasForeignKey(t => t.ClassRoomId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Section).WithMany().HasForeignKey(t => t.SectionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Subject).WithMany(s => s.Timetables).HasForeignKey(t => t.SubjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Teacher).WithMany(te => te.Timetables).HasForeignKey(t => t.TeacherId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PeriodConfiguration : IEntityTypeConfiguration<Period>
{
    public void Configure(EntityTypeBuilder<Period> builder)
    {
        builder.ToTable("Periods");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(50);
    }
}

public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.ToTable("Holidays");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);
    }
}

public class SchoolEventConfiguration : IEntityTypeConfiguration<SchoolEvent>
{
    public void Configure(EntityTypeBuilder<SchoolEvent> builder)
    {
        builder.ToTable("SchoolEvents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
    }
}
