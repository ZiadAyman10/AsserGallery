using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Finances.Queries;
using AsserGallery.Application.Features.Products.Commands;
using AsserGallery.Application.Features.Products.Dtos;
using AsserGallery.Application.Features.Products.Queries;
using AsserGallery.Application.Features.Sales.Commands;
using AsserGallery.Application.Features.Sales.Dtos;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AsserGallery.Application.Tests;

public class ApplicationTests
{
    private class TestDbContext : DbContext, IApplicationDbContext
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<SubCategory> SubCategories => Set<SubCategory>();
        public DbSet<Color> Colors => Set<Color>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<Sale> Sales => Set<Sale>();
        public DbSet<SaleItem> SaleItems => Set<SaleItem>();
        public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
        public DbSet<CustomerRequest> CustomerRequests => Set<CustomerRequest>();
        public DbSet<FacebookDestination> FacebookDestinations => Set<FacebookDestination>();
        public DbSet<ProductPost> ProductPosts => Set<ProductPost>();
        public DbSet<StoreSetting> StoreSettings => Set<StoreSetting>();

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }
    }

    private TestDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task CreateProductCommandHandler_ShouldCreateProductWithVariants()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var handler = new CreateProductCommandHandler(context);

        var command = new CreateProductCommand(
            Name: "Linen Shirt",
            ArabicName: "قميص كتان",
            Description: "High quality summer linen shirt",
            ArabicDescription: "قميص صيفي من الكتان عالي الجودة",
            Price: 450m,
            DiscountedPrice: 350m,
            SubCategoryId: 1,
            IsFeatured: true,
            DisplayOrder: 1,
            Variants: new List<CreateProductVariantInput>
            {
                new(ColorId: 1, Quantity: 5),
                new(ColorId: 2, Quantity: 3)
            },
            Images: new List<CreateProductImageInput>
            {
                new("/images/shirt1.jpg", ImageType.Original, true, 1)
            }
        );

        // Act
        var productId = await handler.Handle(command, CancellationToken.None);

        // Assert
        productId.Should().BeGreaterThan(0);
        var created = await context.Products.Include(p => p.Variants).Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Linen Shirt");
        created.Status.Should().Be(ProductStatus.Available);
        created.Variants.Should().HaveCount(2);
        created.Images.Should().HaveCount(1);
    }

    [Fact]
    public async Task RegisterSaleCommandHandler_ShouldDeductStockAndRecordIncome()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var color = new Color { Id = 1, Name = "Black", ArabicName = "أسود", HexCode = "#000000" };
        context.Colors.Add(color);

        var product = new Product
        {
            Id = 1,
            Name = "Winter Jacket",
            ArabicName = "جاكيت شتوي",
            Price = 1200m,
            Status = ProductStatus.Available,
            Variants = new List<ProductVariant>
            {
                new() { Id = 1, ColorId = 1, Quantity = 10 }
            }
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new RegisterSaleCommandHandler(context);
        var command = new RegisterSaleCommand(
            CustomerName: "Ahmed Ali",
            CustomerPhone: "01012345678",
            Notes: "Delivered via Cairo Courier",
            SaleDate: DateTime.UtcNow,
            Items: new List<CreateSaleItemInput>
            {
                new(ProductId: 1, ProductVariantId: 1, Quantity: 3, UnitPrice: 1200m)
            }
        );

        // Act
        var saleId = await handler.Handle(command, CancellationToken.None);

        // Assert
        saleId.Should().BeGreaterThan(0);

        var updatedVariant = await context.ProductVariants.FindAsync(1);
        updatedVariant!.Quantity.Should().Be(7);

        var sales = await context.Sales.Include(s => s.Items).ToListAsync();
        sales.Should().HaveCount(1);
        sales[0].TotalAmount.Should().Be(3600m);

        var transactions = await context.FinancialTransactions.ToListAsync();
        transactions.Should().ContainSingle(t => t.Type == TransactionType.Income && t.Amount == 3600m);
    }

    [Fact]
    public async Task GetFinancialSummaryQueryHandler_ShouldCalculateAccurateTotals()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        context.FinancialTransactions.AddRange(
            new FinancialTransaction { Title = "Sale 1", Amount = 1500m, Type = TransactionType.Income, Category = "SalesRevenue", Date = DateTime.UtcNow },
            new FinancialTransaction { Title = "Sale 2", Amount = 2000m, Type = TransactionType.Income, Category = "SalesRevenue", Date = DateTime.UtcNow },
            new FinancialTransaction { Title = "Fabric Purchase", Amount = 800m, Type = TransactionType.Expense, Category = "StockPurchase", Date = DateTime.UtcNow },
            new FinancialTransaction { Title = "Packaging Bags", Amount = 200m, Type = TransactionType.Expense, Category = "Packaging", Date = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var handler = new GetFinancialSummaryQueryHandler(context);

        // Act
        var summary = await handler.Handle(new GetFinancialSummaryQuery(), CancellationToken.None);

        // Assert
        summary.TotalIncome.Should().Be(3500m);
        summary.TotalExpense.Should().Be(1000m);
        summary.NetProfit.Should().Be(2500m);
    }
}
