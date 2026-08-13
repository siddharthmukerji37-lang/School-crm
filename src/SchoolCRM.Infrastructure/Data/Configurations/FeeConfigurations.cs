using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolCRM.Domain.Entities.Fee;

namespace SchoolCRM.Infrastructure.Data.Configurations;

public class FeeHeadConfiguration : IEntityTypeConfiguration<FeeHead>
{
    public void Configure(EntityTypeBuilder<FeeHead> builder)
    {
        builder.ToTable("FeeHeads");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Name).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Code).IsRequired().HasMaxLength(50);
    }
}

public class FeeStructureConfiguration : IEntityTypeConfiguration<FeeStructure>
{
    public void Configure(EntityTypeBuilder<FeeStructure> builder)
    {
        builder.ToTable("FeeStructures");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Name).IsRequired().HasMaxLength(200);
        builder.Property(f => f.Amount).HasPrecision(18, 2);
        builder.Property(f => f.FineAmount).HasPrecision(18, 2);

        builder.HasOne(f => f.FeeHead).WithMany().HasForeignKey(f => f.FeeHeadId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(f => f.ClassRoom).WithMany().HasForeignKey(f => f.ClassRoomId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(f => f.AcademicYear).WithMany().HasForeignKey(f => f.AcademicYearId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class FeeInstallmentConfiguration : IEntityTypeConfiguration<FeeInstallment>
{
    public void Configure(EntityTypeBuilder<FeeInstallment> builder)
    {
        builder.ToTable("FeeInstallments");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Amount).HasPrecision(18, 2);
        builder.Property(f => f.PaidAmount).HasPrecision(18, 2);
        builder.Property(f => f.Fine).HasPrecision(18, 2);
        builder.Property(f => f.Discount).HasPrecision(18, 2);
        builder.Property(f => f.Scholarship).HasPrecision(18, 2);

        builder.HasOne(f => f.Student).WithMany(s => s.FeeInstallments).HasForeignKey(f => f.StudentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(f => f.FeeStructure).WithMany(fs => fs.Installments).HasForeignKey(f => f.FeeStructureId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class FeeReceiptConfiguration : IEntityTypeConfiguration<FeeReceipt>
{
    public void Configure(EntityTypeBuilder<FeeReceipt> builder)
    {
        builder.ToTable("FeeReceipts");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.ReceiptNumber).IsRequired().HasMaxLength(50);
        builder.Property(f => f.Amount).HasPrecision(18, 2);
        builder.Property(f => f.Fine).HasPrecision(18, 2);
        builder.Property(f => f.Discount).HasPrecision(18, 2);
        builder.Property(f => f.TotalPaid).HasPrecision(18, 2);

        builder.HasIndex(f => f.ReceiptNumber).IsUnique();

        builder.HasOne(f => f.FeeInstallment).WithMany(fi => fi.Receipts).HasForeignKey(f => f.FeeInstallmentId).OnDelete(DeleteBehavior.Cascade);
    }
}
