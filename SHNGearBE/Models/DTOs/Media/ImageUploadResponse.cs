namespace SHNGearBE.Models.DTOs.Media;

public class ImageUploadResponse
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
    public long Bytes { get; set; }
    public string Format { get; set; } = string.Empty;
}
