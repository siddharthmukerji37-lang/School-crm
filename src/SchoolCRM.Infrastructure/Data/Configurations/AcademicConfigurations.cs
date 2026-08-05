using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolCRM.Domain.Entities.Attendance;
using SchoolCRM.Domain.Entities.Exam;
using SchoolCRM.Domain.Entities.Fee;
using SchoolCRM.Domain.Entities.Library;

namespace SchoolCRM.Infrastructure.Data.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Domain.Entities.Attendance.Attendance>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Attendance.Attendance> builder)
    {
        builder.ToTable("Attendances");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Student).WithMany(s => s.Attendances).HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(a => a.Teacher).WithMany(t => t.Attendances).HasForeignKey(a => a.TeacherId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(a => a.Employee).WithMany(e => e.Attendances).HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(a => a.School).WithMany().HasForeignKey(a => a.SchoolId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.Date, a.StudentId });
        builder.HasIndex(a => new { a.Date, a.TeacherId });
        builder.HasIndex(a => new { a.Date, a.EmployeeId });
    }
}

public class AttendanceSummaryConfiguration : IEntityTypeConfiguration<AttendanceSummary>
{
    public void Configure(EntityTypeBuilder<AttendanceSummary> builder)
    {
        builder.ToTable("AttendanceSummaries");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Student).WithMany().HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(a => a.Teacher).WithMany().HasForeignKey(a => a.TeacherId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(a => a.Employee).WithMany().HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ExamTypeConfiguration : IEntityTypeConfiguration<ExamType>
{
    public void Configure(EntityTypeBuilder<ExamType> builder)
    {
        builder.ToTable("ExamTypes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
    }
}

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("Exams");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);

        builder.HasOne(e => e.ExamType).WithMany(et => et.Exams).HasForeignKey(e => e.ExamTypeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.School).WithMany().HasForeignKey(e => e.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.ClassRoom).WithMany().HasForeignKey(e => e.ClassRoomId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.AcademicYear).WithMany().HasForeignKey(e => e.AcademicYearId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ExamScheduleConfiguration : IEntityTypeConfiguration<ExamSchedule>
{
    public void Configure(EntityTypeBuilder<ExamSchedule> builder)
    {
        builder.ToTable("ExamSchedules");
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.Exam).WithMany(ex => ex.Schedules).HasForeignKey(e => e.ExamId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Subject).WithMany(s => s.ExamSchedules).HasForeignKey(e => e.SubjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class MarkConfiguration : IEntityTypeConfiguration<Mark>
{
    public void Configure(EntityTypeBuilder<Mark> builder)
    {
        builder.ToTable("Marks");
        builder.HasKey(m => m.Id);

        builder.HasOne(m => m.ExamSchedule).WithMany(es => es.Marks).HasForeignKey(m => m.ExamScheduleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Student).WithMany(s => s.Marks).HasForeignKey(m => m.StudentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Teacher).WithMany(t => t.ExamMarks).HasForeignKey(m => m.TeacherId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => new { m.ExamScheduleId, m.StudentId }).IsUnique();
    }
}

public class GradeSystemConfiguration : IEntityTypeConfiguration<GradeSystem>
{
    public void Configure(EntityTypeBuilder<GradeSystem> builder)
    {
        builder.ToTable("GradeSystems");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
        builder.Property(g => g.Grade).IsRequired().HasMaxLength(10);
    }
}

public class ReportCardConfiguration : IEntityTypeConfiguration<ReportCard>
{
    public void Configure(EntityTypeBuilder<ReportCard> builder)
    {
        builder.ToTable("ReportCards");
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Student).WithMany().HasForeignKey(r => r.StudentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.Exam).WithMany().HasForeignKey(r => r.ExamId).OnDelete(DeleteBehavior.Cascade);
    }
}
