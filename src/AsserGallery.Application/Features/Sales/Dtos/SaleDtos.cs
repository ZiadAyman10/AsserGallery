using AsserGallery.Domain.Enums;

namespace AsserGallery.Application.Features.Sales.Dtos;

public record SaleDto(
    int Id,
    string SaleNumber,
    DateTime SaleDate,
    decimal TotalAmount,
    string? CustomerName,
    string? CustomerPhone,
    string? Notes,
    List<SaleItemDto> Items
);

public record SaleItemDto(
    int Id,
    int SaleId,
    int ProductId,
    string ProductName,
    string ProductArabicName,
    int ProductVariantId,
    string ColorName,
    string ColorArabicName,
    string ColorHexCode,
    int Quantity,
    decimal UnitPrice,
    decimal SubTotal
);

public record CreateSaleItemInput(
    int ProductId,
    int ProductVariantId,
    int Quantity,
    decimal UnitPrice
);
