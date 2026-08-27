using AsserGallery.Application.Features.CustomerRequests.Dtos;
using AsserGallery.Application.Features.Sales.Dtos;

namespace AsserGallery.Application.Features.Dashboard.Dtos;

public record DashboardSummaryDto(
    int TotalProductsCount,
    int InStockProductsCount,
    int LimitedStockProductsCount,
    int OutOfStockProductsCount,
    int TotalOrdersCount,
    decimal TotalRevenue,
    decimal TotalExpenses,
    decimal NetProfit,
    int PendingCustomerRequestsCount,
    List<SaleDto> RecentSales,
    List<CustomerRequestDto> RecentRequests
);
