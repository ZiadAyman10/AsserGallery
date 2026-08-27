using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Application.Features.Sales.Dtos;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using AsserGallery.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Sales.Commands;

public record RegisterSaleCommand(
    string? CustomerName,
    string? CustomerPhone,
    string? Notes,
    DateTime? SaleDate,
    List<CreateSaleItemInput> Items
) : IRequest<int>;

public class RegisterSaleCommandValidator : AbstractValidator<RegisterSaleCommand>
{
    public RegisterSaleCommandValidator()
    {
        RuleFor(v => v.Items).NotEmpty().WithMessage("At least one sale item is required.");
        RuleForEach(v => v.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).GreaterThan(0);
            item.RuleFor(x => x.ProductVariantId).GreaterThan(0);
            item.RuleFor(x => x.Quantity).GreaterThan(0);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class RegisterSaleCommandHandler : IRequestHandler<RegisterSaleCommand, int>
{
    private readonly IApplicationDbContext _context;

    public RegisterSaleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(RegisterSaleCommand request, CancellationToken cancellationToken)
    {
        var saleDate = request.SaleDate ?? DateTime.UtcNow;
        var saleNumber = $"SAL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var sale = new Sale
        {
            SaleNumber = saleNumber,
            SaleDate = saleDate,
            CustomerName = request.CustomerName?.Trim(),
            CustomerPhone = request.CustomerPhone?.Trim(),
            Notes = request.Notes?.Trim()
        };

        decimal totalAmount = 0;
        var touchedProductIds = new HashSet<int>();

        foreach (var itemInput in request.Items)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == itemInput.ProductVariantId, cancellationToken);

            if (variant == null)
            {
                throw new DomainException($"Product variant ID {itemInput.ProductVariantId} not found.");
            }

            variant.DeductStock(itemInput.Quantity);
            touchedProductIds.Add(variant.ProductId);

            var saleItem = new SaleItem
            {
                ProductId = variant.ProductId,
                ProductVariantId = variant.Id,
                Quantity = itemInput.Quantity,
                UnitPrice = itemInput.UnitPrice
            };

            totalAmount += saleItem.SubTotal;
            sale.Items.Add(saleItem);
        }

        sale.TotalAmount = totalAmount;
        _context.Sales.Add(sale);

        // Update product statuses
        var products = await _context.Products
            .Include(p => p.Variants)
            .Where(p => touchedProductIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            product.UpdateStatusFromStock();
        }

        // Automatic Financial Transaction recording
        var incomeTransaction = new FinancialTransaction
        {
            Title = $"Sale {sale.SaleNumber}",
            Description = $"Sale to {request.CustomerName ?? "Customer"} ({sale.Items.Count} item(s))",
            Amount = totalAmount,
            Date = saleDate,
            Type = TransactionType.Income,
            Category = "SalesRevenue"
        };
        _context.FinancialTransactions.Add(incomeTransaction);

        await _context.SaveChangesAsync(cancellationToken);
        return sale.Id;
    }
}
