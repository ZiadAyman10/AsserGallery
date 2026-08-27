using AsserGallery.Domain.Enums;

namespace AsserGallery.Application.Features.Facebook.Dtos;

public record FacebookDestinationDto(
    int Id,
    string Name,
    DestinationType DestinationType,
    string TargetIdOrUrl,
    bool IsActive,
    int PostCount
);

public record ProductPostDto(
    int Id,
    int ProductId,
    string ProductName,
    int FacebookDestinationId,
    string FacebookDestinationName,
    DestinationType DestinationType,
    DateTime PostedAt,
    string PostContent,
    string? PostUrlOrId,
    string Status,
    string? Notes
);

public record GeneratedGroupPostDto(
    int ProductId,
    string ProductName,
    string FormattedText,
    string? TargetGroupUrl,
    string StoreWhatsAppLink
);
