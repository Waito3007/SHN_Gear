namespace SHNGearBE.Models.DTOs.Account;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public IEnumerable<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}

public class CreateRoleRequestDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<Guid> PermissionIds { get; set; } = new();
}

public class UpdateRoleRequestDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public List<Guid>? PermissionIds { get; set; }
}

public class CreatePermissionRequestDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
}
