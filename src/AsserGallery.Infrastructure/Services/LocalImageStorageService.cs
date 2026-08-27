using AsserGallery.Application.Common.Interfaces;

namespace AsserGallery.Infrastructure.Services;

public class LocalImageStorageService : IImageStorageService
{
    private readonly string _webRootPath;

    public LocalImageStorageService(string? webRootPath = null)
    {
        _webRootPath = webRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<string> SaveImageAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var relativeDir = Path.Combine("uploads", folder);
        var targetDir = Path.Combine(_webRootPath, relativeDir);

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        var ext = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(targetDir, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        return $"/{relativeDir.Replace('\\', '/')}/{uniqueFileName}";
    }

    public Task DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return Task.CompletedTask;

        var cleanPath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_webRootPath, cleanPath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
