using AsserGallery.Domain.Enums;

namespace AsserGallery.Application.Features.Products.Dtos;

public record ProductDto(
    int Id,
    string Name,
    string ArabicName,
    string? Description,
    string? ArabicDescription,
    decimal Price,
    decimal? DiscountedPrice,
    int? DiscountPercentage,
    DateTime DateAdded,
    ProductStatus Status,
    bool IsFeatured,
    int DisplayOrder,
    int SubCategoryId,
    string SubCategoryName,
    string SubCategoryArabicName,
    int CategoryId,
    string CategoryName,
    string CategoryArabicName,
    string? PrimaryImageUrl,
    int TotalStock,
    List<ProductVariantDto> Variants,
    List<ProductImageDto> Images
);

public record ProductVariantDto(
    int Id,
    int ProductId,
    int ColorId,
    string ColorName,
    string ColorArabicName,
    string ColorHexCode,
    int Quantity
);

public record ProductImageDto(
    int Id,
    int ProductId,
    string ImageUrl,
    ImageType ImageType,
    bool IsPrimary,
    int DisplayOrder
);

public record ColorDto(
    int Id,
    string Name,
    string ArabicName,
    string HexCode
);

public record CreateProductVariantInput(
    int ColorId,
    int Quantity
);

public record CreateProductImageInput(
    string ImageUrl,
    ImageType ImageType,
    bool IsPrimary,
    int DisplayOrder
);
