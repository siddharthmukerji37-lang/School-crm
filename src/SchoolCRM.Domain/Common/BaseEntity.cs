namespace SchoolCRM.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}

public abstract class BaseAuditableEntity : BaseEntity
{
    public byte[]? RowVersion { get; set; }
}
