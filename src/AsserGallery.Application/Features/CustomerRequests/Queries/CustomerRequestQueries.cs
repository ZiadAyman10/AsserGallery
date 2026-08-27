using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.CustomerRequests.Dtos;
using AsserGallery.Application.Mappers;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.CustomerRequests.Queries;

public record GetCustomerRequestsQuery(CustomerRequestStatus? Status = null) : IRequest<List<CustomerRequestDto>>;

public class GetCustomerRequestsQueryHandler : IRequestHandler<GetCustomerRequestsQuery, List<CustomerRequestDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerRequestDto>> Handle(GetCustomerRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CustomerRequests
            .Include(r => r.Product)
            .AsNoTracking()
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        var list = await query.OrderByDescending(r => r.CreatedAt).ToListAsync(cancellationToken);
        return list.Select(r => r.ToDto()).ToList();
    }
}
