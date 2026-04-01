using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SHNGearBE.Configurations;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Services.Interfaces.Media;

namespace SHNGearBE.Infrastructure.Media;

public class CloudinaryImageStorageService : IImageStorageService
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif",
        "image/avif"
    };

    private readonly Cloudinary? _cloudinary;
    private readonly bool _isConfigured;
    private const string MissingCloudinaryConfigMessage = "Cloudinary is not configured. Please set CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, and CLOUDINARY_API_SECRET in environment variables.";

    public CloudinaryImageStorageService(IOptions<CloudinarySettings> settings)
    {
        var cloudinarySettings = settings.Value;

        var cloudName = string.IsNullOrWhiteSpace(cloudinarySettings.CloudName)
            ? Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
            : cloudinarySettings.CloudName;
        var apiKey = string.IsNullOrWhiteSpace(cloudinarySettings.ApiKey)
            ? Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")
            : cloudinarySettings.ApiKey;
        var apiSecret = string.IsNullOrWhiteSpace(cloudinarySettings.ApiSecret)
            ? Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
            : cloudinarySettings.ApiSecret;

        if (string.IsNullOrWhiteSpace(cloudName)
            || string.IsNullOrWhiteSpace(apiKey)
            || string.IsNullOrWhiteSpace(apiSecret))
        {
            _isConfigured = false;
            return;
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account)
        {
            Api = { Secure = true }
        };
        _isConfigured = true;
    }

    public async Task<StoredImageResult> UploadImageAsync(IFormFile file, string? folder = null, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        ValidateFile(file);

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = string.IsNullOrWhiteSpace(folder) ? "shn-gear" : folder.Trim(),
            Overwrite = false,
            UseFilename = true,
            UniqueFilename = true
        };

        var result = await _cloudinary!.UploadAsync(uploadParams, cancellationToken);

        if (result.Error != null)
        {
            throw new ProjectException(ResponseType.ServiceUnavailable, $"Upload image failed: {result.Error.Message}");
        }

        if (string.IsNullOrWhiteSpace(result.SecureUrl?.ToString()) || string.IsNullOrWhiteSpace(result.PublicId))
        {
            throw new ProjectException(ResponseType.ServiceUnavailable, "Cloudinary response is invalid.");
        }

        return new StoredImageResult(
            result.SecureUrl.ToString(),
            result.PublicId,
            result.Bytes,
            result.Format ?? string.Empty);
    }

    public async Task<IReadOnlyList<StoredImageResult>> UploadImagesAsync(IEnumerable<IFormFile> files, string? folder = null, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        var fileList = files?.ToList() ?? new List<IFormFile>();
        if (fileList.Count == 0)
        {
            throw new ProjectException(ResponseType.ImageCannotBeEmpty, "Danh sách ảnh không được rỗng");
        }

        var results = new List<StoredImageResult>(fileList.Count);
        foreach (var file in fileList)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await UploadImageAsync(file, folder, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    private void EnsureConfigured()
    {
        if (!_isConfigured)
        {
            throw new ProjectException(ResponseType.ServiceUnavailable, MissingCloudinaryConfigMessage);
        }
    }

    private static void ValidateFile(IFormFile? file)
    {
        if (file == null || file.Length <= 0)
        {
            throw new ProjectException(ResponseType.ImageCannotBeEmpty, "Ảnh không hợp lệ");
        }

        if (!AllowedMimeTypes.Contains(file.ContentType))
        {
            throw new ProjectException(ResponseType.InvalidData, "Định dạng ảnh không hỗ trợ. Chỉ chấp nhận jpeg, png, webp, gif, avif");
        }

        const long maxBytes = 10 * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            throw new ProjectException(ResponseType.InvalidData, "Kích thước ảnh tối đa là 10MB");
        }
    }
}
