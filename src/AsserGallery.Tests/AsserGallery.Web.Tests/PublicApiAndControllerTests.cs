using System.Net;
using FluentAssertions;
using Xunit;

namespace AsserGallery.Web.Tests;

public class PublicApiAndControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PublicApiAndControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_ApiProductById_WithValidId_ShouldReturnProduct()
    {
        var response = await _client.GetAsync("/api/products/1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("id");
        json.Should().Contain("price");
    }

    [Fact]
    public async Task Get_ApiProductById_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/products/99999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ApiColors_ShouldReturnColorsList()
    {
        var response = await _client.GetAsync("/api/products/colors");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("hexCode");
    }

    [Fact]
    public async Task Get_Catalog_WithCategoryFilter_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/Catalog?categoryId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_Catalog_WithPriceFilter_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/Catalog?minPrice=100&maxPrice=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_Contact_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/Contact");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("WhatsApp");
    }

    [Fact]
    public async Task Get_AdminLogin_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/Admin/Account/Login");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("admin@assergallery.com");
    }

    [Fact]
    public async Task Get_AdminAccessDenied_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/Admin/Account/AccessDenied");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_SetLanguage_ShouldSetCultureCookieAndRedirect()
    {
        var response = await _client.GetAsync("/Culture/SetLanguage?culture=ar&returnUrl=%2F");
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().Be("/");
        response.Headers.Should().ContainKey("Set-Cookie");
    }

    [Fact]
    public async Task Localization_ArabicCulture_ShouldRenderArabicStrings()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("Cookie", ".AspNetCore.Culture=c%3Dar%7Cuic%3Dar");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rawHtml = await response.Content.ReadAsStringAsync();
        var decodedHtml = WebUtility.HtmlDecode(rawHtml);

        decodedHtml.Should().Contain("آسر جاليري");
        decodedHtml.Should().Contain("الرئيسية");
        decodedHtml.Should().Contain("ج.م");
        decodedHtml.Should().NotContain("@Localizer");
    }

    [Fact]
    public async Task Localization_EnglishCulture_ShouldRenderEnglishStrings()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("Cookie", ".AspNetCore.Culture=c%3Den%7Cuic%3Den");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rawHtml = await response.Content.ReadAsStringAsync();
        var decodedHtml = WebUtility.HtmlDecode(rawHtml);

        decodedHtml.Should().Contain("Asser Gallery");
        decodedHtml.Should().Contain("Home");
        decodedHtml.Should().Contain("EGP");
        decodedHtml.Should().NotContain("@Localizer");
    }
}
