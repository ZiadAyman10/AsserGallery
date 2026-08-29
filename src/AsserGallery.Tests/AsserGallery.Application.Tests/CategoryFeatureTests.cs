using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Categories.Commands;
using AsserGallery.Application.Features.Categories.Queries;
using AsserGallery.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AsserGallery.Application.Tests;

public class CategoryFeatureTests
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
    public async Task CreateCategoryCommandHandler_ShouldCreateCategory()
    {
        using var context = GetInMemoryDbContext();
        var handler = new CreateCategoryCommandHandler(context);

        var command = new CreateCategoryCommand(
            Name: "Kids",
            ArabicName: "أطفال",
            Description: "Kids clothing collection",
            ArabicDescription: "ملابس أطفال",
            ImageUrl: "/images/kids.jpg",
            DisplayOrder: 3
        );

        var id = await handler.Handle(command, CancellationToken.None);

        id.Should().BeGreaterThan(0);
        var created = await context.Categories.FindAsync(id);
        created.Should().NotBeNull();
        created!.Name.Should().Be("Kids");
        created.ArabicName.Should().Be("أطفال");
        created.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateCategoryCommandValidator_ShouldValidateFields()
    {
        var validator = new CreateCategoryCommandValidator();
        var invalid = new CreateCategoryCommand("", "", null, null, null, 0);

        var result = validator.Validate(invalid);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "ArabicName");
    }

    [Fact]
    public async Task DeleteCategoryCommandHandler_ShouldRemoveCategory()
    {
        using var context = GetInMemoryDbContext();
        var cat = new Category { Name = "To Delete", ArabicName = "للحذف", IsActive = true };
        context.Categories.Add(cat);
        await context.SaveChangesAsync();

        var handler = new DeleteCategoryCommandHandler(context);
        var success = await handler.Handle(new DeleteCategoryCommand(cat.Id), CancellationToken.None);

        success.Should().BeTrue();
        var deleted = await context.Categories.FindAsync(cat.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetCategoriesQueryHandler_ShouldReturnCategories()
    {
        using var context = GetInMemoryDbContext();
        context.Categories.AddRange(
            new Category { Name = "Active Cat", ArabicName = "نشط", IsActive = true, DisplayOrder = 1 },
            new Category { Name = "Inactive Cat", ArabicName = "غير نشط", IsActive = false, DisplayOrder = 2 }
        );
        await context.SaveChangesAsync();

        var handler = new GetCategoriesQueryHandler(context);

        var all = await handler.Handle(new GetCategoriesQuery(OnlyActive: false), CancellationToken.None);
        all.Should().HaveCount(2);

        var activeOnly = await handler.Handle(new GetCategoriesQuery(OnlyActive: true), CancellationToken.None);
        activeOnly.Should().HaveCount(1);
        activeOnly[0].Name.Should().Be("Active Cat");
    }

    [Fact]
    public async Task CreateSubCategoryCommandHandler_ShouldCreateSubCategory()
    {
        using var context = GetInMemoryDbContext();
        var cat = new Category { Name = "Men", ArabicName = "رجالي", IsActive = true };
        context.Categories.Add(cat);
        await context.SaveChangesAsync();

        var handler = new CreateSubCategoryCommandHandler(context);
        var command = new CreateSubCategoryCommand(
            CategoryId: cat.Id,
            Name: "Suits",
            ArabicName: "بدل",
            Description: "Formal suits",
            ArabicDescription: "بدل رسمية",
            DisplayOrder: 1
        );

        var subId = await handler.Handle(command, CancellationToken.None);

        subId.Should().BeGreaterThan(0);
        var created = await context.SubCategories.FindAsync(subId);
        created.Should().NotBeNull();
        created!.CategoryId.Should().Be(cat.Id);
        created.Name.Should().Be("Suits");
    }
}
