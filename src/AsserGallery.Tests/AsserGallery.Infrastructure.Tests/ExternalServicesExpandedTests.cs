using AsserGallery.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AsserGallery.Infrastructure.Tests;

public class ExternalServicesExpandedTests
{
    [Fact]
    public void WhatsAppLinkBuilder_EnglishLanguage_ShouldGenerateEnglishMessage()
    {
        var builder = new WhatsAppLinkBuilder();
        var link = builder.BuildOrderLink("01012345678", "Silk Dress", "Red", 900m, "https://asser.com/item/1", "en");

        link.Should().StartWith("https://wa.me/201012345678?text=");
        link.Should().Contain("Silk+Dress");
        link.Should().Contain("900+EGP");
    }

    [Fact]
    public void WhatsAppLinkBuilder_BuildDirectChatLink_WithAndWithoutMessage()
    {
        var builder = new WhatsAppLinkBuilder();

        var simpleLink = builder.BuildDirectChatLink("01012345678", null);
        simpleLink.Should().Be("https://wa.me/201012345678");

        var messageLink = builder.BuildDirectChatLink("01012345678", "Hello there");
        messageLink.Should().Be("https://wa.me/201012345678?text=Hello+there");
    }

    [Fact]
    public void FacebookGroupAssistHelper_EnglishLanguage_ShouldGenerateEnglishPost()
    {
        var helper = new FacebookGroupAssistHelper();
        var post = helper.GenerateGroupPostText(
            productName: "Classic Linen Shirt",
            price: 700m,
            discountedPrice: 550m,
            description: "100% pure linen",
            availableColors: new[] { "White", "Navy" },
            storeWhatsApp: "01099999999",
            language: "en"
        );

        post.Should().Contain("Classic Linen Shirt");
        post.Should().Contain("550 EGP");
        post.Should().Contain("White / Navy");
        post.Should().Contain("wa.me/201099999999");
    }

    [Fact]
    public void FacebookGroupAssistHelper_GetGroupWebUrl_ShouldFormatUrls()
    {
        var helper = new FacebookGroupAssistHelper();

        var directUrl = helper.GetGroupWebUrl("https://facebook.com/groups/assergalleryvip");
        directUrl.Should().Be("https://facebook.com/groups/assergalleryvip");

        var idOnly = helper.GetGroupWebUrl("987654321");
        idOnly.Should().Be("https://www.facebook.com/groups/987654321");
    }
}
