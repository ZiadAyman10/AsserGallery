using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Domain.Entities;
using AsserGallery.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Product Configuration
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.ArabicName).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.DiscountedPrice).HasPrecision(18, 2);

            entity.HasOne(p => p.SubCategory)
                  .WithMany(sc => sc.Products)
                  .HasForeignKey(p => p.SubCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(p => p.Images)
                  .WithOne(i => i.Product)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Variants)
                  .WithOne(v => v.Product)
                  .HasForeignKey(v => v.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Category Configuration
        builder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.ArabicName).IsRequired().HasMaxLength(100);
        });

        builder.Entity<SubCategory>(entity =>
        {
            entity.HasKey(sc => sc.Id);
            entity.Property(sc => sc.Name).IsRequired().HasMaxLength(100);
            entity.Property(sc => sc.ArabicName).IsRequired().HasMaxLength(100);

            entity.HasOne(sc => sc.Category)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(sc => sc.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Color Configuration
        builder.Entity<Color>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(50);
            entity.Property(c => c.ArabicName).IsRequired().HasMaxLength(50);
            entity.Property(c => c.HexCode).IsRequired().HasMaxLength(10);
        });

        // ProductVariant Configuration
        builder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(pv => pv.Id);
            entity.HasOne(pv => pv.Color)
                  .WithMany(c => c.Variants)
                  .HasForeignKey(pv => pv.ColorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Sale Configuration
        builder.Entity<Sale>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);
            entity.Property(s => s.TotalAmount).HasPrecision(18, 2);

            entity.HasMany(s => s.Items)
                  .WithOne(i => i.Sale)
                  .HasForeignKey(i => i.SaleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(si => si.Id);
            entity.Property(si => si.UnitPrice).HasPrecision(18, 2);

            entity.HasOne(si => si.Product)
                  .WithMany()
                  .HasForeignKey(si => si.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(si => si.ProductVariant)
                  .WithMany()
                  .HasForeignKey(si => si.ProductVariantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // FinancialTransaction Configuration
        builder.Entity<FinancialTransaction>(entity =>
        {
            entity.HasKey(ft => ft.Id);
            entity.Property(ft => ft.Title).IsRequired().HasMaxLength(150);
            entity.Property(ft => ft.Amount).HasPrecision(18, 2);
            entity.Property(ft => ft.Category).IsRequired().HasMaxLength(50);

            entity.HasOne(ft => ft.LinkedProduct)
                  .WithMany(p => p.FinancialTransactions)
                  .HasForeignKey(ft => ft.LinkedProductId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // CustomerRequest Configuration
        builder.Entity<CustomerRequest>(entity =>
        {
            entity.HasKey(cr => cr.Id);
            entity.Property(cr => cr.CustomerName).IsRequired().HasMaxLength(100);
            entity.Property(cr => cr.PhoneNumber).IsRequired().HasMaxLength(30);

            entity.HasOne(cr => cr.Product)
                  .WithMany()
                  .HasForeignKey(cr => cr.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // FacebookDestination & ProductPost Configuration
        builder.Entity<FacebookDestination>(entity =>
        {
            entity.HasKey(fd => fd.Id);
            entity.Property(fd => fd.Name).IsRequired().HasMaxLength(100);
            entity.Property(fd => fd.TargetIdOrUrl).IsRequired().HasMaxLength(255);
        });

        builder.Entity<ProductPost>(entity =>
        {
            entity.HasKey(pp => pp.Id);
            entity.HasOne(pp => pp.Product)
                  .WithMany(p => p.Posts)
                  .HasForeignKey(pp => pp.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pp => pp.FacebookDestination)
                  .WithMany(d => d.Posts)
                  .HasForeignKey(pp => pp.FacebookDestinationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // StoreSetting Configuration
        builder.Entity<StoreSetting>(entity =>
        {
            entity.HasKey(ss => ss.Id);
            entity.Property(ss => ss.Key).IsRequired().HasMaxLength(100);
            entity.HasIndex(ss => ss.Key).IsUnique();
        });
    }
}
