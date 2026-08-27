using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Sales.Dtos;
using AsserGallery.Application.Mappers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Sales.Queries;

public record GetSalesQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Search = null
) : IRequest<List<SaleDto>>;

public class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, List<SaleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSalesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SaleDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v!.Color)
            .AsNoTracking()
            .AsQueryable();

        if (request.StartDate.HasValue)
        {
            query = query.Where(s => s.SaleDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(s => s.SaleDate <= request.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s => s.SaleNumber.ToLower().Contains(search) ||
                                     (s.CustomerName != null && s.CustomerName.ToLower().Contains(search)) ||
                                     (s.CustomerPhone != null && s.CustomerPhone.Contains(search)));
        }

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync(cancellationToken);

        return sales.Select(s => s.ToDto()).ToList();
    }
}

public record GetSaleByIdQuery(int Id) : IRequest<SaleDto?>;

public class GetSaleByIdQueryHandler : IRequestHandler<GetSaleByIdQuery, SaleDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSaleByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SaleDto?> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v!.Color)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        return sale?.ToDto();
    }
}
