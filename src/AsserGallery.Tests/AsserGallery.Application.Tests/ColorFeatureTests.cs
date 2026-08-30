using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Colors.Commands;
using AsserGallery.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AsserGallery.Application.Tests;

public class ColorFeatureTests
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
    public async Task CreateColorCommandHandler_ShouldCreateColor()
    {
        using var context = GetInMemoryDbContext();
        var handler = new CreateColorCommandHandler(context);

        var command = new CreateColorCommand("Crimson Red", "أحمر قرمزي", "#DC2626");
        var id = await handler.Handle(command, CancellationToken.None);

        id.Should().BeGreaterThan(0);
        var created = await context.Colors.FindAsync(id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Crimson Red");
        created.ArabicName.Should().Be("أحمر قرمزي");
        created.HexCode.Should().Be("#DC2626");
    }

    [Fact]
    public void CreateColorCommandValidator_ShouldValidateHexCode()
    {
        var validator = new CreateColorCommandValidator();
        
        var valid = new CreateColorCommand("Blue", "أزرق", "#0000FF");
        validator.Validate(valid).IsValid.Should().BeTrue();

        var invalidHex = new CreateColorCommand("Blue", "أزرق", "INVALID");
        validator.Validate(invalidHex).IsValid.Should().BeFalse();

        var emptyName = new CreateColorCommand("", "", "#0000FF");
        validator.Validate(emptyName).IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateColorCommandHandler_ShouldUpdateExistingColor()
    {
        using var context = GetInMemoryDbContext();
        var color = new Color { Name = "Old Name", ArabicName = "قديم", HexCode = "#111111" };
        context.Colors.Add(color);
        await context.SaveChangesAsync();

        var handler = new UpdateColorCommandHandler(context);
        var success = await handler.Handle(new UpdateColorCommand(color.Id, "Updated Black", "أسود معدل", "#000000"), CancellationToken.None);

        success.Should().BeTrue();
        var updated = await context.Colors.FindAsync(color.Id);
        updated!.Name.Should().Be("Updated Black");
        updated.HexCode.Should().Be("#000000");
    }

    [Fact]
    public async Task DeleteColorCommandHandler_ShouldRemoveColor()
    {
        using var context = GetInMemoryDbContext();
        var color = new Color { Name = "To Delete", ArabicName = "حذف", HexCode = "#222222" };
        context.Colors.Add(color);
        await context.SaveChangesAsync();

        var handler = new DeleteColorCommandHandler(context);
        var success = await handler.Handle(new DeleteColorCommand(color.Id), CancellationToken.None);

        success.Should().BeTrue();
        var deleted = await context.Colors.FindAsync(color.Id);
        deleted.Should().BeNull();
    }
}
