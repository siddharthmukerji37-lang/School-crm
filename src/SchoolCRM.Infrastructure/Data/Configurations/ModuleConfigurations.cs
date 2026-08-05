using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolCRM.Domain.Entities.Library;
using SchoolCRM.Domain.Entities.Transport;
using SchoolCRM.Domain.Entities.Hostel;
using SchoolCRM.Domain.Entities.Homework;

namespace SchoolCRM.Infrastructure.Data.Configurations;

public class BookCategoryConfiguration : IEntityTypeConfiguration<BookCategory>
{
    public void Configure(EntityTypeBuilder<BookCategory> builder)
    {
        builder.ToTable("BookCategories");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Code).IsRequired().HasMaxLength(50);
    }
}

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Title).IsRequired().HasMaxLength(500);
        builder.Property(b => b.ISBN).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Author).HasMaxLength(300);
        builder.Property(b => b.Price).HasPrecision(18, 2);

        builder.HasIndex(b => b.ISBN);

        builder.HasOne(b => b.Category).WithMany(c => c.Books).HasForeignKey(b => b.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class BookIssueConfiguration : IEntityTypeConfiguration<BookIssue>
{
    public void Configure(EntityTypeBuilder<BookIssue> builder)
    {
        builder.ToTable("BookIssues");
        builder.HasKey(b => b.Id);

        builder.HasOne(b => b.Book).WithMany(bk => bk.Issues).HasForeignKey(b => b.BookId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(b => b.Student).WithMany(s => s.BookIssues).HasForeignKey(b => b.StudentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TransportRouteConfiguration : IEntityTypeConfiguration<TransportRoute>
{
    public void Configure(EntityTypeBuilder<TransportRoute> builder)
    {
        builder.ToTable("TransportRoutes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Code).IsRequired().HasMaxLength(50);
        builder.Property(t => t.MonthlyFee).HasPrecision(18, 2);
    }
}

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.VehicleNumber).IsRequired().HasMaxLength(50);

        builder.HasOne(v => v.Driver).WithMany(d => d.Vehicles).HasForeignKey(v => v.DriverId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(v => v.Route).WithMany(r => r.Vehicles).HasForeignKey(v => v.RouteId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Phone).IsRequired().HasMaxLength(20);
    }
}

public class PickupPointConfiguration : IEntityTypeConfiguration<PickupPoint>
{
    public void Configure(EntityTypeBuilder<PickupPoint> builder)
    {
        builder.ToTable("PickupPoints");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);

        builder.HasOne(p => p.Route).WithMany(r => r.PickupPoints).HasForeignKey(p => p.RouteId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class StudentTransportAllocationConfiguration : IEntityTypeConfiguration<StudentTransportAllocation>
{
    public void Configure(EntityTypeBuilder<StudentTransportAllocation> builder)
    {
        builder.ToTable("StudentTransportAllocations");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Student).WithMany(s => s.TransportAllocations).HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Route).WithMany(r => r.Allocations).HasForeignKey(a => a.RouteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.PickupPoint).WithMany().HasForeignKey(a => a.PickupPointId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class HostelRoomConfiguration : IEntityTypeConfiguration<HostelRoom>
{
    public void Configure(EntityTypeBuilder<HostelRoom> builder)
    {
        builder.ToTable("HostelRooms");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.RoomNumber).IsRequired().HasMaxLength(50);
        builder.Property(h => h.MonthlyFee).HasPrecision(18, 2);
    }
}

public class HostelBedConfiguration : IEntityTypeConfiguration<HostelBed>
{
    public void Configure(EntityTypeBuilder<HostelBed> builder)
    {
        builder.ToTable("HostelBeds");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BedNumber).IsRequired().HasMaxLength(50);

        builder.HasOne(b => b.Room).WithMany(r => r.Beds).HasForeignKey(b => b.RoomId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class HostelAllocationConfiguration : IEntityTypeConfiguration<HostelAllocation>
{
    public void Configure(EntityTypeBuilder<HostelAllocation> builder)
    {
        builder.ToTable("HostelAllocations");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Student).WithMany(s => s.HostelAllocations).HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Room).WithMany(r => r.Allocations).HasForeignKey(a => a.RoomId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Bed).WithMany().HasForeignKey(a => a.BedId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class HomeworkConfiguration : IEntityTypeConfiguration<Homework>
{
    public void Configure(EntityTypeBuilder<Homework> builder)
    {
        builder.ToTable("Homeworks");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Title).IsRequired().HasMaxLength(300);
        builder.Property(h => h.Description).IsRequired();

        builder.HasOne(h => h.ClassRoom).WithMany().HasForeignKey(h => h.ClassRoomId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(h => h.Subject).WithMany().HasForeignKey(h => h.SubjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(h => h.Teacher).WithMany(t => t.Homeworks).HasForeignKey(h => h.TeacherId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class HomeworkSubmissionConfiguration : IEntityTypeConfiguration<HomeworkSubmission>
{
    public void Configure(EntityTypeBuilder<HomeworkSubmission> builder)
    {
        builder.ToTable("HomeworkSubmissions");
        builder.HasKey(h => h.Id);

        builder.HasOne(h => h.Homework).WithMany(hw => hw.Submissions).HasForeignKey(h => h.HomeworkId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(h => h.Student).WithMany().HasForeignKey(h => h.StudentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(300);

        builder.HasOne(a => a.ClassRoom).WithMany().HasForeignKey(a => a.ClassRoomId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Subject).WithMany().HasForeignKey(a => a.SubjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Teacher).WithMany(t => t.Assignments).HasForeignKey(a => a.TeacherId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AssignmentSubmissionConfiguration : IEntityTypeConfiguration<AssignmentSubmission>
{
    public void Configure(EntityTypeBuilder<AssignmentSubmission> builder)
    {
        builder.ToTable("AssignmentSubmissions");
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.Assignment).WithMany(asn => asn.Submissions).HasForeignKey(a => a.AssignmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Student).WithMany().HasForeignKey(a => a.StudentId).OnDelete(DeleteBehavior.Cascade);
    }
}
