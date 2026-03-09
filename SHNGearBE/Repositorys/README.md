# Repositories

This folder contains data access layer implementations using Repository pattern with Entity Framework Core.

---

## 📁 Repository Overview

All repositories:

- Inherit from `GenericRepository<TEntity>` for basic CRUD operations
- Use Entity Framework Core for data access
- Implement soft delete pattern (IsDelete flag)
- Support eager loading with `.Include()` and `.ThenInclude()`
- Use async/await for all database operations

---

## 1. **AccountRepository** (`AccountRepository.cs`)

**Interface**: `IAccountRepository`

**Purpose**: Data access for Account entity with role and permission relationships.

### Methods:

| Method                                   | Parameters        | Return Type      | Description                                         |
| ---------------------------------------- | ----------------- | ---------------- | --------------------------------------------------- |
| `GetByEmailAsync`                        | `string email`    | `Task<Account?>` | Get account by email address                        |
| `GetByUsernameAsync`                     | `string username` | `Task<Account?>` | Get account by username                             |
| `GetAccountWithRolesAndPermissionsAsync` | `Guid accountId`  | `Task<Account?>` | Get account with eager-loaded roles and permissions |
| `GetAccountWithDetailsAsync`             | `Guid accountId`  | `Task<Account?>` | Get account with roles (no permissions)             |
| `EmailExistsAsync`                       | `string email`    | `Task<bool>`     | Check if email exists (excluding deleted)           |
| `UsernameExistsAsync`                    | `string username` | `Task<bool>`     | Check if username exists (excluding deleted)        |

**Inherited from GenericRepository**:

- `GetByIdAsync(Guid id)` - Get by ID
- `GetAllAsync()` - Get all non-deleted entities
- `AddAsync(Account entity)` - Add new account
- `UpdateAsync(Account entity)` - Update existing account
- `DeleteAsync(Guid id)` - Soft delete account

### Key Features:

- **Eager Loading**: `GetAccountWithRolesAndPermissionsAsync` uses nested `.ThenInclude()` for complete data retrieval
  ```csharp
  .Include(a => a.AccountRoles)
      .ThenInclude(ar => ar.Role)
          .ThenInclude(r => r.RolePermissions)
              .ThenInclude(rp => rp.Permission)
  ```
- **Email/Username Uniqueness**: Fast existence checks without full entity loading
- **Soft Delete Awareness**: All queries filter by `!IsDelete`

**Notes**:

- ✅ Use `GetAccountWithRolesAndPermissionsAsync` when you need full authorization data
- ✅ Use `GetAccountWithDetailsAsync` for simpler queries (roles only)
- ⚠️ `GetByEmailAsync` and `GetByUsernameAsync` don't eager-load relationships - call `GetAccountWithRolesAndPermissionsAsync` separately if needed

---

## 2. **RoleRepository** (`RoleRepository.cs`)

**Interface**: `IRoleRepository`

**Purpose**: Data access for Role entity with permission relationships.

### Methods:

| Method                        | Parameters        | Return Type                     | Description                            |
| ----------------------------- | ----------------- | ------------------------------- | -------------------------------------- |
| `GetByNameAsync`              | `string roleName` | `Task<Role?>`                   | Get role by name                       |
| `GetRoleWithPermissionsAsync` | `Guid roleId`     | `Task<Role?>`                   | Get role with eager-loaded permissions |
| `GetRolesByAccountIdAsync`    | `Guid accountId`  | `Task<IEnumerable<Role>>`       | Get all roles for an account           |
| `GetPermissionsByRoleIdAsync` | `Guid roleId`     | `Task<IEnumerable<Permission>>` | Get all permissions for a role         |

**Inherited from GenericRepository**: Standard CRUD methods

### Key Features:

- **Permission Eager Loading**: `GetRoleWithPermissionsAsync` loads all permissions via join table
  ```csharp
  .Include(r => r.RolePermissions)
      .ThenInclude(rp => rp.Permission)
  ```
- **Account-Role Relationship**: Query roles by account ID
- **Role-Permission Relationship**: Query permissions by role ID

