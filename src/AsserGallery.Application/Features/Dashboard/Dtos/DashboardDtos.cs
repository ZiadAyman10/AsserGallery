using AsserGallery.Application.Features.CustomerRequests.Dtos;
using AsserGallery.Application.Features.Sales.Dtos;

namespace AsserGallery.Application.Features.Dashboard.Dtos;

public record MonthlyTrendDto(
    string MonthLabel,
    decimal Revenue,
    decimal Expenses,
    decimal Profit
);

public record CategoryBreakdownDto(
    string CategoryName,
    int ProductCount,
    int TotalStock
);

public record TopSellingProductDto(
    int ProductId,
    string ProductName,
    int QuantitySold,
    decimal TotalRevenue
);

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
    List<CustomerRequestDto> RecentRequests,
    List<MonthlyTrendDto> MonthlyTrends,
    List<CategoryBreakdownDto> CategoryBreakdowns,
    List<TopSellingProductDto> TopSellingProducts
);
