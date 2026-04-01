using Microsoft.AspNetCore.Mvc;
using SHNGearBE.Helpers.Attributes;
using SHNGearBE.Models.DTOs.Media;
using SHNGearBE.Services.Interfaces.Media;

namespace SHNGearBE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IImageStorageService _imageStorageService;

    public MediaController(IImageStorageService imageStorageService)
    {
        _imageStorageService = imageStorageService;
    }

    [RequirePermission(Permissions.EditProduct)]
    [HttpPost("images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(15 * 1024 * 1024)]
    public async Task<ActionResult<IReadOnlyList<ImageUploadResponse>>> UploadImages(
        [FromForm] List<IFormFile> files,
        [FromForm] string? folder,
        CancellationToken cancellationToken)
    {
        var uploadResults = await _imageStorageService.UploadImagesAsync(files, folder, cancellationToken);
        var response = uploadResults
            .Select(x => new ImageUploadResponse
            {
                Url = x.Url,
                PublicId = x.PublicId,
                Bytes = x.Bytes,
                Format = x.Format
            })
            .ToList();

        return Ok(response);
    }
}
