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
    public const string ViewAccounts = "account.view";
    public const string CreateAccount = "account.create";
    public const string EditAccount = "account.update";
    public const string DeleteAccount = "account.delete";
    public const string ManageRoles = "account.update";

    // Role permissions
    public const string ViewRoles = "role.view";
    public const string CreateRole = "role.create";
    public const string EditRole = "role.update";
    public const string DeleteRole = "role.delete";
    public const string AssignPermissions = "permission.assign";

    // Permission permissions
    public const string ViewPermissions = "permission.view";
    public const string CreatePermission = "permission.assign";
    public const string EditPermission = "permission.assign";
    public const string DeletePermission = "permission.assign";
    public const string ManageRolePermissions = "permission.assign";

    // Product permissions (example)
    public const string ViewProducts = "product.view";
    public const string CreateProduct = "product.create";
    public const string EditProduct = "product.update";
    public const string DeleteProduct = "product.delete";

    // Brand permissions
    public const string ViewBrands = "brand.view";
    public const string CreateBrand = "brand.create";
    public const string EditBrand = "brand.update";
    public const string DeleteBrand = "brand.delete";
    public const string ManageBrands = "brand.manage";

    // Category permissions
    public const string ViewCategories = "category.view";
    public const string CreateCategory = "category.create";
    public const string EditCategory = "category.update";
    public const string DeleteCategory = "category.delete";
    public const string ManageCategories = "category.manage";

    // Order permissions
    public const string CreateOrder = "order.create";
    public const string ViewMyOrders = "order.view.mine";
    public const string ViewAllOrders = "order.view.all";
    public const string ManageOrders = "order.manage";
}