**Notes**:

- ✅ Use `GetRoleWithPermissionsAsync` when displaying role details
- ✅ Use `GetRolesByAccountIdAsync` for user authorization
- ⚠️ `GetByNameAsync` doesn't eager-load permissions - call `GetRoleWithPermissionsAsync` if needed

---

## 3. **PermissionRepository** (`PermissionRepository.cs`)

**Interface**: `IPermissionRepository`

**Purpose**: Data access for Permission entity and authorization checks.

### Methods:

| Method                           | Parameters                              | Return Type                     | Description                                    |
| -------------------------------- | --------------------------------------- | ------------------------------- | ---------------------------------------------- |
| `GetByNameAsync`                 | `string permissionName`                 | `Task<Permission?>`             | Get permission by name (e.g., "accounts.view") |
| `GetPermissionsByAccountIdAsync` | `Guid accountId`                        | `Task<IEnumerable<Permission>>` | Get all permissions for an account (via roles) |
| `HasPermissionAsync`             | `Guid accountId, string permissionName` | `Task<bool>`                    | Check if account has specific permission       |

**Inherited from GenericRepository**: Standard CRUD methods

### Key Features:

- **Authorization Check**: `HasPermissionAsync` performs complex query through AccountRoles → RolePermissions
  ```csharp
  .Where(a => a.Id == accountId && !a.IsDelete)
  .SelectMany(a => a.AccountRoles)
  .Where(ar => !ar.Role.IsDelete)
  .SelectMany(ar => ar.Role.RolePermissions)
  .Any(rp => rp.Permission.Name == permissionName && !rp.Permission.IsDelete)
  ```
- **Permission Aggregation**: `GetPermissionsByAccountIdAsync` returns union of all permissions from all roles

**Notes**:

- ✅ Use `HasPermissionAsync` for authorization checks (fast, optimized query)
- ✅ Use `GetPermissionsByAccountIdAsync` when displaying user permissions
- ✅ Permission names are unique and case-sensitive

---

## 4. **RefreshTokenRepository** (`RefreshTokenRepository.cs`)

**Interface**: `IRefreshTokenRepository`

**Purpose**: Data access for RefreshToken entity (JWT token management).

### Methods:

| Method                          | Parameters        | Return Type                       | Description                               |
| ------------------------------- | ----------------- | --------------------------------- | ----------------------------------------- |
| `GetByTokenAsync`               | `string token`    | `Task<RefreshToken?>`             | Get refresh token by token string         |
| `GetByJwtTokenAsync`            | `string jwtToken` | `Task<RefreshToken?>`             | Get refresh token by associated JWT token |
| `RevokeTokenAsync`              | `Guid tokenId`    | `Task`                            | Revoke a specific token                   |
| `RevokeAllUserTokensAsync`      | `Guid accountId`  | `Task`                            | Revoke all tokens for an account          |
| `GetActiveTokensByAccountAsync` | `Guid accountId`  | `Task<IEnumerable<RefreshToken>>` | Get all active tokens for an account      |

**Inherited from GenericRepository**: Standard CRUD methods

### Key Features:

- **Token Revocation**: Soft revoke (IsRevoked = true, RevokedAt = DateTime.UtcNow)
- **Multiple Token Support**: Users can have multiple active tokens (different devices)
- **Token Validation**: Check expiry and revocation status

**Notes**:

- ✅ Use `RevokeAllUserTokensAsync` on password change or security events
- ✅ Use `GetActiveTokensByAccountAsync` for admin monitoring
- ⚠️ Tokens have 7-day expiry (configured in AuthService)
- ⚠️ Revoked tokens cannot be reused (check IsRevoked before refresh)

---

## 5. **ProductRepository** (`ProductRepository.cs`)

**Interface**: `IProductRepository`

**Purpose**: Data access for Product entity with complex relationships (categories, brands, variants, tags).

### Methods:

