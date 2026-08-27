using AsserGallery.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<Category> Categories { get; }
    DbSet<SubCategory> SubCategories { get; }
    DbSet<Color> Colors { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleItem> SaleItems { get; }
    DbSet<FinancialTransaction> FinancialTransactions { get; }
    DbSet<CustomerRequest> CustomerRequests { get; }
    DbSet<FacebookDestination> FacebookDestinations { get; }
    DbSet<ProductPost> ProductPosts { get; }
    DbSet<StoreSetting> StoreSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IImageStorageService
{
    Task<string> SaveImageAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default);
}

public interface IWhatsAppLinkBuilder
{
    string BuildOrderLink(string phoneNumber, string productName, string? colorName, decimal price, string? productUrl, string language = "ar");
    string BuildDirectChatLink(string phoneNumber, string? initialMessage);
}

public record FacebookPublishResult(bool Success, string? PostId, string? ErrorMessage);

public interface IFacebookPagePublisher
{
    Task<FacebookPublishResult> PublishPostAsync(string pageId, string accessToken, string message, string? imageUrl, CancellationToken cancellationToken = default);
}

public interface IFacebookGroupAssistHelper
{
    string GenerateGroupPostText(string productName, decimal price, decimal? discountedPrice, string? description, IEnumerable<string> availableColors, string storeWhatsApp, string language = "ar");
    string GetGroupWebUrl(string groupUrlOrId);
}

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
