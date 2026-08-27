using AsserGallery.Domain.Common;
using AsserGallery.Domain.Enums;

namespace AsserGallery.Domain.Entities;

public class ProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
    public ImageType ImageType { get; set; } = ImageType.Original;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ArabicDescription { get; set; }

    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    public ProductStatus Status { get; set; } = ProductStatus.Available;
    public bool IsFeatured { get; set; }
    public int DisplayOrder { get; set; }

    public int SubCategoryId { get; set; }
    public SubCategory? SubCategory { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<FinancialTransaction> FinancialTransactions { get; set; } = new List<FinancialTransaction>();
    public ICollection<ProductPost> Posts { get; set; } = new List<ProductPost>();

    public int GetTotalStock()
    {
        return Variants.Sum(v => v.Quantity);
    }

    public void UpdateStatusFromStock()
    {
        var total = GetTotalStock();
        if (total <= 0)
        {
            Status = ProductStatus.OutOfStock;
        }
        else if (total <= 3)
        {
            Status = ProductStatus.LimitedStock;
        }
        else
        {
            Status = ProductStatus.Available;
        }
    }

    public int? CalculateDiscountPercentage()
    {
        if (DiscountedPrice.HasValue && DiscountedPrice.Value > 0 && DiscountedPrice.Value < Price)
        {
            return (int)Math.Round((1 - (DiscountedPrice.Value / Price)) * 100);
        }
        return null;
    }
}
