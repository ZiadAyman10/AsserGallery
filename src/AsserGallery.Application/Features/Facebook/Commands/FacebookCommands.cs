using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Facebook.Commands;

public record CreateFacebookDestinationCommand(
    string Name,
    DestinationType DestinationType,
    string TargetIdOrUrl,
    string? AccessToken
) : IRequest<int>;

public class CreateFacebookDestinationCommandValidator : AbstractValidator<CreateFacebookDestinationCommand>
{
    public CreateFacebookDestinationCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
        RuleFor(v => v.TargetIdOrUrl).NotEmpty().MaximumLength(255);
    }
}

public class CreateFacebookDestinationCommandHandler : IRequestHandler<CreateFacebookDestinationCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateFacebookDestinationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateFacebookDestinationCommand request, CancellationToken cancellationToken)
    {
        var dest = new FacebookDestination
        {
            Name = request.Name.Trim(),
            DestinationType = request.DestinationType,
            TargetIdOrUrl = request.TargetIdOrUrl.Trim(),
            AccessToken = request.AccessToken?.Trim(),
            IsActive = true
        };

        _context.FacebookDestinations.Add(dest);
        await _context.SaveChangesAsync(cancellationToken);
        return dest.Id;
    }
}

public record PublishToFacebookPageCommand(int ProductId, int DestinationId, string Message) : IRequest<FacebookPublishResult>;

public class PublishToFacebookPageCommandHandler : IRequestHandler<PublishToFacebookPageCommand, FacebookPublishResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IFacebookPagePublisher _pagePublisher;

    public PublishToFacebookPageCommandHandler(IApplicationDbContext context, IFacebookPagePublisher pagePublisher)
    {
        _context = context;
        _pagePublisher = pagePublisher;
    }

    public async Task<FacebookPublishResult> Handle(PublishToFacebookPageCommand request, CancellationToken cancellationToken)
    {
        var dest = await _context.FacebookDestinations
            .FirstOrDefaultAsync(d => d.Id == request.DestinationId && d.DestinationType == DestinationType.Page, cancellationToken);

        if (dest == null)
        {
            return new FacebookPublishResult(false, null, "Facebook Page destination not found.");
        }

        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            return new FacebookPublishResult(false, null, "Product not found.");
        }

        var primaryImage = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                           ?? product.Images.FirstOrDefault()?.ImageUrl;

        var result = await _pagePublisher.PublishPostAsync(
            pageId: dest.TargetIdOrUrl,
            accessToken: dest.AccessToken ?? string.Empty,
            message: request.Message,
            imageUrl: primaryImage,
            cancellationToken: cancellationToken
        );

        var postRecord = new ProductPost
        {
            ProductId = product.Id,
            FacebookDestinationId = dest.Id,
            PostedAt = DateTime.UtcNow,
            PostContent = request.Message,
            PostUrlOrId = result.PostId,
            Status = result.Success ? "Published" : "Failed",
            Notes = result.ErrorMessage
        };

        _context.ProductPosts.Add(postRecord);
        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}

public record LogGroupPostConfirmationCommand(int ProductId, int DestinationId, string PostContent, string? PostUrl, string? Notes) : IRequest<int>;

public class LogGroupPostConfirmationCommandHandler : IRequestHandler<LogGroupPostConfirmationCommand, int>
{
    private readonly IApplicationDbContext _context;

    public LogGroupPostConfirmationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(LogGroupPostConfirmationCommand request, CancellationToken cancellationToken)
    {
        var postRecord = new ProductPost
        {
            ProductId = request.ProductId,
            FacebookDestinationId = request.DestinationId,
            PostedAt = DateTime.UtcNow,
            PostContent = request.PostContent,
            PostUrlOrId = request.PostUrl,
            Status = "ConfirmedByUser",
            Notes = request.Notes
        };

        _context.ProductPosts.Add(postRecord);
        await _context.SaveChangesAsync(cancellationToken);
        return postRecord.Id;
    }
}
