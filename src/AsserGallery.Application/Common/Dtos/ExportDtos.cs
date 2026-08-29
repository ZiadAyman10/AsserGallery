namespace AsserGallery.Application.Common.Dtos;

public record FileExportDto(
    byte[] Content,
    string ContentType,
    string FileName
);
