using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Common.Models;
using AsserGallery.Application.Features.Products.Dtos;
using AsserGallery.Application.Mappers;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Products.Queries;

public record GetProductsQuery(
    string? Search = null,
    int? CategoryId = null,
    int? SubCategoryId = null,
    int? ColorId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    ProductStatus? Status = null,
    bool? IsFeatured = null,
    bool OnlyInStock = false,
    string? SortBy = null,
    int PageNumber = 1,
    int PageSize = 12
) : IRequest<PaginatedList<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedList<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc!.Category)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Color)
            .Include(p => p.Images)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) ||
                                     p.ArabicName.ToLower().Contains(search) ||
                                     (p.Description != null && p.Description.ToLower().Contains(search)) ||
                                     (p.ArabicDescription != null && p.ArabicDescription.ToLower().Contains(search)));
        }

        if (request.CategoryId.HasValue && request.CategoryId.Value > 0)
        {
            query = query.Where(p => p.SubCategory != null && p.SubCategory.CategoryId == request.CategoryId.Value);
        }

        if (request.SubCategoryId.HasValue && request.SubCategoryId.Value > 0)
        {
            query = query.Where(p => p.SubCategoryId == request.SubCategoryId.Value);
        }

        if (request.ColorId.HasValue && request.ColorId.Value > 0)
        {
            query = query.Where(p => p.Variants.Any(v => v.ColorId == request.ColorId.Value && v.Quantity > 0));
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => (p.DiscountedPrice ?? p.Price) >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => (p.DiscountedPrice ?? p.Price) <= request.MaxPrice.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(p => p.Status == request.Status.Value);
        }

        if (request.IsFeatured.HasValue)
        {
            query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);
        }

        if (request.OnlyInStock)
        {
            query = query.Where(p => p.Status != ProductStatus.OutOfStock && p.Variants.Any(v => v.Quantity > 0));
        }

        query = request.SortBy switch
        {
            "price_asc" => query.OrderBy(p => p.DiscountedPrice ?? p.Price),
            "price_desc" => query.OrderByDescending(p => p.DiscountedPrice ?? p.Price),
            "date_asc" => query.OrderBy(p => p.DateAdded),
            "name_asc" => query.OrderBy(p => p.Name),
            "name_desc" => query.OrderByDescending(p => p.Name),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.DisplayOrder).ThenByDescending(p => p.DateAdded)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(p => p.ToDto()).ToList();
        return PaginatedList<ProductDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc!.Category)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Color)
            .Include(p => p.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        return product?.ToDto();
    }
}

public record GetColorsQuery : IRequest<List<ColorDto>>;

public class GetColorsQueryHandler : IRequestHandler<GetColorsQuery, List<ColorDto>>
{
    private readonly IApplicationDbContext _context;

    public GetColorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ColorDto>> Handle(GetColorsQuery request, CancellationToken cancellationToken)
    {
        var colors = await _context.Colors
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return colors.Select(c => c.ToDto()).ToList();
    }
}
