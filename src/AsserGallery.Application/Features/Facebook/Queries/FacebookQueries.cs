using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Facebook.Dtos;
using AsserGallery.Application.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Facebook.Queries;

public record GetFacebookDestinationsQuery : IRequest<List<FacebookDestinationDto>>;

public class GetFacebookDestinationsQueryHandler : IRequestHandler<GetFacebookDestinationsQuery, List<FacebookDestinationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFacebookDestinationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FacebookDestinationDto>> Handle(GetFacebookDestinationsQuery request, CancellationToken cancellationToken)
    {
        var destinations = await _context.FacebookDestinations
            .Include(d => d.Posts)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return destinations.Select(d => d.ToDto()).ToList();
    }
}

public record GenerateGroupPostQuery(int ProductId, int DestinationId, string Language = "ar") : IRequest<GeneratedGroupPostDto?>;

public class GenerateGroupPostQueryHandler : IRequestHandler<GenerateGroupPostQuery, GeneratedGroupPostDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IFacebookGroupAssistHelper _groupHelper;
    private readonly IWhatsAppLinkBuilder _whatsAppBuilder;

    public GenerateGroupPostQueryHandler(
        IApplicationDbContext context,
        IFacebookGroupAssistHelper groupHelper,
        IWhatsAppLinkBuilder whatsAppBuilder)
    {
        _context = context;
        _groupHelper = groupHelper;
        _whatsAppBuilder = whatsAppBuilder;
    }

    public async Task<GeneratedGroupPostDto?> Handle(GenerateGroupPostQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
                .ThenInclude(v => v.Color)
            .Include(p => p.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null) return null;

        var destination = await _context.FacebookDestinations
            .FindAsync(new object[] { request.DestinationId }, cancellationToken);

        var storeWhatsAppSetting = await _context.StoreSettings
            .FirstOrDefaultAsync(s => s.Key == "WhatsAppNumber", cancellationToken);
        var whatsAppNumber = storeWhatsAppSetting?.Value ?? "201000000000";

        var colors = product.Variants.Where(v => v.Quantity > 0 && v.Color != null)
            .Select(v => request.Language == "ar" ? v.Color!.ArabicName : v.Color!.Name)
            .ToList();

        var desc = request.Language == "ar" ? product.ArabicDescription ?? product.Description : product.Description;
        var name = request.Language == "ar" ? product.ArabicName : product.Name;

        var formattedText = _groupHelper.GenerateGroupPostText(
            productName: name,
            price: product.Price,
            discountedPrice: product.DiscountedPrice,
            description: desc,
            availableColors: colors,
            storeWhatsApp: whatsAppNumber,
            language: request.Language
        );

        var targetUrl = destination != null ? _groupHelper.GetGroupWebUrl(destination.TargetIdOrUrl) : null;
        var orderLink = _whatsAppBuilder.BuildOrderLink(whatsAppNumber, name, null, product.DiscountedPrice ?? product.Price, null, request.Language);

        return new GeneratedGroupPostDto(
            ProductId: product.Id,
            ProductName: name,
            FormattedText: formattedText,
            TargetGroupUrl: targetUrl,
            StoreWhatsAppLink: orderLink
        );
    }
}
