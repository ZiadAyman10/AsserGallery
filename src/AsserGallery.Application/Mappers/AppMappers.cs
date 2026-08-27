using AsserGallery.Application.Features.Categories.Dtos;
using AsserGallery.Application.Features.CustomerRequests.Dtos;
using AsserGallery.Application.Features.Facebook.Dtos;
using AsserGallery.Application.Features.Finances.Dtos;
using AsserGallery.Application.Features.Products.Dtos;
using AsserGallery.Application.Features.Sales.Dtos;
using AsserGallery.Domain.Entities;

namespace AsserGallery.Application.Mappers;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product product)
    {
        var primaryImage = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                           ?? product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl;

        return new ProductDto(
            Id: product.Id,
            Name: product.Name,
            ArabicName: product.ArabicName,
            Description: product.Description,
            ArabicDescription: product.ArabicDescription,
            Price: product.Price,
            DiscountedPrice: product.DiscountedPrice,
            DiscountPercentage: product.CalculateDiscountPercentage(),
            DateAdded: product.DateAdded,
            Status: product.Status,
            IsFeatured: product.IsFeatured,
            DisplayOrder: product.DisplayOrder,
            SubCategoryId: product.SubCategoryId,
            SubCategoryName: product.SubCategory?.Name ?? string.Empty,
            SubCategoryArabicName: product.SubCategory?.ArabicName ?? string.Empty,
            CategoryId: product.SubCategory?.CategoryId ?? 0,
            CategoryName: product.SubCategory?.Category?.Name ?? string.Empty,
            CategoryArabicName: product.SubCategory?.Category?.ArabicName ?? string.Empty,
            PrimaryImageUrl: primaryImage,
            TotalStock: product.GetTotalStock(),
            Variants: product.Variants.Select(v => v.ToDto()).ToList(),
            Images: product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ToDto()).ToList()
        );
    }

    public static ProductVariantDto ToDto(this ProductVariant variant)
    {
        return new ProductVariantDto(
            Id: variant.Id,
            ProductId: variant.ProductId,
            ColorId: variant.ColorId,
            ColorName: variant.Color?.Name ?? string.Empty,
            ColorArabicName: variant.Color?.ArabicName ?? string.Empty,
            ColorHexCode: variant.Color?.HexCode ?? "#000000",
            Quantity: variant.Quantity
        );
    }

    public static ProductImageDto ToDto(this ProductImage image)
    {
        return new ProductImageDto(
            Id: image.Id,
            ProductId: image.ProductId,
            ImageUrl: image.ImageUrl,
            ImageType: image.ImageType,
            IsPrimary: image.IsPrimary,
            DisplayOrder: image.DisplayOrder
        );
    }

    public static ColorDto ToDto(this Color color)
    {
        return new ColorDto(
            Id: color.Id,
            Name: color.Name,
            ArabicName: color.ArabicName,
            HexCode: color.HexCode
        );
    }
}

public static class CategoryMapper
{
    public static CategoryDto ToDto(this Category category)
    {
        return new CategoryDto(
            Id: category.Id,
            Name: category.Name,
            ArabicName: category.ArabicName,
            Description: category.Description,
            ArabicDescription: category.ArabicDescription,
            ImageUrl: category.ImageUrl,
            DisplayOrder: category.DisplayOrder,
            IsActive: category.IsActive,
            ProductCount: category.SubCategories.Sum(sc => sc.Products.Count),
            SubCategories: category.SubCategories.OrderBy(sc => sc.DisplayOrder).Select(sc => sc.ToDto()).ToList()
        );
    }

