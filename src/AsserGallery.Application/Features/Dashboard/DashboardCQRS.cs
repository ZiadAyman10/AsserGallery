using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Dashboard.Dtos;
using AsserGallery.Application.Mappers;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Dashboard.Queries;

public record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;

public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc!.Category)
            .Include(p => p.Variants)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalProducts = products.Count;
        var inStock = products.Count(p => p.Status == ProductStatus.Available);
        var limitedStock = products.Count(p => p.Status == ProductStatus.LimitedStock);
        var outOfStock = products.Count(p => p.Status == ProductStatus.OutOfStock);

        var sales = await _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v!.Color)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalOrders = sales.Count;
        var totalRevenue = sales.Sum(s => s.TotalAmount);

        var transactions = await _context.FinancialTransactions.AsNoTracking().ToListAsync(cancellationToken);
        var totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var netProfit = totalIncome - totalExpenses;

        var pendingRequests = await _context.CustomerRequests
            .AsNoTracking()
            .CountAsync(r => r.Status == CustomerRequestStatus.New, cancellationToken);

        var recentSales = sales.OrderByDescending(s => s.SaleDate).Take(5).Select(s => s.ToDto()).ToList();

        var recentRequests = await _context.CustomerRequests
            .Include(r => r.Product)
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        // 1. Calculate Monthly Revenue & Expense Trends (Last 6 months)
        var monthlyTrends = new List<MonthlyTrendDto>();
        var now = DateTime.UtcNow;
        for (int i = 5; i >= 0; i--)
        {
            var targetMonth = now.AddMonths(-i);
            var year = targetMonth.Year;
            var month = targetMonth.Month;
            var monthLabel = targetMonth.ToString("MMM yyyy");

            var rev = sales
                .Where(s => s.SaleDate.Year == year && s.SaleDate.Month == month)
                .Sum(s => s.TotalAmount);

            var exp = transactions
                .Where(t => t.Type == TransactionType.Expense && t.Date.Year == year && t.Date.Month == month)
                .Sum(t => t.Amount);

            monthlyTrends.Add(new MonthlyTrendDto(monthLabel, rev, exp, rev - exp));
        }

        // 2. Category Breakdowns
        var categoryBreakdowns = products
            .GroupBy(p => p.SubCategory?.Category?.Name ?? "Uncategorized")
            .Select(g => new CategoryBreakdownDto(
                CategoryName: g.Key,
                ProductCount: g.Count(),
                TotalStock: g.Sum(p => p.GetTotalStock())
            ))
            .OrderByDescending(c => c.ProductCount)
            .ToList();

        // 3. Top Selling Products
        var allSaleItems = await _context.SaleItems.AsNoTracking().ToListAsync(cancellationToken);
        var topSellingProducts = allSaleItems
            .GroupBy(i => i.ProductId)
            .Select(g =>
            {
                var prod = products.FirstOrDefault(p => p.Id == g.Key);
                return new TopSellingProductDto(
                    ProductId: g.Key,
                    ProductName: prod?.Name ?? $"Product #{g.Key}",
                    QuantitySold: g.Sum(i => i.Quantity),
                    TotalRevenue: g.Sum(i => i.Quantity * i.UnitPrice)
                );
            })
            .OrderByDescending(t => t.TotalRevenue)
            .Take(5)
            .ToList();

        return new DashboardSummaryDto(
            TotalProductsCount: totalProducts,
            InStockProductsCount: inStock,
            LimitedStockProductsCount: limitedStock,
            OutOfStockProductsCount: outOfStock,
            TotalOrdersCount: totalOrders,
            TotalRevenue: totalRevenue,
            TotalExpenses: totalExpenses,
            NetProfit: netProfit,
            PendingCustomerRequestsCount: pendingRequests,
            RecentSales: recentSales,
            RecentRequests: recentRequests.Select(r => r.ToDto()).ToList(),
            MonthlyTrends: monthlyTrends,
            CategoryBreakdowns: categoryBreakdowns,
            TopSellingProducts: topSellingProducts
        );
    }
}
