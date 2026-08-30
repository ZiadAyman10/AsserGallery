using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Facebook.Commands;
using AsserGallery.Application.Features.Finances.Commands;
using AsserGallery.Application.Features.Finances.Queries;
using AsserGallery.Application.Features.Sales.Commands;
using AsserGallery.Application.Features.Sales.Dtos;
using AsserGallery.Application.Features.Sales.Queries;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AsserGallery.Application.Tests;

public class FinanceAndSalesExpandedTests
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
    public async Task AddAndDeleteFinancialTransaction_ShouldWorkCorrectly()
    {
        using var context = GetInMemoryDbContext();
        var addHandler = new AddFinancialTransactionCommandHandler(context);

        var id = await addHandler.Handle(new AddFinancialTransactionCommand(
            Title: "Facebook Ads Campaign",
            Description: "Summer collection ads",
            Amount: 1200m,
            Type: TransactionType.Expense,
            Category: "Ads"
        ), CancellationToken.None);

        id.Should().BeGreaterThan(0);
        var tx = await context.FinancialTransactions.FindAsync(id);
        tx!.Title.Should().Be("Facebook Ads Campaign");
        tx.Amount.Should().Be(1200m);

        var deleteHandler = new DeleteFinancialTransactionCommandHandler(context);
        var deleted = await deleteHandler.Handle(new DeleteFinancialTransactionCommand(id), CancellationToken.None);
        deleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetFinancialTransactionsQuery_ShouldFilterAccurately()
    {
        using var context = GetInMemoryDbContext();
        context.FinancialTransactions.AddRange(
            new FinancialTransaction { Title = "Revenue", Amount = 5000m, Type = TransactionType.Income, Category = "Sales" },
            new FinancialTransaction { Title = "Courier Fee", Amount = 200m, Type = TransactionType.Expense, Category = "Delivery" },
            new FinancialTransaction { Title = "Paper Bags", Amount = 300m, Type = TransactionType.Expense, Category = "Packaging" }
        );
        await context.SaveChangesAsync();

        var handler = new GetFinancialTransactionsQueryHandler(context);

        var expensesOnly = await handler.Handle(new GetFinancialTransactionsQuery(Type: TransactionType.Expense), CancellationToken.None);
        expensesOnly.Should().HaveCount(2);

        var deliveryOnly = await handler.Handle(new GetFinancialTransactionsQuery(Category: "Delivery"), CancellationToken.None);
        deliveryOnly.Should().HaveCount(1);
        deliveryOnly[0].Title.Should().Be("Courier Fee");
    }

    [Fact]
    public async Task GetSalesQuery_And_GetSaleByIdQuery_ShouldReturnSalesData()
    {
        using var context = GetInMemoryDbContext();
        var sale = new Sale
        {
            Id = 1,
            SaleNumber = "INV-TEST-001",
            CustomerName = "Kareem",
            CustomerPhone = "01011112222",
            SaleDate = DateTime.UtcNow,
            TotalAmount = 850m
        };
        context.Sales.Add(sale);
        await context.SaveChangesAsync();

        var listHandler = new GetSalesQueryHandler(context);
        var salesList = await listHandler.Handle(new GetSalesQuery(Search: "Kareem"), CancellationToken.None);
        salesList.Should().HaveCount(1);
        salesList[0].SaleNumber.Should().Be("INV-TEST-001");

        var detailsHandler = new GetSaleByIdQueryHandler(context);
        var singleSale = await detailsHandler.Handle(new GetSaleByIdQuery(1), CancellationToken.None);
        singleSale.Should().NotBeNull();
        singleSale!.CustomerName.Should().Be("Kareem");
    }

    [Fact]
    public async Task PublishToFacebookPageCommandHandler_WithMockPublisher_ShouldRecordPost()
    {
        using var context = GetInMemoryDbContext();
        var dest = new FacebookDestination
        {
            Id = 1,
            Name = "Official Page",
            DestinationType = DestinationType.Page,
            TargetIdOrUrl = "100200300",
            AccessToken = "EAAB123"
        };
        var product = new Product { Id = 1, Name = "Linen Tunic" };
        context.FacebookDestinations.Add(dest);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var mockPublisher = new Mock<IFacebookPagePublisher>();
        mockPublisher
            .Setup(p => p.PublishPostAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FacebookPublishResult(true, "FB_POST_999", null));

        var handler = new PublishToFacebookPageCommandHandler(context, mockPublisher.Object);
        var result = await handler.Handle(new PublishToFacebookPageCommand(1, 1, "New arrival linen tunic in stock!"), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.PostId.Should().Be("FB_POST_999");

        var post = await context.ProductPosts.FirstOrDefaultAsync(p => p.ProductId == 1);
        post.Should().NotBeNull();
        post!.Status.Should().Be("Published");
        post.PostUrlOrId.Should().Be("FB_POST_999");
    }
}
