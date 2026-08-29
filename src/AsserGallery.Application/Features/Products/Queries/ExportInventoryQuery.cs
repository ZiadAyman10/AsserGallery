using System.Text;
using AsserGallery.Application.Common.Dtos;
using AsserGallery.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Products.Queries;

public record ExportInventoryQuery(
    int? CategoryId = null,
    string? Search = null
) : IRequest<FileExportDto>;

public class ExportInventoryQueryHandler : IRequestHandler<ExportInventoryQuery, FileExportDto>
{
    private readonly IApplicationDbContext _context;

    public ExportInventoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FileExportDto> Handle(ExportInventoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .Include(p => p.SubCategory)
                .ThenInclude(sc => sc!.Category)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Color)
            .AsNoTracking()
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.SubCategory != null && p.SubCategory.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                p.ArabicName.ToLower().Contains(searchLower));
        }

        var products = await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        // CSV Header
        sb.AppendLine("Product ID,Product Name,Arabic Name,Category,Subcategory,Price (EGP),Discounted Price (EGP),Status,Total Stock,Color Variants Breakdown");

        foreach (var p in products)
        {
            var variantsDetail = string.Join(" | ", p.Variants.Select(v =>
                $"{v.Color?.Name ?? "Color"}: {v.Quantity} pcs"));

            var safeId = p.Id;
            var safeName = EscapeCsv(p.Name);
            var safeArName = EscapeCsv(p.ArabicName);
            var safeCat = EscapeCsv(p.SubCategory?.Category?.Name ?? "Uncategorized");
            var safeSubCat = EscapeCsv(p.SubCategory?.Name ?? "General");
            var safePrice = p.Price.ToString("F2");
            var safeDiscPrice = p.DiscountedPrice.HasValue ? p.DiscountedPrice.Value.ToString("F2") : "-";
            var safeStatus = p.Status.ToString();
            var safeStock = p.GetTotalStock();
            var safeVariants = EscapeCsv(variantsDetail);

            sb.AppendLine($"{safeId},{safeName},{safeArName},{safeCat},{safeSubCat},{safePrice},{safeDiscPrice},{safeStatus},{safeStock},{safeVariants}");
        }

        // Summary footer
        var totalUnits = products.Sum(p => p.GetTotalStock());
        sb.AppendLine();
        sb.AppendLine($"Total Products in Catalog,,{products.Count}");
        sb.AppendLine($"Total Inventory Units on Hand,,{totalUnits}");

        // Add UTF-8 BOM so Excel opens with proper Arabic and formatting
        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var fileBytes = new byte[preamble.Length + bytes.Length];
        Buffer.BlockCopy(preamble, 0, fileBytes, 0, preamble.Length);
        Buffer.BlockCopy(bytes, 0, fileBytes, preamble.Length, bytes.Length);

        var fileName = $"AsserGallery_Inventory_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
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
