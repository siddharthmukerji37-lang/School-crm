using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolCRM.Domain.Entities.Student;

namespace SchoolCRM.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.AdmissionNumber).IsRequired().HasMaxLength(50);
        builder.Property(s => s.RollNumber).IsRequired().HasMaxLength(50);
        builder.Property(s => s.ParentName).HasMaxLength(200);
        builder.Property(s => s.ParentPhone).HasMaxLength(50);
        builder.Property(s => s.ParentEmail).HasMaxLength(200);
        builder.Property(s => s.Notes).HasColumnType("text");

        builder.HasIndex(s => s.AdmissionNumber).IsUnique();
        builder.HasIndex(s => s.UserId);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Section)
            .WithMany(sec => sec.Students)
            .HasForeignKey(s => s.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.School)
            .WithMany()
            .HasForeignKey(s => s.SchoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Parent)
            .WithMany(p => p.Students)
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class StudentDocumentConfiguration : IEntityTypeConfiguration<StudentDocument>
{
    public void Configure(EntityTypeBuilder<StudentDocument> builder)
    {
        builder.ToTable("StudentDocuments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.DocumentName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.FileUrl).IsRequired().HasMaxLength(500);

        builder.HasOne(d => d.Student)
            .WithMany(s => s.Documents)
            .HasForeignKey(d => d.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StudentHealthRecordConfiguration : IEntityTypeConfiguration<StudentHealthRecord>
{
    public void Configure(EntityTypeBuilder<StudentHealthRecord> builder)
    {
        builder.ToTable("StudentHealthRecords");
        builder.HasKey(h => h.Id);

        builder.HasOne(h => h.Student)
            .WithMany(s => s.HealthRecords)
            .HasForeignKey(h => h.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StudentLeaveConfiguration : IEntityTypeConfiguration<StudentLeave>
{
    public void Configure(EntityTypeBuilder<StudentLeave> builder)
    {
        builder.ToTable("StudentLeaves");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Reason).IsRequired().HasMaxLength(500);

        builder.HasOne(l => l.Student)
            .WithMany()
            .HasForeignKey(l => l.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
