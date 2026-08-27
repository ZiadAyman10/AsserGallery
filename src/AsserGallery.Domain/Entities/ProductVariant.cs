using AsserGallery.Domain.Common;
using AsserGallery.Domain.Exceptions;

namespace AsserGallery.Domain.Entities;

public class Color : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string HexCode { get; set; } = "#000000";

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}

public class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int ColorId { get; set; }
    public Color? Color { get; set; }

    public int Quantity { get; set; }

    public void DeductStock(int count)
    {
        if (count <= 0) return;
        if (Quantity < count)
        {
            throw new InsufficientStockException(ProductId, Id, count, Quantity);
        }
        Quantity -= count;
    }

    public void AddStock(int count)
    {
        if (count <= 0) return;
        Quantity += count;
    }
}
