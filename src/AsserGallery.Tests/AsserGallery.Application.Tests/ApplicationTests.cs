using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.CustomerRequests.Commands;
using AsserGallery.Application.Features.Dashboard.Queries;
using AsserGallery.Application.Features.Finances.Commands;
using AsserGallery.Application.Features.Finances.Queries;
using AsserGallery.Application.Features.Products.Commands;
using AsserGallery.Application.Features.Products.Dtos;
using AsserGallery.Application.Features.Products.Queries;
using AsserGallery.Application.Features.Sales.Commands;
using AsserGallery.Application.Features.Sales.Dtos;
using AsserGallery.Application.Features.Sales.Queries;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
    public void CreateProductCommandValidator_ShouldFailOnEmptyNameOrInvalidPrice()
    {
        var validator = new CreateProductCommandValidator();
        var invalidCommand = new CreateProductCommand(
            Name: "",
            ArabicName: "",
            Description: null,
            ArabicDescription: null,
            Price: 0m,
            DiscountedPrice: null,
            SubCategoryId: 0,
            IsFeatured: false,
            DisplayOrder: 0,
            Variants: new List<CreateProductVariantInput>(),
            Images: new List<CreateProductImageInput>()
        );

        var result = validator.Validate(invalidCommand);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "ArabicName");
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
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
    public void RegisterSaleCommandValidator_ShouldFailOnEmptyItems()
    {
        var validator = new RegisterSaleCommandValidator();
        var invalidCommand = new RegisterSaleCommand(
            CustomerName: null,
            CustomerPhone: null,
            Notes: null,
            SaleDate: DateTime.UtcNow,
            Items: new List<CreateSaleItemInput>()
        );

        var result = validator.Validate(invalidCommand);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Items");
    }

    [Fact]
    public async Task SubmitCustomerRequestCommandHandler_ShouldPersistCustomerInquiry()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var handler = new SubmitCustomerRequestCommandHandler(context);

        var command = new SubmitCustomerRequestCommand(
            CustomerName: "Sara Hassan",
            PhoneNumber: "01123456789",
            PreferredChannel: ContactChannel.WhatsApp,
            Message: "Is the red dress available in size Large?",
            ProductId: 1
        );

        // Act
        var requestId = await handler.Handle(command, CancellationToken.None);

        // Assert
        requestId.Should().BeGreaterThan(0);
        var request = await context.CustomerRequests.FindAsync(requestId);
        request.Should().NotBeNull();
        request!.CustomerName.Should().Be("Sara Hassan");
        request.Status.Should().Be(CustomerRequestStatus.New);
    }

    [Fact]
    public void SubmitCustomerRequestCommandValidator_ShouldValidateRequiredFields()
    {
        var validator = new SubmitCustomerRequestCommandValidator();
        var invalid = new SubmitCustomerRequestCommand("", "", ContactChannel.WhatsApp, null, null);

        var result = validator.Validate(invalid);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerName");
        result.Errors.Should().Contain(e => e.PropertyName == "PhoneNumber");
    }

    [Fact]
    public void AddFinancialTransactionCommandValidator_ShouldValidateAmountAndTitle()
    {
        var validator = new AddFinancialTransactionCommandValidator();
        var invalid = new AddFinancialTransactionCommand("", null, 0m, TransactionType.Expense, "", DateTime.UtcNow, null);

        var result = validator.Validate(invalid);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
        result.Errors.Should().Contain(e => e.PropertyName == "Amount");
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

    [Fact]
    public async Task GetDashboardSummaryQueryHandler_ShouldCalculateMonthlyTrendsAndTopSelling()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var cat = new Category { Id = 1, Name = "Men", ArabicName = "رجالي" };
        var subCat = new SubCategory { Id = 1, CategoryId = 1, Category = cat, Name = "Shirts", ArabicName = "قمصان" };
        context.Categories.Add(cat);
        context.SubCategories.Add(subCat);

        var product = new Product
        {
            Id = 1,
            Name = "Oxford Shirt",
            ArabicName = "قميص أكسفورد",
            Price = 600m,
            SubCategoryId = 1,
            SubCategory = subCat,
            Status = ProductStatus.Available,
            Variants = new List<ProductVariant>
            {
                new() { Id = 1, Quantity = 10 }
            }
        };
        context.Products.Add(product);

        var saleItem = new SaleItem { Id = 1, SaleId = 1, ProductId = 1, Quantity = 2, UnitPrice = 600m };
        var sale = new Sale
        {
            Id = 1,
            SaleNumber = "INV-001",
            SaleDate = DateTime.UtcNow,
            TotalAmount = 1200m,
            Items = new List<SaleItem> { saleItem }
        };
        context.Sales.Add(sale);
        context.SaleItems.Add(saleItem);
        await context.SaveChangesAsync();

        var handler = new GetDashboardSummaryQueryHandler(context);

        // Act
        var result = await handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);

        // Assert
        result.TotalProductsCount.Should().Be(1);
        result.TotalRevenue.Should().Be(1200m);
        result.MonthlyTrends.Should().HaveCount(6);
        result.CategoryBreakdowns.Should().NotBeEmpty();
        result.CategoryBreakdowns[0].CategoryName.Should().Be("Men");
        result.TopSellingProducts.Should().HaveCount(1);
        result.TopSellingProducts[0].ProductName.Should().Be("Oxford Shirt");
        result.TopSellingProducts[0].QuantitySold.Should().Be(2);
    }

    [Fact]
    public async Task ExportSalesQueryHandler_ShouldGenerateCsvBytesWithBom()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var product = new Product { Id = 1, Name = "Silk Blouse", Price = 750m };
        context.Products.Add(product);

        context.Sales.Add(new Sale
        {
            Id = 1,
            SaleNumber = "INV-EXPORT-01",
            CustomerName = "Mona",
            CustomerPhone = "01099999999",
            SaleDate = DateTime.UtcNow,
            TotalAmount = 750m,
            Items = new List<SaleItem>
            {
                new() { ProductId = 1, Product = product, Quantity = 1, UnitPrice = 750m }
            }
        });
        await context.SaveChangesAsync();

        var handler = new ExportSalesQueryHandler(context);

        // Act
        var export = await handler.Handle(new ExportSalesQuery(), CancellationToken.None);

        // Assert
        export.Should().NotBeNull();
        export.ContentType.Should().Contain("text/csv");
        export.FileName.Should().StartWith("AsserGallery_Sales_");
        export.Content.Length.Should().BeGreaterThan(0);

        var csvString = System.Text.Encoding.UTF8.GetString(export.Content);
        csvString.Should().Contain("Invoice Number");
        csvString.Should().Contain("INV-EXPORT-01");
        csvString.Should().Contain("Mona");
    }

    [Fact]
    public async Task ExportFinancesQueryHandler_ShouldGenerateFinancialCsvSummary()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        context.FinancialTransactions.Add(new FinancialTransaction
        {
            Title = "Delivery Courier Expense",
            Amount = 150m,
            Type = TransactionType.Expense,
            Category = "Delivery",
            Date = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new ExportFinancesQueryHandler(context);

        // Act
        var export = await handler.Handle(new ExportFinancesQuery(), CancellationToken.None);

        // Assert
        export.Should().NotBeNull();
        export.FileName.Should().StartWith("AsserGallery_Finances_");
        var csvString = System.Text.Encoding.UTF8.GetString(export.Content);
        csvString.Should().Contain("Delivery Courier Expense");
        csvString.Should().Contain("Total Expenses");
    }

    [Fact]
    public async Task ExportInventoryQueryHandler_ShouldGenerateInventoryCsv()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var cat = new Category { Id = 1, Name = "Women", ArabicName = "حريمي" };
        var sub = new SubCategory { Id = 1, CategoryId = 1, Category = cat, Name = "Skirts", ArabicName = "جيبات" };
        var color = new Color { Id = 1, Name = "Blue", ArabicName = "أزرق" };
        context.Categories.Add(cat);
        context.SubCategories.Add(sub);
        context.Colors.Add(color);

        context.Products.Add(new Product
        {
            Id = 1,
            Name = "Pleated Skirt",
            ArabicName = "جيبة بليسيه",
            Price = 400m,
            SubCategoryId = 1,
            SubCategory = sub,
            Status = ProductStatus.Available,
            Variants = new List<ProductVariant>
            {
                new() { ColorId = 1, Color = color, Quantity = 12 }
            }
        });
        await context.SaveChangesAsync();

        var handler = new ExportInventoryQueryHandler(context);

        // Act
        var export = await handler.Handle(new ExportInventoryQuery(), CancellationToken.None);

        // Assert
        export.Should().NotBeNull();
        export.FileName.Should().StartWith("AsserGallery_Inventory_");
        var csvString = System.Text.Encoding.UTF8.GetString(export.Content);
        csvString.Should().Contain("Pleated Skirt");
        csvString.Should().Contain("Total Inventory Units on Hand");
    }
}
