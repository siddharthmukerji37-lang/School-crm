using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolCRM.Domain.Entities.Account;
using SchoolCRM.Domain.Entities.Inventory;
using SchoolCRM.Domain.Entities.Notification;
using SchoolCRM.Domain.Entities.Setting;

namespace SchoolCRM.Infrastructure.Data.Configurations;

public class AccountHeadConfiguration : IEntityTypeConfiguration<AccountHead>
{
    public void Configure(EntityTypeBuilder<AccountHead> builder)
    {
        builder.ToTable("AccountHeads");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Code).IsRequired().HasMaxLength(50);
    }
}

public class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> builder)
    {
        builder.ToTable("LedgerEntries");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Description).IsRequired().HasMaxLength(500);
        builder.Property(l => l.Debit).HasPrecision(18, 2);
        builder.Property(l => l.Credit).HasPrecision(18, 2);
        builder.Property(l => l.Balance).HasPrecision(18, 2);

        builder.HasOne(l => l.AccountHead).WithMany().HasForeignKey(l => l.AccountHeadId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class IncomeConfiguration : IEntityTypeConfiguration<Income>
{
    public void Configure(EntityTypeBuilder<Income> builder)
    {
        builder.ToTable("Incomes");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Description).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Amount).HasPrecision(18, 2);

        builder.HasOne(i => i.AccountHead).WithMany().HasForeignKey(i => i.AccountHeadId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Amount).HasPrecision(18, 2);

        builder.HasOne(e => e.AccountHead).WithMany().HasForeignKey(e => e.AccountHeadId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("BankAccounts");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BankName).IsRequired().HasMaxLength(200);
        builder.Property(b => b.AccountNumber).IsRequired().HasMaxLength(50);
        builder.Property(b => b.CurrentBalance).HasPrecision(18, 2);
    }
}

public class InventoryCategoryConfiguration : IEntityTypeConfiguration<InventoryCategory>
{
    public void Configure(EntityTypeBuilder<InventoryCategory> builder)
    {
        builder.ToTable("InventoryCategories");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
    }
}

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name).IsRequired().HasMaxLength(200);
        builder.Property(v => v.Phone).IsRequired().HasMaxLength(20);
    }
}

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Code).IsRequired().HasMaxLength(50);
        builder.Property(i => i.PurchasePrice).HasPrecision(18, 2);
        builder.Property(i => i.SellingPrice).HasPrecision(18, 2);

        builder.HasOne(i => i.Category).WithMany().HasForeignKey(i => i.CategoryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Vendor).WithMany().HasForeignKey(i => i.VendorId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.ToTable("StockTransactions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.UnitPrice).HasPrecision(18, 2);
        builder.Property(t => t.TotalAmount).HasPrecision(18, 2);

        builder.HasOne(t => t.Item).WithMany(i => i.Transactions).HasForeignKey(t => t.ItemId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Domain.Entities.Notification.Notification>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Notification.Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(300);
        builder.Property(n => n.Message).IsRequired();

        builder.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable("Announcements");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(300);
        builder.Property(a => a.Content).IsRequired();
    }
}

public class CircularConfiguration : IEntityTypeConfiguration<Circular>
{
    public void Configure(EntityTypeBuilder<Circular> builder)
    {
        builder.ToTable("Circulars");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Content).IsRequired();
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Message).IsRequired();

        builder.HasOne(c => c.Sender).WithMany().HasForeignKey(c => c.SenderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Receiver).WithMany().HasForeignKey(c => c.ReceiverId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Section).WithMany().HasForeignKey(c => c.SectionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.ParentMessage).WithMany().HasForeignKey(c => c.ParentMessageId).OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.SenderId);
        builder.HasIndex(c => c.ReceiverId);
        builder.HasIndex(c => c.SectionId);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.PerformedBy).IsRequired().HasMaxLength(200);

        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.Timestamp);
    }
}

public class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("LoginHistories");
        builder.HasKey(l => l.Id);

        builder.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Module).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Action).IsRequired().HasMaxLength(50);
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Role).WithMany().HasForeignKey(r => r.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(r => r.Permission).WithMany(p => p.RolePermissions).HasForeignKey(r => r.PermissionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SchoolSettingConfiguration : IEntityTypeConfiguration<SchoolSetting>
{
    public void Configure(EntityTypeBuilder<SchoolSetting> builder)
    {
        builder.ToTable("SchoolSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Key).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Value).IsRequired().HasMaxLength(2000);
    }
}

public class EmailSettingConfiguration : IEntityTypeConfiguration<EmailSetting>
{
    public void Configure(EntityTypeBuilder<EmailSetting> builder)
    {
        builder.ToTable("EmailSettings");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SmtpServer).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Username).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Password).IsRequired().HasMaxLength(500);
        builder.Property(e => e.FromEmail).IsRequired().HasMaxLength(200);
    }
}

public class SmsSettingConfiguration : IEntityTypeConfiguration<SmsSetting>
{
    public void Configure(EntityTypeBuilder<SmsSetting> builder)
    {
        builder.ToTable("SmsSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Provider).IsRequired().HasMaxLength(200);
        builder.Property(s => s.ApiKey).IsRequired().HasMaxLength(500);
        builder.Property(s => s.SenderId).IsRequired().HasMaxLength(50);
    }
}

public class DataBackupConfiguration : IEntityTypeConfiguration<DataBackup>
{
    public void Configure(EntityTypeBuilder<DataBackup> builder)
    {
        builder.ToTable("DataBackups");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.FileName).IsRequired().HasMaxLength(500);
    }
}
