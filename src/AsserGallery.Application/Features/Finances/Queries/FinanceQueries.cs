using System.Globalization;
using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Finances.Dtos;
using AsserGallery.Application.Mappers;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Finances.Queries;

public record GetFinancialSummaryQuery : IRequest<FinancialSummaryDto>;

public class GetFinancialSummaryQueryHandler : IRequestHandler<GetFinancialSummaryQuery, FinancialSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetFinancialSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialSummaryDto> Handle(GetFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var transactions = await _context.FinancialTransactions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var netProfit = totalIncome - totalExpense;

        var monthTransactions = transactions.Where(t => t.Date >= startOfMonth).ToList();
        var monthIncome = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var monthExpense = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var monthProfit = monthIncome - monthExpense;

        // Last 6 months breakdown
        var breakdown = new List<MonthlyFinancialPointDto>();
        for (int i = 5; i >= 0; i--)
        {
            var mDate = now.AddMonths(-i);
            var mStart = new DateTime(mDate.Year, mDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var mEnd = mStart.AddMonths(1);

            var mTx = transactions.Where(t => t.Date >= mStart && t.Date < mEnd).ToList();
            var inc = mTx.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var exp = mTx.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            breakdown.Add(new MonthlyFinancialPointDto(
                MonthLabel: mStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Income: inc,
                Expense: exp,
                Profit: inc - exp
            ));
        }

        return new FinancialSummaryDto(
            TotalIncome: totalIncome,
            TotalExpense: totalExpense,
            NetProfit: netProfit,
            ThisMonthIncome: monthIncome,
            ThisMonthExpense: monthExpense,
            ThisMonthNetProfit: monthProfit,
            MonthlyBreakdown: breakdown
        );
    }
}

public record GetFinancialTransactionsQuery(
    TransactionType? Type = null,
    string? Category = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? LinkedProductId = null
) : IRequest<List<FinancialTransactionDto>>;

public class GetFinancialTransactionsQueryHandler : IRequestHandler<GetFinancialTransactionsQuery, List<FinancialTransactionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFinancialTransactionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FinancialTransactionDto>> Handle(GetFinancialTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FinancialTransactions
            .Include(t => t.LinkedProduct)
            .AsNoTracking()
            .AsQueryable();

        if (request.Type.HasValue)
        {
            query = query.Where(t => t.Type == request.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(t => t.Category == request.Category);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(t => t.Date >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(t => t.Date <= request.EndDate.Value);
        }

        if (request.LinkedProductId.HasValue)
        {
            query = query.Where(t => t.LinkedProductId == request.LinkedProductId.Value);
        }

        var list = await query.OrderByDescending(t => t.Date).ToListAsync(cancellationToken);
        return list.Select(t => t.ToDto()).ToList();
    }
}
