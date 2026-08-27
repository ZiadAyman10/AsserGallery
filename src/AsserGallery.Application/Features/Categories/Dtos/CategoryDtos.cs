using AsserGallery.Domain.Enums;

namespace AsserGallery.Application.Features.Categories.Dtos;

public record CategoryDto(
    int Id,
    string Name,
    string ArabicName,
    string? Description,
    string? ArabicDescription,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    int ProductCount,
    List<SubCategoryDto> SubCategories
);

public record SubCategoryDto(
    int Id,
    string Name,
    string ArabicName,
    string? Description,
    string? ArabicDescription,
    int DisplayOrder,
    bool IsActive,
    int CategoryId,
    string CategoryName,
    string CategoryArabicName,
    int ProductCount
);
