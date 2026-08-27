using System.Text;
using AsserGallery.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AsserGallery.Infrastructure.Tests;

public class InfrastructureTests
{
    [Fact]
    public void WhatsAppLinkBuilder_ShouldGenerateValidOrderLink_Arabic()
    {
        // Arrange
        var builder = new WhatsAppLinkBuilder();

        // Act
        var link = builder.BuildOrderLink(
            phoneNumber: "01012345678",
            productName: "قميص كتان",
            colorName: "أبيض",
            price: 450m,
            productUrl: "https://assergallery.com/catalog/details/1",
            language: "ar"
        );

        // Assert
        link.Should().StartWith("https://wa.me/201012345678?text=");
        link.Should().Contain("%D9%82%D9%85%D9%8A%D8%B5"); // URL encoded Arabic
    }

    [Fact]
    public void FacebookGroupAssistHelper_ShouldFormatPost_WithAllDetails()
    {
        // Arrange
        var helper = new FacebookGroupAssistHelper();
        var colors = new[] { "أسود", "أبيض", "كحلي" };

        // Act
        var post = helper.GenerateGroupPostText(
            productName: "سويت شيرت قطن",
            price: 500m,
            discountedPrice: 400m,
            description: "خامة ممتازة ومريحة",
            availableColors: colors,
            storeWhatsApp: "01012345678",
            language: "ar"
        );

        // Assert
        post.Should().Contain("سويت شيرت قطن");
        post.Should().Contain("400");
        post.Should().Contain("أسود / أبيض / كحلي");
        post.Should().Contain("wa.me/201012345678");
    }

    [Fact]
    public async Task LocalImageStorageService_ShouldSaveAndDeleteFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var wwwrootDir = Path.Combine(tempDir, "wwwroot");
        Directory.CreateDirectory(wwwrootDir);

        var service = new LocalImageStorageService(wwwrootDir);
        var bytes = Encoding.UTF8.GetBytes("fake image data");
        using var stream = new MemoryStream(bytes);

        // Act - Save
        var savedUrl = await service.SaveImageAsync(stream, "sample.jpg", "products");

        // Assert - Save
        savedUrl.Should().StartWith("/uploads/products/");
        var localFilePath = Path.Combine(wwwrootDir, savedUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        File.Exists(localFilePath).Should().BeTrue();

        // Act - Delete
        await service.DeleteImageAsync(savedUrl);

        // Assert - Delete
        File.Exists(localFilePath).Should().BeFalse();

        // Cleanup
        try { Directory.Delete(tempDir, true); } catch { }
    }
}
