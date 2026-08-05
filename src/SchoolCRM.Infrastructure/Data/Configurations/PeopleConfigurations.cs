using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolCRM.Domain.Entities.Teacher;
using SchoolCRM.Domain.Entities.Parent;
using SchoolCRM.Domain.Entities.Employee;
using TeacherEntity = SchoolCRM.Domain.Entities.Teacher.Teacher;
using EmployeeEntity = SchoolCRM.Domain.Entities.Employee.Employee;

namespace SchoolCRM.Infrastructure.Data.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<TeacherEntity>
{
    public void Configure(EntityTypeBuilder<TeacherEntity> builder)
    {
        builder.ToTable("Teachers");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.EmployeeCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.EmployeeCode).IsUnique();
        builder.HasIndex(t => t.UserId);
        builder.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.School).WithMany().HasForeignKey(t => t.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(t => t.Department).WithMany(d => d.Teachers).HasForeignKey(t => t.DepartmentId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class TeacherDocumentConfiguration : IEntityTypeConfiguration<TeacherDocument>
{
    public void Configure(EntityTypeBuilder<TeacherDocument> builder)
    {
        builder.ToTable("TeacherDocuments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.DocumentName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.FileUrl).IsRequired().HasMaxLength(500);
        builder.HasOne(d => d.Teacher).WithMany(t => t.Documents).HasForeignKey(d => d.TeacherId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TeacherLeaveConfiguration : IEntityTypeConfiguration<TeacherLeave>
{
    public void Configure(EntityTypeBuilder<TeacherLeave> builder)
    {
        builder.ToTable("TeacherLeaves");
        builder.HasKey(l => l.Id);
        builder.HasOne(l => l.Teacher).WithMany(t => t.Leaves).HasForeignKey(l => l.TeacherId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TeacherSalaryConfiguration : IEntityTypeConfiguration<TeacherSalary>
{
    public void Configure(EntityTypeBuilder<TeacherSalary> builder)
    {
        builder.ToTable("TeacherSalaries");
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Teacher).WithMany(t => t.Salaries).HasForeignKey(s => s.TeacherId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TeacherPerformanceConfiguration : IEntityTypeConfiguration<TeacherPerformance>
{
    public void Configure(EntityTypeBuilder<TeacherPerformance> builder)
    {
        builder.ToTable("TeacherPerformances");
        builder.HasKey(p => p.Id);
        builder.HasOne(p => p.Teacher).WithMany().HasForeignKey(p => p.TeacherId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ParentConfiguration : IEntityTypeConfiguration<Domain.Entities.Parent.Parent>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Parent.Parent> builder)
    {
        builder.ToTable("Parents");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ParentCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.ParentCode).IsUnique();
        builder.HasIndex(p => p.UserId);
        builder.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class GuardianDetailConfiguration : IEntityTypeConfiguration<GuardianDetail>
{
    public void Configure(EntityTypeBuilder<GuardianDetail> builder)
    {
        builder.ToTable("GuardianDetails");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.FullName).IsRequired().HasMaxLength(200);
        builder.Property(g => g.Phone).IsRequired().HasMaxLength(20);
        builder.HasOne(g => g.Parent).WithMany(p => p.GuardianDetails).HasForeignKey(g => g.ParentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeConfiguration : IEntityTypeConfiguration<EmployeeEntity>
{
    public void Configure(EntityTypeBuilder<EmployeeEntity> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.EmployeeCode).IsUnique();
        builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.School).WithMany().HasForeignKey(e => e.SchoolId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.Designation).WithMany(d => d.Employees).HasForeignKey(e => e.DesignationId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> builder)
    {
        builder.ToTable("EmployeeDocuments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.DocumentName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.FileUrl).IsRequired().HasMaxLength(500);
        builder.HasOne(d => d.Employee).WithMany(e => e.Documents).HasForeignKey(d => d.EmployeeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeLeaveConfiguration : IEntityTypeConfiguration<EmployeeLeave>
{
    public void Configure(EntityTypeBuilder<EmployeeLeave> builder)
    {
        builder.ToTable("EmployeeLeaves");
        builder.HasKey(l => l.Id);
        builder.HasOne(l => l.Employee).WithMany(e => e.Leaves).HasForeignKey(l => l.EmployeeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EmployeeSalaryConfiguration : IEntityTypeConfiguration<EmployeeSalary>
{
    public void Configure(EntityTypeBuilder<EmployeeSalary> builder)
    {
        builder.ToTable("EmployeeSalaries");
        builder.HasKey(s => s.Id);
        builder.HasOne(s => s.Employee).WithMany(e => e.Salaries).HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
{
    public void Configure(EntityTypeBuilder<Designation> builder)
    {
        builder.ToTable("Designations");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Code).IsRequired().HasMaxLength(50);
    }
}
