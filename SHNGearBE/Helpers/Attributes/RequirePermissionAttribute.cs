using Microsoft.AspNetCore.Authorization;

namespace SHNGearBE.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public string Permission { get; }

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = $"RequirePermission:{permission}";
    }
}

/// <summary>
/// Common permission constants
/// </summary>
public static class Permissions
{
    // Account permissions
    public const string ViewAccounts = "accounts.view";
    public const string CreateAccount = "accounts.create";
    public const string EditAccount = "accounts.edit";
    public const string DeleteAccount = "accounts.delete";
    public const string ManageRoles = "accounts.manage_roles";

    // Role permissions
    public const string ViewRoles = "roles.view";
    public const string CreateRole = "roles.create";
    public const string EditRole = "roles.edit";
    public const string DeleteRole = "roles.delete";
    public const string AssignPermissions = "roles.assign_permissions";

    // Permission permissions
    public const string ViewPermissions = "permissions.view";
    public const string CreatePermission = "permissions.create";
    public const string EditPermission = "permissions.edit";
    public const string DeletePermission = "permissions.delete";
    public const string ManageRolePermissions = "roles.manage_permissions";

    // Product permissions (example)
    public const string ViewProducts = "products.view";
    public const string CreateProduct = "products.create";
    public const string EditProduct = "products.edit";
    public const string DeleteProduct = "products.delete";
}
