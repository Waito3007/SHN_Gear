namespace SHNGearBE.Models.DTOs.Permission;

public class CreatePermissionRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdatePermissionRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreateAt { get; set; }
    public DateTime? UpdateAt { get; set; }
}
