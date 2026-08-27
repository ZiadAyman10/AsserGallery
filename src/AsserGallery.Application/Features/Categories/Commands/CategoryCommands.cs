using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AsserGallery.Application.Features.Categories.Commands;

public record CreateCategoryCommand(
    string Name,
    string ArabicName,
    string? Description,
    string? ArabicDescription,
    string? ImageUrl,
    int DisplayOrder
) : IRequest<int>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
        RuleFor(v => v.ArabicName).NotEmpty().MaximumLength(100);
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = request.Name.Trim(),
            ArabicName = request.ArabicName.Trim(),
            Description = request.Description?.Trim(),
            ArabicDescription = request.ArabicDescription?.Trim(),
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}

public record CreateSubCategoryCommand(
    int CategoryId,
    string Name,
    string ArabicName,
    string? Description,
    string? ArabicDescription,
    int DisplayOrder
) : IRequest<int>;

public class CreateSubCategoryCommandValidator : AbstractValidator<CreateSubCategoryCommand>
{
    public CreateSubCategoryCommandValidator()
    {
        RuleFor(v => v.CategoryId).GreaterThan(0);
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
        RuleFor(v => v.ArabicName).NotEmpty().MaximumLength(100);
    }
}

public class CreateSubCategoryCommandHandler : IRequestHandler<CreateSubCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateSubCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateSubCategoryCommand request, CancellationToken cancellationToken)
    {
        var subCategory = new SubCategory
        {
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            ArabicName = request.ArabicName.Trim(),
            Description = request.Description?.Trim(),
            ArabicDescription = request.ArabicDescription?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        _context.SubCategories.Add(subCategory);
        await _context.SaveChangesAsync(cancellationToken);

        return subCategory.Id;
    }
}

public record DeleteCategoryCommand(int Id) : IRequest<bool>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null) return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
