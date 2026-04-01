using Microsoft.AspNetCore.Http;

namespace SHNGearBE.Services.Interfaces.Media;

public interface IImageStorageService
{
    Task<StoredImageResult> UploadImageAsync(IFormFile file, string? folder = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredImageResult>> UploadImagesAsync(IEnumerable<IFormFile> files, string? folder = null, CancellationToken cancellationToken = default);
}

public record StoredImageResult(
    string Url,
    string PublicId,
    long Bytes,
    string Format
);
