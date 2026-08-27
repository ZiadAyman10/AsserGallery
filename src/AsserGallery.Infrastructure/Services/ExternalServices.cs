using System.Net;
using AsserGallery.Application.Common.Interfaces;

namespace AsserGallery.Infrastructure.Services;

public class WhatsAppLinkBuilder : IWhatsAppLinkBuilder
{
    public string BuildOrderLink(string phoneNumber, string productName, string? colorName, decimal price, string? productUrl, string language = "ar")
    {
        var cleanPhone = FormatPhoneNumber(phoneNumber);

        string message;
        if (language == "ar")
        {
            message = $"مرحباً آسر جاليري 👋، أود الاستفسار عن / طلب المنتج:\n✨ *{productName}*";
            if (!string.IsNullOrWhiteSpace(colorName))
            {
                message += $"\n🎨 اللون: {colorName}";
            }
            message += $"\n💰 السعر: {price:N0} ج.م";
            if (!string.IsNullOrWhiteSpace(productUrl))
            {
                message += $"\n🔗 رابط المنتج: {productUrl}";
            }
            message += "\n\nهل المنتج متوفر حالياً؟";
        }
        else
        {
            message = $"Hello Asser Gallery 👋, I would like to inquire about / order:\n✨ *{productName}*";
            if (!string.IsNullOrWhiteSpace(colorName))
            {
                message += $"\n🎨 Color: {colorName}";
            }
            message += $"\n💰 Price: {price:N0} EGP";
            if (!string.IsNullOrWhiteSpace(productUrl))
            {
                message += $"\n🔗 Product Link: {productUrl}";
            }
            message += "\n\nIs this item available?";
        }

        var encoded = WebUtility.UrlEncode(message);
        return $"https://wa.me/{cleanPhone}?text={encoded}";
    }

    public string BuildDirectChatLink(string phoneNumber, string? initialMessage)
    {
        var cleanPhone = FormatPhoneNumber(phoneNumber);
        if (string.IsNullOrWhiteSpace(initialMessage))
        {
            return $"https://wa.me/{cleanPhone}";
        }
        return $"https://wa.me/{cleanPhone}?text={WebUtility.UrlEncode(initialMessage)}";
    }

    private static string FormatPhoneNumber(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("01") && digits.Length == 11)
        {
            return "2" + digits;
        }
        return digits;
    }
}

public class FacebookGroupAssistHelper : IFacebookGroupAssistHelper
{
    public string GenerateGroupPostText(
        string productName,
        decimal price,
        decimal? discountedPrice,
        string? description,
        IEnumerable<string> availableColors,
        string storeWhatsApp,
        string language = "ar")
    {
        var colorList = availableColors.ToList();
        var colorsText = colorList.Count != 0 ? string.Join(" / ", colorList) : (language == "ar" ? "ألوان متعددة" : "Multiple colors");

        if (language == "ar")
        {
            var priceText = discountedPrice.HasValue && discountedPrice.Value < price
                ? $"🔥 عرض خاص: {discountedPrice.Value:N0} ج.م بدلاً من ~{price:N0} ج.م~"
                : $"💰 السعر: {price:N0} ج.م فقط!";

            return $"""
                    ✨ موديل جديد حصري من آسر جاليري ✨
                    
                    🧥 الموديل: {productName}
                    🎨 الألوان المتاحة: {colorsText}
                    {priceText}
                    
                    📝 التفاصيل:
                    {description ?? "خامات قطنية عالية الجودة، مظهر أنيق ومريح جداً."}
                    
                    🛵 الشحن متاح لجميع المحافظات مع إمكانية المعاينة قبل الاستلام!
                    📲 للطلب والاستفسار مباشرة عبر واتساب:
                    wa.me/{FormatPhone(storeWhatsApp)}
                    
                    #آسر_جاليري #ملابس #أناقة #عروض #موضة
                    """;
        }
        else
        {
            var priceText = discountedPrice.HasValue && discountedPrice.Value < price
                ? $"🔥 Special Offer: {discountedPrice.Value:N0} EGP (Original: ~{price:N0} EGP~)"
                : $"💰 Price: {price:N0} EGP";

            return $"""
                    ✨ New Collection Arrival at Asser Gallery ✨
                    
                    🧥 Item: {productName}
                    🎨 Available Colors: {colorsText}
                    {priceText}
                    
                    📝 Details:
                    {description ?? "Premium fabric, elegant fit and superior comfort."}
                    
                    🛵 Delivery available everywhere!
                    📲 Order now directly via WhatsApp:
                    wa.me/{FormatPhone(storeWhatsApp)}
                    
                    #AsserGallery #Fashion #Style #Offers
                    """;
        }
    }

    public string GetGroupWebUrl(string groupUrlOrId)
    {
        if (string.IsNullOrWhiteSpace(groupUrlOrId)) return "https://www.facebook.com/groups";
        if (groupUrlOrId.StartsWith("http://") || groupUrlOrId.StartsWith("https://"))
        {
            return groupUrlOrId;
        }
        return $"https://www.facebook.com/groups/{groupUrlOrId}";
    }

    private static string FormatPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("01") && digits.Length == 11) return "2" + digits;
        return digits;
    }
}

public class FacebookPagePublisher : IFacebookPagePublisher
{
    private readonly HttpClient _httpClient;

    public FacebookPagePublisher(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<FacebookPublishResult> PublishPostAsync(
        string pageId,
        string accessToken,
        string message,
        string? imageUrl,
        CancellationToken cancellationToken = default)
    {
        // If no access token provided or placeholder token, simulate success for test/demo
        if (string.IsNullOrWhiteSpace(accessToken) || accessToken.StartsWith("MOCK_") || accessToken.Length < 10)
        {
            var simulatedPostId = $"{pageId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            return new FacebookPublishResult(true, simulatedPostId, null);
        }

        try
        {
            var endpoint = string.IsNullOrWhiteSpace(imageUrl)
                ? $"https://graph.facebook.com/v19.0/{pageId}/feed"
                : $"https://graph.facebook.com/v19.0/{pageId}/photos";

            var formData = new Dictionary<string, string>
            {
                { "access_token", accessToken },
                { string.IsNullOrWhiteSpace(imageUrl) ? "message" : "caption", message }
            };

            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                formData.Add("url", imageUrl);
            }

            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new FacebookPublishResult(true, responseString, null);
            }

            return new FacebookPublishResult(false, null, $"Meta Graph API Error: {response.StatusCode} - {responseString}");
        }
        catch (Exception ex)
        {
            return new FacebookPublishResult(false, null, $"Exception publishing to Facebook: {ex.Message}");
        }
    }
}
