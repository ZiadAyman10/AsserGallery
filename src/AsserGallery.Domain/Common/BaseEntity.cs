namespace AsserGallery.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedAt { get; set; }
}

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? LastModifiedAt { get; set; }
}
