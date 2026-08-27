using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Products.Dtos;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Products.Commands;

public record CreateProductCommand(
    string Name,
    string ArabicName,
    string? Description,
    string? ArabicDescription,
    decimal Price,
    decimal? DiscountedPrice,
    int SubCategoryId,
    bool IsFeatured,
    int DisplayOrder,
    List<CreateProductVariantInput> Variants,
    List<CreateProductImageInput> Images
) : IRequest<int>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(150);
        RuleFor(v => v.ArabicName).NotEmpty().MaximumLength(150);
        RuleFor(v => v.Price).GreaterThan(0);
        RuleFor(v => v.DiscountedPrice)
            .LessThan(v => v.Price)
            .When(v => v.DiscountedPrice.HasValue)
            .WithMessage("Discounted price must be less than the regular price.");
        RuleFor(v => v.SubCategoryId).GreaterThan(0);
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            ArabicName = request.ArabicName.Trim(),
            Description = request.Description?.Trim(),
            ArabicDescription = request.ArabicDescription?.Trim(),
            Price = request.Price,
            DiscountedPrice = request.DiscountedPrice,
            SubCategoryId = request.SubCategoryId,
            IsFeatured = request.IsFeatured,
            DisplayOrder = request.DisplayOrder,
            DateAdded = DateTime.UtcNow
        };

        if (request.Variants.Any())
        {
            foreach (var variantInput in request.Variants)
            {
                product.Variants.Add(new ProductVariant
                {
                    ColorId = variantInput.ColorId,
                    Quantity = Math.Max(0, variantInput.Quantity)
                });
            }
        }

        if (request.Images.Any())
        {
            var isFirst = true;
            foreach (var imageInput in request.Images)
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = imageInput.ImageUrl,
                    ImageType = imageInput.ImageType,
                    IsPrimary = imageInput.IsPrimary || isFirst,
                    DisplayOrder = imageInput.DisplayOrder
                });
                isFirst = false;
            }
        }

        product.UpdateStatusFromStock();

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}

public record UpdateProductCommand(
    int Id,
    string Name,
    string ArabicName,
    string? Description,
    string? ArabicDescription,
    decimal Price,
    decimal? DiscountedPrice,
    int SubCategoryId,
    bool IsFeatured,
    int DisplayOrder,
    List<CreateProductVariantInput> Variants
) : IRequest<bool>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(v => v.Id).GreaterThan(0);
        RuleFor(v => v.Name).NotEmpty().MaximumLength(150);
        RuleFor(v => v.ArabicName).NotEmpty().MaximumLength(150);
        RuleFor(v => v.Price).GreaterThan(0);
        RuleFor(v => v.DiscountedPrice)
            .LessThan(v => v.Price)
            .When(v => v.DiscountedPrice.HasValue)
            .WithMessage("Discounted price must be less than the regular price.");
        RuleFor(v => v.SubCategoryId).GreaterThan(0);
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null) return false;

        product.Name = request.Name.Trim();
        product.ArabicName = request.ArabicName.Trim();
        product.Description = request.Description?.Trim();
        product.ArabicDescription = request.ArabicDescription?.Trim();
        product.Price = request.Price;
        product.DiscountedPrice = request.DiscountedPrice;
        product.SubCategoryId = request.SubCategoryId;
        product.IsFeatured = request.IsFeatured;
        product.DisplayOrder = request.DisplayOrder;
        product.LastModifiedAt = DateTime.UtcNow;

        // Update variants
        var existingVariantMap = product.Variants.ToDictionary(v => v.ColorId);
        var requestedColorIds = request.Variants.Select(v => v.ColorId).ToHashSet();

        // Remove variants not in the request
        foreach (var variant in product.Variants.ToList())
        {
            if (!requestedColorIds.Contains(variant.ColorId))
            {
                _context.ProductVariants.Remove(variant);
            }
        }

        // Add or update
        foreach (var variantInput in request.Variants)
        {
            if (existingVariantMap.TryGetValue(variantInput.ColorId, out var existingVariant))
            {
                existingVariant.Quantity = Math.Max(0, variantInput.Quantity);
            }
            else
            {
                product.Variants.Add(new ProductVariant
                {
                    ColorId = variantInput.ColorId,
                    Quantity = Math.Max(0, variantInput.Quantity)
                });
            }
        }

        product.UpdateStatusFromStock();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record DeleteProductCommand(int Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record AdjustVariantStockCommand(int VariantId, int NewQuantity) : IRequest<bool>;

public class AdjustVariantStockCommandHandler : IRequestHandler<AdjustVariantStockCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AdjustVariantStockCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AdjustVariantStockCommand request, CancellationToken cancellationToken)
    {
        var variant = await _context.ProductVariants
            .Include(v => v.Product)
                .ThenInclude(p => p!.Variants)
            .FirstOrDefaultAsync(v => v.Id == request.VariantId, cancellationToken);

        if (variant == null) return false;

        variant.Quantity = Math.Max(0, request.NewQuantity);
        variant.Product?.UpdateStatusFromStock();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record AddProductImageCommand(int ProductId, string ImageUrl, ImageType ImageType, bool IsPrimary) : IRequest<int>;

public class AddProductImageCommandHandler : IRequestHandler<AddProductImageCommand, int>
{
    private readonly IApplicationDbContext _context;

    public AddProductImageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AddProductImageCommand request, CancellationToken cancellationToken)
    {
        if (request.IsPrimary)
        {
            var existingImages = await _context.ProductImages
                .Where(i => i.ProductId == request.ProductId)
                .ToListAsync(cancellationToken);
            foreach (var img in existingImages)
            {
                img.IsPrimary = false;
            }
        }

        var image = new ProductImage
        {
            ProductId = request.ProductId,
            ImageUrl = request.ImageUrl,
            ImageType = request.ImageType,
            IsPrimary = request.IsPrimary,
            DisplayOrder = await _context.ProductImages.CountAsync(i => i.ProductId == request.ProductId, cancellationToken) + 1
        };

        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync(cancellationToken);
        return image.Id;
    }
}

public record DeleteProductImageCommand(int ImageId) : IRequest<bool>;

public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductImageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var img = await _context.ProductImages.FindAsync(new object[] { request.ImageId }, cancellationToken);
        if (img == null) return false;

        _context.ProductImages.Remove(img);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