    public static SubCategoryDto ToDto(this SubCategory subCategory)
    {
        return new SubCategoryDto(
            Id: subCategory.Id,
            Name: subCategory.Name,
            ArabicName: subCategory.ArabicName,
            Description: subCategory.Description,
            ArabicDescription: subCategory.ArabicDescription,
            DisplayOrder: subCategory.DisplayOrder,
            IsActive: subCategory.IsActive,
            CategoryId: subCategory.CategoryId,
            CategoryName: subCategory.Category?.Name ?? string.Empty,
            CategoryArabicName: subCategory.Category?.ArabicName ?? string.Empty,
            ProductCount: subCategory.Products.Count
        );
    }
}

public static class SaleMapper
{
    public static SaleDto ToDto(this Sale sale)
    {
        return new SaleDto(
            Id: sale.Id,
            SaleNumber: sale.SaleNumber,
            SaleDate: sale.SaleDate,
            TotalAmount: sale.TotalAmount,
            CustomerName: sale.CustomerName,
            CustomerPhone: sale.CustomerPhone,
            Notes: sale.Notes,
            Items: sale.Items.Select(i => i.ToDto()).ToList()
        );
    }

    public static SaleItemDto ToDto(this SaleItem item)
    {
        return new SaleItemDto(
            Id: item.Id,
            SaleId: item.SaleId,
            ProductId: item.ProductId,
            ProductName: item.Product?.Name ?? string.Empty,
            ProductArabicName: item.Product?.ArabicName ?? string.Empty,
            ProductVariantId: item.ProductVariantId,
            ColorName: item.ProductVariant?.Color?.Name ?? string.Empty,
            ColorArabicName: item.ProductVariant?.Color?.ArabicName ?? string.Empty,
            ColorHexCode: item.ProductVariant?.Color?.HexCode ?? "#000000",
            Quantity: item.Quantity,
            UnitPrice: item.UnitPrice,
            SubTotal: item.SubTotal
        );
    }
}

public static class FinanceMapper
{
    public static FinancialTransactionDto ToDto(this FinancialTransaction transaction)
    {
        return new FinancialTransactionDto(
            Id: transaction.Id,
            Title: transaction.Title,
            Description: transaction.Description,
            Amount: transaction.Amount,
            Date: transaction.Date,
            Type: transaction.Type,
            Category: transaction.Category,
            LinkedProductId: transaction.LinkedProductId,
            LinkedProductName: transaction.LinkedProduct?.Name
        );
    }
}

public static class CustomerRequestMapper
{
    public static CustomerRequestDto ToDto(this CustomerRequest request)
    {
        return new CustomerRequestDto(
            Id: request.Id,
            CustomerName: request.CustomerName,
            PhoneNumber: request.PhoneNumber,
            PreferredChannel: request.PreferredChannel,
            Message: request.Message,
            ProductId: request.ProductId,
            ProductName: request.Product?.Name,
            ProductArabicName: request.Product?.ArabicName,
            Status: request.Status,
            AdminNotes: request.AdminNotes,
            CreatedAt: request.CreatedAt
        );
    }
}

public static class FacebookMapper
{
    public static FacebookDestinationDto ToDto(this FacebookDestination destination)
    {
        return new FacebookDestinationDto(
            Id: destination.Id,
            Name: destination.Name,
            DestinationType: destination.DestinationType,
            TargetIdOrUrl: destination.TargetIdOrUrl,
            IsActive: destination.IsActive,
            PostCount: destination.Posts.Count
        );
    }

    public static ProductPostDto ToDto(this ProductPost post)
    {
        return new ProductPostDto(
            Id: post.Id,
            ProductId: post.ProductId,
            ProductName: post.Product?.Name ?? string.Empty,
            FacebookDestinationId: post.FacebookDestinationId,
            FacebookDestinationName: post.FacebookDestination?.Name ?? string.Empty,
            DestinationType: post.FacebookDestination?.DestinationType ?? Domain.Enums.DestinationType.Group,
            PostedAt: post.PostedAt,
            PostContent: post.PostContent,
            PostUrlOrId: post.PostUrlOrId,
            Status: post.Status,
            Notes: post.Notes
        );
    }
}
