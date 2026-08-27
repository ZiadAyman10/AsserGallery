using AsserGallery.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Settings.Queries;

public record StoreSettingsDto(
    string StoreName,
    string StoreArabicName,
    string WhatsAppNumber,
    string MessengerUsername,
    string Currency,
    string CurrencyArabic,
    bool HideOutOfStock
);

public record GetStoreSettingsQuery : IRequest<StoreSettingsDto>;

public class GetStoreSettingsQueryHandler : IRequestHandler<GetStoreSettingsQuery, StoreSettingsDto>
{
    private readonly IApplicationDbContext _context;

    public GetStoreSettingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StoreSettingsDto> Handle(GetStoreSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.StoreSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        return new StoreSettingsDto(
            StoreName: settings.GetValueOrDefault("StoreName", "Asser Gallery"),
            StoreArabicName: settings.GetValueOrDefault("StoreArabicName", "آسر جاليري"),
            WhatsAppNumber: settings.GetValueOrDefault("WhatsAppNumber", "201000000000"),
            MessengerUsername: settings.GetValueOrDefault("MessengerUsername", "assergallery"),
            Currency: settings.GetValueOrDefault("Currency", "EGP"),
            CurrencyArabic: settings.GetValueOrDefault("CurrencyArabic", "ج.م"),
            HideOutOfStock: bool.TryParse(settings.GetValueOrDefault("HideOutOfStock", "false"), out var hide) && hide
        );
    }
}
