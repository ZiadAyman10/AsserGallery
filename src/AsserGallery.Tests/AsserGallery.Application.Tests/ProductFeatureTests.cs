using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Products.Commands;
using AsserGallery.Application.Features.Products.Dtos;
using AsserGallery.Application.Features.Products.Queries;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AsserGallery.Application.Tests;

public class ProductFeatureTests
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
    public async Task UpdateProductCommandHandler_ShouldUpdatePropertiesAndVariants()
    {
        using var context = GetInMemoryDbContext();
        var cat = new Category { Id = 1, Name = "Men", ArabicName = "رجالي" };
        var subCat = new SubCategory { Id = 1, CategoryId = 1, Category = cat, Name = "Pants", ArabicName = "بنطلونات" };
        context.Categories.Add(cat);
        context.SubCategories.Add(subCat);

        var product = new Product
        {
            Id = 1,
            Name = "Denim Jeans",
            ArabicName = "جينز",
            Price = 500m,
            SubCategoryId = 1,
            SubCategory = subCat,
            Variants = new List<ProductVariant>
            {
                new() { Id = 1, ColorId = 1, Quantity = 5 }
            }
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new UpdateProductCommandHandler(context);
        var command = new UpdateProductCommand(
            1,
            "Slim Denim Jeans",
            "جينز سليم",
            "Updated desc",
            "وصف محدث",
            550m,
            450m,
            1,
            true,
            2,
            new List<CreateProductVariantInput>
            {
                new(ColorId: 1, Quantity: 10),
                new(ColorId: 2, Quantity: 4)
            }
        );

        var success = await handler.Handle(command, CancellationToken.None);

        success.Should().BeTrue();
        var updated = await context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == 1);
        updated!.Name.Should().Be("Slim Denim Jeans");
        updated.Price.Should().Be(550m);
        updated.DiscountedPrice.Should().Be(450m);
        updated.Variants.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteProductCommandHandler_ShouldRemoveProduct()
    {
        using var context = GetInMemoryDbContext();
        var product = new Product { Id = 10, Name = "Temporary Item", ArabicName = "عنصر مؤقت", Price = 100m };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new DeleteProductCommandHandler(context);
        var success = await handler.Handle(new DeleteProductCommand(10), CancellationToken.None);

        success.Should().BeTrue();
        var deleted = await context.Products.FindAsync(10);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task AdjustVariantStockCommandHandler_ShouldUpdateQuantityAndStatus()
    {
        using var context = GetInMemoryDbContext();
        var product = new Product
        {
            Id = 20,
            Name = "T-Shirt",
            Price = 200m,
            Status = ProductStatus.Available,
            Variants = new List<ProductVariant>
            {
                new() { Id = 100, ColorId = 1, Quantity = 10 }
            }
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var handler = new AdjustVariantStockCommandHandler(context);
        var success = await handler.Handle(new AdjustVariantStockCommand(100, 0), CancellationToken.None);

        success.Should().BeTrue();
        var variant = await context.ProductVariants.FindAsync(100);
        variant!.Quantity.Should().Be(0);
        var updatedProd = await context.Products.FindAsync(20);
        updatedProd!.Status.Should().Be(ProductStatus.OutOfStock);
    }

    [Fact]
    public async Task AddAndDeleteProductImageCommandHandler_ShouldManageImages()
    {
        using var context = GetInMemoryDbContext();
        var product = new Product { Id = 30, Name = "Blouse", Price = 300m };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var addHandler = new AddProductImageCommandHandler(context);
        var imgId = await addHandler.Handle(new AddProductImageCommand(30, "/img1.png", ImageType.Original, true), CancellationToken.None);

        imgId.Should().BeGreaterThan(0);
        var image = await context.ProductImages.FindAsync(imgId);
        image!.IsPrimary.Should().BeTrue();

        var deleteHandler = new DeleteProductImageCommandHandler(context);
        var deleted = await deleteHandler.Handle(new DeleteProductImageCommand(imgId), CancellationToken.None);
        deleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductsQueryHandler_ShouldFilterCorrectly()
    {
        using var context = GetInMemoryDbContext();
        var cat = new Category { Id = 1, Name = "Women", ArabicName = "حريمي" };
        var subCat = new SubCategory { Id = 1, CategoryId = 1, Category = cat, Name = "Dresses", ArabicName = "فساتين" };
        var color = new Color { Id = 1, Name = "Red", ArabicName = "أحمر", HexCode = "#FF0000" };
        context.Categories.Add(cat);
        context.SubCategories.Add(subCat);
        context.Colors.Add(color);

        context.Products.AddRange(
            new Product
            {
                Id = 1,
                Name = "Evening Red Dress",
                ArabicName = "فستان سهرة أحمر",
                Price = 1500m,
                SubCategoryId = 1,
                SubCategory = subCat,
                Status = ProductStatus.Available,
                Variants = new List<ProductVariant> { new() { ColorId = 1, Quantity = 5 } }
            },
            new Product
            {
                Id = 2,
                Name = "Casual Top",
                ArabicName = "بلوزة كاجوال",
                Price = 300m,
                SubCategoryId = 1,
                SubCategory = subCat,
                Status = ProductStatus.OutOfStock,
                Variants = new List<ProductVariant> { new() { ColorId = 1, Quantity = 0 } }
            }
        );
        await context.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(context);

        var searchResult = await handler.Handle(new GetProductsQuery(Search: "Evening"), CancellationToken.None);
        searchResult.Items.Should().HaveCount(1);
        searchResult.Items[0].Name.Should().Be("Evening Red Dress");

        var inStockResult = await handler.Handle(new GetProductsQuery(OnlyInStock: true), CancellationToken.None);
        inStockResult.Items.Should().HaveCount(1);

        var priceFiltered = await handler.Handle(new GetProductsQuery(MinPrice: 1000m), CancellationToken.None);
        priceFiltered.Items.Should().HaveCount(1);
        priceFiltered.Items[0].Price.Should().Be(1500m);
    }

    [Fact]
    public async Task GetColorsQueryHandler_ShouldReturnAllColors()
    {
        using var context = GetInMemoryDbContext();
        context.Colors.AddRange(
            new Color { Name = "Black", ArabicName = "أسود", HexCode = "#000" },
            new Color { Name = "White", ArabicName = "أبيض", HexCode = "#FFF" }
        );
        await context.SaveChangesAsync();

        var handler = new GetColorsQueryHandler(context);
        var colors = await handler.Handle(new GetColorsQuery(), CancellationToken.None);

        colors.Should().HaveCount(2);
    }
}
