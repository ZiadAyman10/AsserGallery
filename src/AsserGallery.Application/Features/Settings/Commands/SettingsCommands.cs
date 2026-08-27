using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Settings.Commands;

public record UpdateStoreSettingsCommand(
    string StoreName,
    string StoreArabicName,
    string WhatsAppNumber,
    string MessengerUsername,
    string Currency,
    string CurrencyArabic,
    bool HideOutOfStock
) : IRequest<bool>;

public class UpdateStoreSettingsCommandHandler : IRequestHandler<UpdateStoreSettingsCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateStoreSettingsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateStoreSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _context.StoreSettings.ToListAsync(cancellationToken);
        void SetVal(string key, string val)
        {
            var item = settings.FirstOrDefault(s => s.Key == key);
            if (item != null)
            {
                item.Value = val;
            }
            else
            {
                _context.StoreSettings.Add(new StoreSetting { Key = key, Value = val });
            }
        }

        SetVal("StoreName", request.StoreName.Trim());
        SetVal("StoreArabicName", request.StoreArabicName.Trim());
        SetVal("WhatsAppNumber", request.WhatsAppNumber.Trim());
        SetVal("MessengerUsername", request.MessengerUsername.Trim());
        SetVal("Currency", request.Currency.Trim());
        SetVal("CurrencyArabic", request.CurrencyArabic.Trim());
        SetVal("HideOutOfStock", request.HideOutOfStock.ToString().ToLower());

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