| Method                    | Parameters                                                                                   | Return Type                    | Description                        |
| ------------------------- | -------------------------------------------------------------------------------------------- | ------------------------------ | ---------------------------------- |
| `GetByIdWithDetailsAsync` | `Guid id, CancellationToken`                                                                 | `Task<Product?>`               | Get product with all relationships |
| `GetBySlugAsync`          | `string slug, CancellationToken`                                                             | `Task<Product?>`               | Get product by URL slug            |
| `CodeOrSlugExistsAsync`   | `string code, string slug, Guid? excludeProductId, CancellationToken`                        | `Task<bool>`                   | Check if code or slug exists       |
| `GetPagedAsync`           | `int skip, int take, CancellationToken`                                                      | `Task<IReadOnlyList<Product>>` | Get paginated products             |
| `SearchPagedAsync`        | `string? searchTerm, Guid? categoryId, Guid? brandId, int skip, int take, CancellationToken` | `Task<IReadOnlyList<Product>>` | Search with filters                |
| `CountFilteredAsync`      | `string? searchTerm, Guid? categoryId, Guid? brandId, CancellationToken`                     | `Task<int>`                    | Count filtered results             |
| `CountActiveAsync`        | `CancellationToken`                                                                          | `Task<int>`                    | Count active products              |
| `GetTagsByNamesAsync`     | `IEnumerable<string> names, CancellationToken`                                               | `Task<IReadOnlyList<Tag>>`     | Get or create tags                 |
| `VariantSkuExistsAsync`   | `string sku, Guid? excludeVariantId, CancellationToken`                                      | `Task<bool>`                   | Check variant SKU uniqueness       |

**Inherited from GenericRepository**: Standard CRUD methods

### Key Features:

- **Complex Eager Loading**: Loads Category, Brand, Variants, Tags, Images
  ```csharp
  .Include(p => p.Category)
  .Include(p => p.Brand)
  .Include(p => p.Variants)
  .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
  .Include(p => p.ProductImages)
  ```
- **Search & Filtering**: Full-text search on Name and Description with category/brand filters
- **Pagination**: Efficient paging with skip/take
- **SEO-Friendly**: Slug-based product lookup

**Notes**:

- ✅ Use `GetBySlugAsync` for SEO-friendly URLs
- ✅ Use `SearchPagedAsync` for product catalog with filters
- ⚠️ Variants have unique SKUs (enforced by `VariantSkuExistsAsync`)
- ⚠️ Product codes and slugs must be unique

---

## 🔄 Generic Repository (`GenericRepository<T>`)

**Location**: `Repositorys/GenericRepository.cs`

**Purpose**: Base repository providing common CRUD operations for all entities.

### Methods:

| Method                  | Return Type            | Description                               |
| ----------------------- | ---------------------- | ----------------------------------------- |
| `GetByIdAsync(Guid id)` | `Task<T?>`             | Get entity by ID (excluding soft-deleted) |
| `GetAllAsync()`         | `Task<IEnumerable<T>>` | Get all non-deleted entities              |
| `AddAsync(T entity)`    | `Task<T>`              | Add new entity                            |
| `UpdateAsync(T entity)` | `Task`                 | Update existing entity                    |
| `DeleteAsync(Guid id)`  | `Task`                 | Soft delete entity (sets IsDelete = true) |

### Key Features:

- **Soft Delete**: All delete operations set `IsDelete = true` instead of removing records
- **Automatic Filtering**: All queries filter by `!IsDelete` using `_dbSet.Where(e => !e.IsDelete)`
- **Generic Implementation**: Works with any entity inheriting from `BaseEntity`

**Notes**:

- ✅ All entities must inherit from `BaseEntity` (provides Id, IsDelete, CreatedAt, UpdatedAt)
- ✅ Soft delete preserves data for audit trails
- ⚠️ Override `DeleteAsync` if you need hard delete for specific entities

---

## 📊 Repository Pattern Benefits

### 1. **Abstraction**:

- Separates data access logic from business logic
- Services depend on interfaces, not concrete implementations
- Easy to mock for unit testing

### 2. **Consistency**:

- All repositories follow the same pattern
- Soft delete implemented uniformly
- Standard error handling

