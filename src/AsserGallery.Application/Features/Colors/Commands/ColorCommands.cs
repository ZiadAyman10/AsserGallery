using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Colors.Commands;

public record CreateColorCommand(
    string Name,
    string ArabicName,
    string HexCode
) : IRequest<int>;

public class CreateColorCommandValidator : AbstractValidator<CreateColorCommand>
{
    public CreateColorCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(50);
        RuleFor(v => v.ArabicName).NotEmpty().MaximumLength(50);
        RuleFor(v => v.HexCode).NotEmpty().Matches("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$").WithMessage("Invalid Hex color format.");
    }
}

public class CreateColorCommandHandler : IRequestHandler<CreateColorCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateColorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateColorCommand request, CancellationToken cancellationToken)
    {
        var color = new Color
        {
            Name = request.Name.Trim(),
            ArabicName = request.ArabicName.Trim(),
            HexCode = request.HexCode.Trim().ToUpperInvariant()
        };

        _context.Colors.Add(color);
        await _context.SaveChangesAsync(cancellationToken);
        return color.Id;
    }
}

public record UpdateColorCommand(
    int Id,
    string Name,
    string ArabicName,
    string HexCode
) : IRequest<bool>;

public class UpdateColorCommandHandler : IRequestHandler<UpdateColorCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateColorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateColorCommand request, CancellationToken cancellationToken)
    {
        var color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (color == null) return false;

        color.Name = request.Name.Trim();
        color.ArabicName = request.ArabicName.Trim();
        color.HexCode = request.HexCode.Trim().ToUpperInvariant();

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record DeleteColorCommand(int Id) : IRequest<bool>;

public class DeleteColorCommandHandler : IRequestHandler<DeleteColorCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteColorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteColorCommand request, CancellationToken cancellationToken)
    {
        var color = await _context.Colors
            .Include(c => c.Variants)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (color == null) return false;

        _context.Colors.Remove(color);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
