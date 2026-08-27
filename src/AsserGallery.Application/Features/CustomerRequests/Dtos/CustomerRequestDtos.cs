using AsserGallery.Domain.Enums;

namespace AsserGallery.Application.Features.CustomerRequests.Dtos;

public record CustomerRequestDto(
    int Id,
    string CustomerName,
    string PhoneNumber,
    ContactChannel PreferredChannel,
    string? Message,
    int? ProductId,
    string? ProductName,
    string? ProductArabicName,
    CustomerRequestStatus Status,
    string? AdminNotes,
    DateTime CreatedAt
);
