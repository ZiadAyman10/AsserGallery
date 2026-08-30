using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.CustomerRequests.Commands;
using AsserGallery.Application.Features.CustomerRequests.Queries;
using AsserGallery.Application.Features.Facebook.Commands;
using AsserGallery.Application.Features.Facebook.Queries;
using AsserGallery.Application.Features.Settings.Commands;
using AsserGallery.Application.Features.Settings.Queries;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AsserGallery.Application.Tests;

public class CustomerRequestAndSettingsTests
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
    public async Task UpdateCustomerRequestStatus_ShouldUpdateStatusAndNotes()
    {
        using var context = GetInMemoryDbContext();
        var req = new CustomerRequest
        {
            CustomerName = "Nader",
            PhoneNumber = "01000000000",
            Status = CustomerRequestStatus.New
        };
        context.CustomerRequests.Add(req);
        await context.SaveChangesAsync();

        var handler = new UpdateCustomerRequestStatusCommandHandler(context);
        var success = await handler.Handle(new UpdateCustomerRequestStatusCommand(req.Id, CustomerRequestStatus.Contacted, "Called and customer confirmed order"), CancellationToken.None);

        success.Should().BeTrue();
        var updated = await context.CustomerRequests.FindAsync(req.Id);
        updated!.Status.Should().Be(CustomerRequestStatus.Contacted);
        updated.AdminNotes.Should().Be("Called and customer confirmed order");
    }

    [Fact]
    public async Task GetCustomerRequestsQuery_ShouldFilterByStatus()
    {
        using var context = GetInMemoryDbContext();
        context.CustomerRequests.AddRange(
            new CustomerRequest { CustomerName = "Req 1", PhoneNumber = "011", Status = CustomerRequestStatus.New },
            new CustomerRequest { CustomerName = "Req 2", PhoneNumber = "012", Status = CustomerRequestStatus.Completed }
        );
        await context.SaveChangesAsync();

        var handler = new GetCustomerRequestsQueryHandler(context);
        var newReqs = await handler.Handle(new GetCustomerRequestsQuery(Status: CustomerRequestStatus.New), CancellationToken.None);

        newReqs.Should().HaveCount(1);
        newReqs[0].CustomerName.Should().Be("Req 1");
    }

    [Fact]
    public async Task StoreSettings_ShouldGetAndUpdateCorrectly()
    {
        using var context = GetInMemoryDbContext();
        context.StoreSettings.AddRange(
            new StoreSetting { Key = "StoreName", Value = "Asser Gallery" },
            new StoreSetting { Key = "WhatsAppNumber", Value = "201012345678" }
        );
        await context.SaveChangesAsync();

        var getHandler = new GetStoreSettingsQueryHandler(context);
        var settings = await getHandler.Handle(new GetStoreSettingsQuery(), CancellationToken.None);

        settings.StoreName.Should().Be("Asser Gallery");
        settings.WhatsAppNumber.Should().Be("201012345678");

        var updateHandler = new UpdateStoreSettingsCommandHandler(context);
        var success = await updateHandler.Handle(new UpdateStoreSettingsCommand(
            StoreName: "Asser Luxury",
            StoreArabicName: "آسر للملابس الراقية",
            WhatsAppNumber: "201111111111",
            MessengerUsername: "assergallery.eg",
            Currency: "EGP",
            CurrencyArabic: "ج.م",
            HideOutOfStock: true
        ), CancellationToken.None);

        success.Should().BeTrue();
        var updated = await getHandler.Handle(new GetStoreSettingsQuery(), CancellationToken.None);
        updated.StoreName.Should().Be("Asser Luxury");
        updated.HideOutOfStock.Should().BeTrue();
    }

    [Fact]
    public async Task FacebookDestinations_ShouldManagePagesAndGroups()
    {
        using var context = GetInMemoryDbContext();
        var createHandler = new CreateFacebookDestinationCommandHandler(context);
        var pageId = await createHandler.Handle(new CreateFacebookDestinationCommand(
            Name: "Official VIP Page",
            DestinationType: DestinationType.Page,
            TargetIdOrUrl: "123456",
            AccessToken: "EAAB...token"
        ), CancellationToken.None);

        pageId.Should().BeGreaterThan(0);
        var dest = await context.FacebookDestinations.FindAsync(pageId);
        dest!.Name.Should().Be("Official VIP Page");
        dest.DestinationType.Should().Be(DestinationType.Page);

        var getHandler = new GetFacebookDestinationsQueryHandler(context);
        var list = await getHandler.Handle(new GetFacebookDestinationsQuery(), CancellationToken.None);
        list.Should().HaveCount(1);

        // Record a group post
        var recordPostHandler = new LogGroupPostConfirmationCommandHandler(context);
        var postId = await recordPostHandler.Handle(new LogGroupPostConfirmationCommand(
            ProductId: 1,
            DestinationId: pageId,
            PostContent: "Check out our newest linen shirt!",
            PostUrl: "https://facebook.com/groups/123",
            Notes: "Posted manually"
        ), CancellationToken.None);

        postId.Should().BeGreaterThan(0);
        var post = await context.ProductPosts.FindAsync(postId);
        post.Should().NotBeNull();
        post!.Status.Should().Be("ConfirmedByUser");
    }
}
