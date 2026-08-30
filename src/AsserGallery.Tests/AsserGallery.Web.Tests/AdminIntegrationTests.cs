using System.Net;
using FluentAssertions;
using Xunit;

namespace AsserGallery.Web.Tests;

public class AdminIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_AdminDashboard_WithoutLogin_ShouldRedirectToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/Admin/Account/Login");
    }

    [Fact]
    public async Task Get_AdminProducts_WithoutLogin_ShouldRedirectToLogin()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.ToString().Should().Contain("/Admin/Account/Login");
    }

    [Fact]
    public async Task Get_AdminAccountLogin_ShouldReturnSuccess()
    {
        // Act
        var response = await _client.GetAsync("/Admin/Account/Login");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("admin@assergallery.com");
        html.Should().Contain("name=\"email\"");
    }

    [Fact]
    public async Task Get_ApiProducts_ShouldReturnJsonList()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("items");
        json.Should().Contain("totalCount");
    }
}
