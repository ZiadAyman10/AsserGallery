using System.Text;
using AsserGallery.Application.Common.Dtos;
using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Finances.Queries;

public record ExportFinancesQuery(
    TransactionType? Type = null,
    string? Category = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<FileExportDto>;

public class ExportFinancesQueryHandler : IRequestHandler<ExportFinancesQuery, FileExportDto>
{
    private readonly IApplicationDbContext _context;

    public ExportFinancesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FileExportDto> Handle(ExportFinancesQuery request, CancellationToken cancellationToken)
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

        var transactions = await query.OrderByDescending(t => t.Date).ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        // CSV Header
        sb.AppendLine("ID,Date,Title,Type,Category,Amount (EGP),Linked Product,Description");

        foreach (var t in transactions)
        {
            var safeId = t.Id;
            var safeDate = t.Date.ToString("yyyy-MM-dd HH:mm");
            var safeTitle = EscapeCsv(t.Title);
            var safeType = t.Type.ToString();
            var safeCategory = EscapeCsv(t.Category);
            var safeAmount = t.Amount.ToString("F2");
            var safeProduct = EscapeCsv(t.LinkedProduct?.Name ?? "-");
            var safeDesc = EscapeCsv(t.Description ?? "");

            sb.AppendLine($"{safeId},{safeDate},{safeTitle},{safeType},{safeCategory},{safeAmount},{safeProduct},{safeDesc}");
        }

        // Summary footer
        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var net = totalIncome - totalExpense;

        sb.AppendLine();
        sb.AppendLine($"Summary,,,Total Income,{totalIncome:F2} EGP");
        sb.AppendLine($",,,Total Expenses,{totalExpense:F2} EGP");
        sb.AppendLine($",,,Net Profit,{net:F2} EGP");

        // Add UTF-8 BOM so Excel opens with proper Arabic and formatting
        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileBytes = new byte[preamble.Length + bytes.Length];
        Buffer.BlockCopy(preamble, 0, fileBytes, 0, preamble.Length);
        Buffer.BlockCopy(bytes, 0, fileBytes, preamble.Length, bytes.Length);

        var fileName = $"AsserGallery_Finances_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return new FileExportDto(fileBytes, "text/csv; charset=utf-8", fileName);
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return $"\"{value}\"";
    }
}
