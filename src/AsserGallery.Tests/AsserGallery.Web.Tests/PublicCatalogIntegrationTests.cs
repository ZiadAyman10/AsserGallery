using System.Net;
using FluentAssertions;
using Xunit;

namespace AsserGallery.Web.Tests;

public class PublicCatalogIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PublicCatalogIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_HomePage_ShouldReturnSuccessAndHtmlContent()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/html");

        var html = await response.Content.ReadAsStringAsync();
        html.Should().NotBeNullOrWhiteSpace();
        html.Should().Contain("Catalog");
    }

    [Fact]
    public async Task Get_Catalog_ShouldReturnProductsListing()
    {
        // Act
        var response = await _client.GetAsync("/Catalog");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("product-card");
    }

    [Fact]
    public async Task Get_ProductDetails_WithValidId_ShouldReturnDetailsPage()
    {
        // Act
        var response = await _client.GetAsync("/Catalog/Details/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("WhatsApp");
    }

    [Fact]
    public async Task Get_ProductDetails_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync("/Catalog/Details/9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ContactPage_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/Contact");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("customerName");
        html.Should().Contain("phoneNumber");
    }

    [Fact]
    public async Task Get_CultureSwitch_ShouldSetCultureCookieAndRedirect()
    {
        // Act
        var response = await _client.GetAsync("/Culture/SetLanguage?culture=en&returnUrl=/Catalog");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Contains("Set-Cookie").Should().BeTrue();
    }
}
