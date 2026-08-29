using System.Text;
using AsserGallery.Application.Common.Dtos;
using AsserGallery.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Sales.Queries;

public record ExportSalesQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Search = null
) : IRequest<FileExportDto>;

public class ExportSalesQueryHandler : IRequestHandler<ExportSalesQuery, FileExportDto>
{
    private readonly IApplicationDbContext _context;

    public ExportSalesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FileExportDto> Handle(ExportSalesQuery request, CancellationToken cancellationToken)
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
            var searchLower = request.Search.Trim().ToLower();
            query = query.Where(s =>
                s.SaleNumber.ToLower().Contains(searchLower) ||
                (s.CustomerName != null && s.CustomerName.ToLower().Contains(searchLower)) ||
                (s.CustomerPhone != null && s.CustomerPhone.Contains(searchLower)));
        }

        var sales = await query.OrderByDescending(s => s.SaleDate).ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        // CSV Header
        sb.AppendLine("Invoice Number,Date,Customer Name,Phone,Total Amount (EGP),Items Count,Items Detail,Notes");

        foreach (var sale in sales)
        {
            var itemsDetail = string.Join(" | ", sale.Items.Select(i =>
                $"{i.Product?.Name ?? "Product"} (Color: {i.ProductVariant?.Color?.Name ?? "N/A"}, Qty: {i.Quantity}, Price: {i.UnitPrice} EGP)"));

            var safeInvoice = EscapeCsv(sale.SaleNumber);
            var safeDate = sale.SaleDate.ToString("yyyy-MM-dd HH:mm");
            var safeCustomer = EscapeCsv(sale.CustomerName ?? "Direct Customer");
            var safePhone = EscapeCsv(sale.CustomerPhone ?? "-");
            var safeTotal = sale.TotalAmount.ToString("F2");
            var safeCount = sale.Items.Sum(i => i.Quantity);
            var safeDetails = EscapeCsv(itemsDetail);
            var safeNotes = EscapeCsv(sale.Notes ?? "");

            sb.AppendLine($"{safeInvoice},{safeDate},{safeCustomer},{safePhone},{safeTotal},{safeCount},{safeDetails},{safeNotes}");
        }

        // Add UTF-8 BOM so Excel renders Arabic properly
        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileBytes = new byte[preamble.Length + bytes.Length];
        Buffer.BlockCopy(preamble, 0, fileBytes, 0, preamble.Length);
        Buffer.BlockCopy(bytes, 0, fileBytes, preamble.Length, bytes.Length);

        var fileName = $"AsserGallery_Sales_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
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
