using AsserGallery.Domain.Enums;

namespace AsserGallery.Application.Features.Finances.Dtos;

public record FinancialTransactionDto(
    int Id,
    string Title,
    string? Description,
    decimal Amount,
    DateTime Date,
    TransactionType Type,
    string Category,
    int? LinkedProductId,
    string? LinkedProductName
);

public record FinancialSummaryDto(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetProfit,
    decimal ThisMonthIncome,
    decimal ThisMonthExpense,
    decimal ThisMonthNetProfit,
    List<MonthlyFinancialPointDto> MonthlyBreakdown
);

public record MonthlyFinancialPointDto(
    string MonthLabel,
    decimal Income,
    decimal Expense,
    decimal Profit
);
