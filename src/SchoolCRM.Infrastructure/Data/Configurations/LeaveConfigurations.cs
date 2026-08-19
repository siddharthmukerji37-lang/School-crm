using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolCRM.Domain.Entities.Leave;

namespace SchoolCRM.Infrastructure.Data.Configurations;

public class LeaveCalendarConfiguration : IEntityTypeConfiguration<LeaveCalendar>
{
    public void Configure(EntityTypeBuilder<LeaveCalendar> builder)
    {
        builder.ToTable("LeaveCalendars");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("LeaveTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.HasOne(x => x.School).WithMany().HasForeignKey(x => x.SchoolId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LeaveTypeConfigConfiguration : IEntityTypeConfiguration<LeaveTypeConfig>
{
    public void Configure(EntityTypeBuilder<LeaveTypeConfig> builder)
    {
        builder.ToTable("LeaveTypeConfigs");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.LeaveCalendar).WithMany(c => c.LeaveTypeConfigs).HasForeignKey(x => x.LeaveCalendarId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.LeaveType).WithMany(t => t.LeaveTypeConfigs).HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(x => x.ApplicableUserType).HasMaxLength(20).IsRequired();
    }
}

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("LeaveBalances");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeaveCalendar).WithMany().HasForeignKey(x => x.LeaveCalendarId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(x => x.RemainingDays);
    }
}

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.AdminReason).HasMaxLength(1000);
        builder.HasOne(x => x.LeaveType).WithMany().HasForeignKey(x => x.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.LeaveCalendar).WithMany().HasForeignKey(x => x.LeaveCalendarId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LeaveRequestDayConfiguration : IEntityTypeConfiguration<LeaveRequestDay>
{
    public void Configure(EntityTypeBuilder<LeaveRequestDay> builder)
    {
        builder.ToTable("LeaveRequestDays");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.LeaveRequest).WithMany(r => r.LeaveRequestDays).HasForeignKey(x => x.LeaveRequestId).OnDelete(DeleteBehavior.Cascade);
    }
}
