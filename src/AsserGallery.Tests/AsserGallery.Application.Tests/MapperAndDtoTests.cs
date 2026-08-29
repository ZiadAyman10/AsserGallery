using AsserGallery.Application.Mappers;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AsserGallery.Application.Tests;

public class MapperAndDtoTests
{
    [Fact]
    public void ProductMapper_ToDto_ShouldMapAllFieldsAccurately()
    {
        var cat = new Category { Id = 1, Name = "Men", ArabicName = "رجالي" };
        var subCat = new SubCategory { Id = 10, CategoryId = 1, Category = cat, Name = "Jackets", ArabicName = "جواكت" };
        var color = new Color { Id = 5, Name = "Navy", ArabicName = "كحلي", HexCode = "#000080" };

        var product = new Product
        {
            Id = 100,
            Name = "Winter Parka",
            ArabicName = "باركا شتوية",
            Description = "Warm winter coat",
            ArabicDescription = "جاكيت شتوي دافئ",
            Price = 1800m,
            DiscountedPrice = 1350m,
            DateAdded = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            Status = ProductStatus.Available,
            IsFeatured = true,
            DisplayOrder = 1,
            SubCategoryId = 10,
            SubCategory = subCat,
            Images = new List<ProductImage>
            {
                new() { Id = 1, ImageUrl = "/img/parka_thumb.jpg", ImageType = ImageType.Original, IsPrimary = true, DisplayOrder = 1 },
                new() { Id = 2, ImageUrl = "/img/parka_ai.jpg", ImageType = ImageType.AiEnhanced, IsPrimary = false, DisplayOrder = 2 }
            },
            Variants = new List<ProductVariant>
            {
                new() { Id = 50, ColorId = 5, Color = color, Quantity = 8 }
            }
        };

        var dto = product.ToDto();

        dto.Id.Should().Be(100);
        dto.Name.Should().Be("Winter Parka");
        dto.ArabicName.Should().Be("باركا شتوية");
        dto.Price.Should().Be(1800m);
        dto.DiscountedPrice.Should().Be(1350m);
        dto.DiscountPercentage.Should().Be(25);
        dto.SubCategoryName.Should().Be("Jackets");
        dto.CategoryName.Should().Be("Men");
        dto.PrimaryImageUrl.Should().Be("/img/parka_thumb.jpg");
        dto.TotalStock.Should().Be(8);
        dto.Variants.Should().HaveCount(1);
        dto.Variants[0].ColorName.Should().Be("Navy");
        dto.Images.Should().HaveCount(2);
    }

    [Fact]
    public void CategoryMapper_ToDto_ShouldMapHierarchicalCategory()
    {
        var category = new Category
        {
            Id = 2,
            Name = "Women",
            ArabicName = "حريمي",
            Description = "Women's collection",
            ArabicDescription = "تشكيلة حريمي",
            ImageUrl = "/img/women.jpg",
            DisplayOrder = 1,
            IsActive = true,
            SubCategories = new List<SubCategory>
            {
                new()
                {
                    Id = 20,
                    CategoryId = 2,
                    Name = "Dresses",
                    ArabicName = "فساتين",
                    DisplayOrder = 1,
                    IsActive = true,
                    Products = new List<Product>
                    {
                        new() { Id = 1, Name = "Maxi Dress" }
                    }
                }
            }
        };

        var dto = category.ToDto();

        dto.Id.Should().Be(2);
        dto.Name.Should().Be("Women");
        dto.ProductCount.Should().Be(1);
        dto.SubCategories.Should().HaveCount(1);
        dto.SubCategories[0].Name.Should().Be("Dresses");
        dto.SubCategories[0].ProductCount.Should().Be(1);
    }

    [Fact]
    public void SaleMapper_ToDto_ShouldMapSaleWithItems()
    {
        var product = new Product { Id = 1, Name = "Silk Shirt", ArabicName = "قميص حرير" };
        var color = new Color { Id = 1, Name = "White", ArabicName = "أبيض" };
        var variant = new ProductVariant { Id = 1, ColorId = 1, Color = color };

        var sale = new Sale
        {
            Id = 5,
            SaleNumber = "INV-2026-0005",
            SaleDate = new DateTime(2026, 8, 20, 14, 30, 0, DateTimeKind.Utc),
            TotalAmount = 750m,
            CustomerName = "Mariam",
            CustomerPhone = "01099999999",
            Notes = "Home delivery",
            Items = new List<SaleItem>
            {
                new()
                {
                    Id = 10,
                    ProductId = 1,
                    Product = product,
                    ProductVariantId = 1,
                    ProductVariant = variant,
                    Quantity = 1,
                    UnitPrice = 750m
                }
            }
        };

        var dto = sale.ToDto();

        dto.Id.Should().Be(5);
        dto.SaleNumber.Should().Be("INV-2026-0005");
        dto.CustomerName.Should().Be("Mariam");
        dto.TotalAmount.Should().Be(750m);
        dto.Items.Should().HaveCount(1);
        dto.Items[0].ProductName.Should().Be("Silk Shirt");
        dto.Items[0].ColorName.Should().Be("White");
        dto.Items[0].SubTotal.Should().Be(750m);
    }

    [Fact]
    public void FinanceMapper_ToDto_ShouldMapTransaction()
    {
        var product = new Product { Id = 1, Name = "Linen Fabric" };
        var tx = new FinancialTransaction
        {
            Id = 12,
            Title = "Bulk Raw Fabric Purchase",
            Description = "100 meters raw organic linen",
            Amount = 4500m,
            Date = new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc),
            Type = TransactionType.Expense,
            Category = "StockPurchase",
            LinkedProductId = 1,
            LinkedProduct = product
        };

        var dto = tx.ToDto();

        dto.Id.Should().Be(12);
        dto.Title.Should().Be("Bulk Raw Fabric Purchase");
        dto.Amount.Should().Be(4500m);
        dto.Type.Should().Be(TransactionType.Expense);
        dto.Category.Should().Be("StockPurchase");
        dto.LinkedProductName.Should().Be("Linen Fabric");
    }

    [Fact]
    public void CustomerRequestMapper_ToDto_ShouldMapInquiry()
    {
        var product = new Product { Id = 3, Name = "Summer Blazer" };
        var req = new CustomerRequest
        {
            Id = 8,
            CustomerName = "Tarek",
            PhoneNumber = "01234567890",
            PreferredChannel = ContactChannel.WhatsApp,
            Message = "Need size XL in Navy",
            ProductId = 3,
            Product = product,
            Status = CustomerRequestStatus.New,
            CreatedAt = new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc)
        };

        var dto = req.ToDto();

        dto.Id.Should().Be(8);
        dto.CustomerName.Should().Be("Tarek");
        dto.PreferredChannel.Should().Be(ContactChannel.WhatsApp);
        dto.ProductName.Should().Be("Summer Blazer");
        dto.Status.Should().Be(CustomerRequestStatus.New);
    }
}
