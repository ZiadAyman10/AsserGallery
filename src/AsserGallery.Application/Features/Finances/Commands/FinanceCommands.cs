using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using FluentValidation;
using MediatR;

namespace AsserGallery.Application.Features.Finances.Commands;

public record AddFinancialTransactionCommand(
    string Title,
    string? Description,
    decimal Amount,
    TransactionType Type,
    string Category,
    DateTime? Date = null,
    int? LinkedProductId = null
) : IRequest<int>;

public class AddFinancialTransactionCommandValidator : AbstractValidator<AddFinancialTransactionCommand>
{
    public AddFinancialTransactionCommandValidator()
    {
        RuleFor(v => v.Title).NotEmpty().MaximumLength(150);
        RuleFor(v => v.Amount).GreaterThan(0);
        RuleFor(v => v.Category).NotEmpty().MaximumLength(50);
    }
}

public class AddFinancialTransactionCommandHandler : IRequestHandler<AddFinancialTransactionCommand, int>
{
    private readonly IApplicationDbContext _context;

    public AddFinancialTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(AddFinancialTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = new FinancialTransaction
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Amount = request.Amount,
            Type = request.Type,
            Category = request.Category.Trim(),
            Date = request.Date ?? DateTime.UtcNow,
            LinkedProductId = request.LinkedProductId
        };

        _context.FinancialTransactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }
}

public record DeleteFinancialTransactionCommand(int Id) : IRequest<bool>;

public class DeleteFinancialTransactionCommandHandler : IRequestHandler<DeleteFinancialTransactionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteFinancialTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteFinancialTransactionCommand request, CancellationToken cancellationToken)
    {
        var tx = await _context.FinancialTransactions.FindAsync(new object[] { request.Id }, cancellationToken);
        if (tx == null) return false;

        _context.FinancialTransactions.Remove(tx);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