### 3. **Eager Loading**:

- Specialized methods for loading relationships
- Prevents N+1 query problems
- Optimized queries for specific use cases

### 4. **Testability**:

- Interfaces can be mocked easily
- Unit tests don't require database
- Integration tests can use InMemoryDatabase

---

## ⚠️ Important Notes

### Redundancy Analysis:

1. **GetByIdAsync vs GetById (from GenericRepository)**:
   - **Not redundant** - Generic method returns basic entity, specialized methods load relationships
   - **Recommendation**: Use generic for simple queries, specialized for complex data needs

2. **Multiple "Get" Methods**:
   - Each serves different purposes (by ID, by name, with/without relationships)
   - **Recommendation**: Choose method based on data needs to avoid over-fetching

3. **Existence Checks vs Get Methods**:
   - `EmailExistsAsync` is faster than `GetByEmailAsync != null`
   - **Recommendation**: Use existence checks for validation, Get methods for data retrieval

### Usage Guidelines:

- ✅ **Use specialized methods** when you need relationships (e.g., `GetAccountWithRolesAndPermissionsAsync`)
- ✅ **Use generic methods** for simple CRUD operations
- ✅ **Use existence checks** for validation before operations
- ⚠️ **Avoid** calling Get methods just to check existence
- ⚠️ **Avoid** lazy loading - use eager loading with `.Include()` instead

### Performance Considerations:

1. **Eager Loading**:
   - Use `.Include()` for required relationships
   - Avoid over-fetching data (only load what you need)
   - Consider using `.Select()` for projection when you don't need full entities

2. **Pagination**:
   - Always use pagination for lists (avoid loading all records)
   - Use `Skip()` and `Take()` with count query

3. **Soft Delete Filtering**:
   - Automatically applied to all queries
   - No need to manually check `IsDelete` flag in services

---

## 🔧 Entity Framework Core Features Used

### 1. **DbSet Operations**:

```csharp
_dbSet.Add(entity);           // Add new entity
_dbSet.Update(entity);        // Update existing entity
_dbSet.Remove(entity);        // Hard delete (not used - we use soft delete)
```

### 2. **Eager Loading**:

```csharp
_dbSet
    .Include(e => e.RelatedEntity)
    .ThenInclude(r => r.NestedEntity)
    .Where(e => !e.IsDelete)
    .FirstOrDefaultAsync();
```

### 3. **Querying**:

```csharp
_dbSet
    .Where(e => e.Property == value && !e.IsDelete)
    .OrderBy(e => e.CreatedAt)
    .Skip(skip)
    .Take(take)
    .ToListAsync();
```

### 4. **Projection**:

```csharp
_dbSet
    .Where(e => !e.IsDelete)
    .Select(e => new Dto { ... })
    .ToListAsync();
```

---

## 🎯 Best Practices

1. **Always use async methods**: All database operations should be async
2. **Filter soft-deleted records**: Use `.Where(e => !e.IsDelete)` in all queries
3. **Eager load relationships**: Use `.Include()` instead of lazy loading
4. **Use specific methods**: Choose specialized methods over generic when you need relationships
5. **Validate before operations**: Check existence before add/update/delete
6. **Use CancellationToken**: Pass CancellationToken for long-running queries
7. **Return null for not found**: Don't throw exceptions from repositories (let services handle it)

---

## 📦 Database Tables

### Core Tables:

- **Accounts**: User accounts
- **Roles**: Authorization roles
- **Permissions**: Authorization permissions
- **RefreshTokens**: JWT refresh tokens

### Join Tables (Many-to-Many):

- **AccountRoles**: Account ↔ Role relationships
- **RolePermissions**: Role ↔ Permission relationships

### Product Tables:

- **Products**: Product catalog
- **Categories**: Product categories
- **Brands**: Product brands
- **Variants**: Product variants (SKU, price, stock)
- **Tags**: Product tags
- **ProductTags**: Product ↔ Tag relationships
- **ProductImages**: Product images

All tables inherit from `BaseEntity` (Id, IsDelete, CreatedAt, UpdatedAt).
