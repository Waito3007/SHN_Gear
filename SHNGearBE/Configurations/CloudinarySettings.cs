namespace SHNGearBE.Configurations;

public class CloudinarySettings
{
    public const string SectionName = "Cloudinary";

    public string? CloudName { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CloudName)
            && !string.IsNullOrWhiteSpace(ApiKey)
            && !string.IsNullOrWhiteSpace(ApiSecret);
    }
}
