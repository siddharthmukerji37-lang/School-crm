using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolCRM.Domain.Entities.Payroll;

namespace SchoolCRM.Infrastructure.Data.Configurations;

public class PayrollSettingConfiguration : IEntityTypeConfiguration<PayrollSetting>
{
    public void Configure(EntityTypeBuilder<PayrollSetting> builder)
    {
        builder.ToTable("PayrollSettings");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalaryProfileConfiguration : IEntityTypeConfiguration<SalaryProfile>
{
    public void Configure(EntityTypeBuilder<SalaryProfile> builder)
    {
        builder.ToTable("SalaryProfiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(256).IsRequired();
        builder.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SalaryComponentConfiguration : IEntityTypeConfiguration<SalaryComponent>
{
    public void Configure(EntityTypeBuilder<SalaryComponent> builder)
    {
        builder.ToTable("SalaryComponents");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.SalaryProfile).WithMany(c => c.Components).HasForeignKey(x => x.SalaryProfileId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PayrollConfiguration : IEntityTypeConfiguration<Domain.Entities.Payroll.Payroll>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Payroll.Payroll> builder)
    {
        builder.ToTable("Payrolls");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(256).IsRequired();
        builder.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PayrollDeductionConfiguration : IEntityTypeConfiguration<PayrollDeduction>
{
    public void Configure(EntityTypeBuilder<PayrollDeduction> builder)
    {
        builder.ToTable("PayrollDeductions");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Payroll).WithMany(p => p.Deductions).HasForeignKey(x => x.PayrollId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.ToTable("Payslips");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(256).IsRequired();
        builder.HasOne(x => x.Payroll).WithMany().HasForeignKey(x => x.PayrollId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.PayrollId).IsUnique();
    }
}
