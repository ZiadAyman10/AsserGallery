using AsserGallery.Application.Common.Interfaces;
using AsserGallery.Domain.Entities;
using AsserGallery.Domain.Enums;
using FluentValidation;
using MediatR;

namespace AsserGallery.Application.Features.CustomerRequests.Commands;

public record SubmitCustomerRequestCommand(
    string CustomerName,
    string PhoneNumber,
    ContactChannel PreferredChannel,
    string? Message,
    int? ProductId
) : IRequest<int>;

public class SubmitCustomerRequestCommandValidator : AbstractValidator<SubmitCustomerRequestCommand>
{
    public SubmitCustomerRequestCommandValidator()
    {
        RuleFor(v => v.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(v => v.PhoneNumber).NotEmpty().MaximumLength(30);
    }
}

public class SubmitCustomerRequestCommandHandler : IRequestHandler<SubmitCustomerRequestCommand, int>
{
    private readonly IApplicationDbContext _context;

    public SubmitCustomerRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(SubmitCustomerRequestCommand request, CancellationToken cancellationToken)
    {
        var req = new CustomerRequest
        {
            CustomerName = request.CustomerName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            PreferredChannel = request.PreferredChannel,
            Message = request.Message?.Trim(),
            ProductId = request.ProductId,
            Status = CustomerRequestStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomerRequests.Add(req);
        await _context.SaveChangesAsync(cancellationToken);
        return req.Id;
    }
}

public record UpdateCustomerRequestStatusCommand(int Id, CustomerRequestStatus Status, string? AdminNotes) : IRequest<bool>;

public class UpdateCustomerRequestStatusCommandHandler : IRequestHandler<UpdateCustomerRequestStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerRequestStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCustomerRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var req = await _context.CustomerRequests.FindAsync(new object[] { request.Id }, cancellationToken);
        if (req == null) return false;

        req.Status = request.Status;
        if (request.AdminNotes != null)
        {
            req.AdminNotes = request.AdminNotes.Trim();
        }
        req.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
