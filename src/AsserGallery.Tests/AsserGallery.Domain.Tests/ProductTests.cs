using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using AsserGallery.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace AsserGallery.Domain.Tests;

public class ProductTests
{
    [Fact]
    public void GetTotalStock_ShouldSumVariantQuantities()
    {
        // Arrange
        var product = new Product
        {
            Name = "Winter Jacket",
            Variants = new List<ProductVariant>
            {
                new() { Quantity = 5 },
                new() { Quantity = 3 },
                new() { Quantity = 0 }
            }
        };

        // Act
        var totalStock = product.GetTotalStock();

        // Assert
        totalStock.Should().Be(8);
    }

    [Theory]
    [InlineData(0, ProductStatus.OutOfStock)]
    [InlineData(2, ProductStatus.LimitedStock)]
    [InlineData(3, ProductStatus.LimitedStock)]
    [InlineData(4, ProductStatus.Available)]
    [InlineData(10, ProductStatus.Available)]
    public void UpdateStatusFromStock_ShouldSetCorrectStatus(int stock, ProductStatus expectedStatus)
    {
        // Arrange
        var product = new Product
        {
            Name = "T-Shirt",
            Variants = new List<ProductVariant>
            {
                new() { Quantity = stock }
            }
        };

        // Act
        product.UpdateStatusFromStock();

        // Assert
        product.Status.Should().Be(expectedStatus);
    }

    [Fact]
    public void CalculateDiscountPercentage_ShouldReturnCorrectDiscount()
    {
        // Arrange
        var product = new Product
        {
            Price = 1000m,
            DiscountedPrice = 700m
        };

        // Act
        var discount = product.CalculateDiscountPercentage();

        // Assert
        discount.Should().Be(30);
    }

    [Fact]
    public void CalculateDiscountPercentage_WhenNoDiscount_ShouldReturnNull()
    {
        // Arrange
        var product = new Product
        {
            Price = 500m,
            DiscountedPrice = null
        };

        // Act
        var discount = product.CalculateDiscountPercentage();

        // Assert
        discount.Should().BeNull();
    }
}

public class ProductVariantTests
{
    [Fact]
    public void DeductStock_WhenSufficientQuantity_ShouldDecreaseStock()
    {
        // Arrange
        var variant = new ProductVariant { ProductId = 1, Quantity = 10 };

        // Act
        variant.DeductStock(4);

        // Assert
        variant.Quantity.Should().Be(6);
    }

    [Fact]
    public void DeductStock_WhenInsufficientQuantity_ShouldThrowInsufficientStockException()
    {
        // Arrange
        var variant = new ProductVariant { ProductId = 1, Quantity = 2 };

        // Act & Assert
        var act = () => variant.DeductStock(5);
        act.Should().Throw<InsufficientStockException>();
    }

    [Fact]
    public void AddStock_ShouldIncreaseStock()
    {
        // Arrange
        var variant = new ProductVariant { ProductId = 1, Quantity = 5 };

        // Act
        variant.AddStock(7);

        // Assert
        variant.Quantity.Should().Be(12);
    }
}
