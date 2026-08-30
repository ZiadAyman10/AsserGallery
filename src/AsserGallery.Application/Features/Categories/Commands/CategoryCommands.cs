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

public record UpdateCategoryCommand(
    int Id,
    string Name,
    string ArabicName,
    string? Description,
    string? ArabicDescription,
    int DisplayOrder,
    bool IsActive
) : IRequest<bool>;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null) return false;

        category.Name = request.Name.Trim();
        category.ArabicName = request.ArabicName.Trim();
        category.Description = request.Description?.Trim();
        category.ArabicDescription = request.ArabicDescription?.Trim();
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record UpdateSubCategoryCommand(
    int Id,
    string Name,
    string ArabicName,
    string? Description,
    string? ArabicDescription,
    int DisplayOrder,
    bool IsActive
) : IRequest<bool>;

public class UpdateSubCategoryCommandHandler : IRequestHandler<UpdateSubCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateSubCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateSubCategoryCommand request, CancellationToken cancellationToken)
    {
        var subCategory = await _context.SubCategories
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subCategory == null) return false;

        subCategory.Name = request.Name.Trim();
        subCategory.ArabicName = request.ArabicName.Trim();
        subCategory.Description = request.Description?.Trim();
        subCategory.ArabicDescription = request.ArabicDescription?.Trim();
        subCategory.DisplayOrder = request.DisplayOrder;
        subCategory.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record DeleteSubCategoryCommand(int Id) : IRequest<bool>;

public class DeleteSubCategoryCommandHandler : IRequestHandler<DeleteSubCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteSubCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteSubCategoryCommand request, CancellationToken cancellationToken)
    {
        var subCategory = await _context.SubCategories
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subCategory == null) return false;

        _context.SubCategories.Remove(subCategory);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
