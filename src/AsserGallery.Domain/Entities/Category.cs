using AsserGallery.Domain.Common;

namespace AsserGallery.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ArabicDescription { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
}

public class SubCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ArabicDescription { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
