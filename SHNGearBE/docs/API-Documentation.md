# 📚 SHNGear API Documentation

## 📋 Table of Contents

- [Authentication (Auth)](#-authentication-auth)
- [Account Management](#-account-management)
- [Repository Pattern](#-repository-pattern)
- [Redis Cache Service](#-redis-cache-service)
- [Product Repository with Cache](#-product-repository-with-cache)

---

## 🔐 Authentication (Auth)

### Base URL: `/api/Auth`

| Method | Endpoint           | Auth         | Description            |
| ------ | ------------------ | ------------ | ---------------------- |
| `POST` | `/register`        | ❌ Anonymous | Register a new account |
| `POST` | `/login`           | ❌ Anonymous | User login             |
| `POST` | `/refresh-token`   | ❌ Anonymous | Refresh JWT token      |
| `POST` | `/logout`          | ✅ Required  | User logout            |
| `POST` | `/change-password` | ✅ Required  | Change password        |

### API Details

#### `POST /api/Auth/register`

Register a new account in the system.

**Request Body:**

```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "firstName": "string",
  "name": "string"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "accessToken": "jwt-token",
    "refreshToken": "refresh-token",
    "expiresIn": 3600
  }
}
```

---

#### `POST /api/Auth/login`

Login with email/username and password.

**Request Body:**

```json
{
  "emailOrUsername": "string",
  "password": "string"
}
```

**Response:**

```json
{
  "success": true,
  "data": {
    "accessToken": "jwt-token",
    "refreshToken": "refresh-token",
    "expiresIn": 3600
  }
}
```

---

#### `POST /api/Auth/refresh-token`

Refresh access token when expired.

**Request Body:**

```json
{
  "refreshToken": "string"
}
```

**Response:** Same as login response.

---

#### `POST /api/Auth/logout`

Logout and revoke all refresh tokens.

**Headers:** `Authorization: Bearer {accessToken}`

**Response:**

```json
{
  "success": true,
  "data": {
    "message": "Logged out successfully"
  }
}
```

---

#### `POST /api/Auth/change-password`

Change current user's password.

**Headers:** `Authorization: Bearer {accessToken}`

**Request Body:**

```json
{
  "currentPassword": "string",
  "newPassword": "string"
}
```

---

## 👤 Account Management

### Base URL: `/api/Account`

| Method   | Endpoint                      | Auth        | Permission      | Description                  |
| -------- | ----------------------------- | ----------- | --------------- | ---------------------------- |
| `GET`    | `/me`                         | ✅ Required | -               | Get current user profile     |
| `PUT`    | `/me`                         | ✅ Required | -               | Update current user profile  |
| `GET`    | `/`                           | ✅ Required | `ViewAccounts`  | Get all accounts             |
| `GET`    | `/{id}`                       | ✅ Required | `ViewAccounts`  | Get account by ID            |
| `DELETE` | `/{id}`                       | ✅ Required | `DeleteAccount` | Delete account (soft delete) |
| `POST`   | `/{accountId}/roles/{roleId}` | ✅ Required | `ManageRoles`   | Assign role to account       |
| `DELETE` | `/{accountId}/roles/{roleId}` | ✅ Required | `ManageRoles`   | Remove role from account     |

### API Details

#### `GET /api/Account/me`

Get profile information of the currently logged-in user.

**Response:**

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "username": "string",
    "email": "string",
    "firstName": "string",
    "name": "string",
    "phoneNumber": "string",
    "address": "string",
    "roles": ["Admin", "User"]
  }
}
```

---

#### `PUT /api/Account/me`

Update profile information.

**Request Body:**

```json
{
  "firstName": "string",
  "name": "string",
  "phoneNumber": "string",
  "address": "string"
}
```

---

#### `POST /api/Account/{accountId}/roles/{roleId}`

Assign a role to an account.

**Path Parameters:**

- `accountId`: GUID of the account
- `roleId`: GUID of the role to assign

---

## 🗄️ Repository Pattern

### Purpose

Repository Pattern separates data access logic from business logic, making code easier to test and maintain.

### IGenericRepository<T>

Base interface for all repositories.

```csharp
public interface IGenericRepository<T> where T : BaseEntity
{
    // Get entity by ID (automatically filters soft-deleted)
    Task<T?> GetByIdAsync(Guid id);

    // Add new entity (auto-sets CreateAt)
    Task<T> AddAsync(T entity);

    // Update entity (auto-sets UpdateAt)
    Task UpdateAsync(T entity);

    // Soft delete (sets IsDelete = true)
    Task DeleteAsync(Guid id);
}
```

### IAccountRepository

Repository for Account entity.

```csharp
public interface IAccountRepository : IGenericRepository<Account>
{
    // Find account by email
    Task<Account?> GetByEmailAsync(string email);

    // Find account by username
    Task<Account?> GetByUsernameAsync(string username);

    // Get account with full roles and permissions (eager loading)
    Task<Account?> GetAccountWithRolesAndPermissionsAsync(Guid accountId);

    // Get account with details (AccountDetail)
    Task<Account?> GetAccountWithDetailsAsync(Guid accountId);

    // Check if email already exists
    Task<bool> EmailExistsAsync(string email);

    // Check if username already exists
    Task<bool> UsernameExistsAsync(string username);
}
```

### IProductRepository

Repository for Product entity with caching support.

```csharp
public interface IProductRepository : IGenericRepository<Product>
{
    // === Basic Methods ===

    // Get product with full details (Category, Brand, Variants, Images, Tags, Attributes)
    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct);

    // Find product by slug
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct);

    // Check if code or slug already exists
    Task<bool> CodeOrSlugExistsAsync(string code, string slug, Guid? excludeId, CancellationToken ct);

    // Paginate products
    Task<IReadOnlyList<Product>> GetPagedAsync(int skip, int take, CancellationToken ct);

    // Count active products
    Task<int> CountActiveAsync(CancellationToken ct);

    // Search with filters
    Task<IReadOnlyList<Product>> SearchPagedAsync(string? searchTerm, Guid? categoryId, Guid? brandId, int skip, int take, CancellationToken ct);

    // Count filtered results
    Task<int> CountFilteredAsync(string? searchTerm, Guid? categoryId, Guid? brandId, CancellationToken ct);

    // Get tags by names
    Task<IReadOnlyList<Tag>> GetTagsByNamesAsync(IEnumerable<string> names, CancellationToken ct);

    // Check if variant SKU already exists
    Task<bool> VariantSkuExistsAsync(string sku, Guid? excludeVariantId, CancellationToken ct);

    // === Cached Methods ===

    // Get featured products (cached 15 minutes)
    Task<IReadOnlyList<Product>> GetFeaturedProductsCachedAsync(int take = 10, CancellationToken ct);

    // Get best-selling products (cached 15 minutes)
    Task<IReadOnlyList<Product>> GetTopSellingProductsCachedAsync(int take = 10, CancellationToken ct);

    // Get newest products (cached 15 minutes)
    Task<IReadOnlyList<Product>> GetNewestProductsCachedAsync(int take = 10, CancellationToken ct);

    // Get product details (cached 30 minutes)
    Task<Product?> GetByIdWithDetailsCachedAsync(Guid id, CancellationToken ct);

    // Get product by slug (cached 30 minutes)
    Task<Product?> GetBySlugCachedAsync(string slug, CancellationToken ct);

    // === Cache Invalidation ===

    // Invalidate cache when data changes (Create/Update/Delete)
    Task InvalidateProductCacheAsync(Guid? productId = null, string? slug = null);
}
```

---

## 🔴 Redis Cache Service

### Purpose

Distributed caching with Redis to improve performance and reduce database load.

### Configuration

```json
// appsettings.json
{
  "Redis": {
    "ConnectionString": "localhost:6379,abortConnect=false,connectTimeout=5000",
    "InstanceName": "SHNGear_",
    "DefaultExpirationMinutes": 30
  }
}
```

### ICacheService Interface

```csharp
public interface ICacheService
{
    // === Basic Operations ===

    // Get value from cache
    // Returns: null if not found or expired
    Task<T?> GetAsync<T>(string key);

    // Store value in cache with expiration time
    // expiration: null = uses DefaultExpirationMinutes
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    // Remove key from cache
    Task RemoveAsync(string key);

    // Check if key exists
    Task<bool> ExistsAsync(string key);

    // Remove all keys matching pattern
    // Example: "products:*" removes all cache starting with "products:"
    Task RemoveByPatternAsync(string pattern);

    // === Helper Methods ===

    // Cache-aside pattern: Get from cache, if miss then call factory and cache result
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    // Distributed lock: Acquire lock with expiration time
    // Returns: true if lock acquired, false if already locked
    Task<bool> AcquireLockAsync(string key, TimeSpan expiry);

    // Release distributed lock
    Task ReleaseLockAsync(string key);
}
```

### Usage Examples

#### 1. Cache-Aside Pattern (Recommended)

```csharp
// Automatic: cache hit → return, cache miss → query DB → cache → return
var products = await _cacheService.GetOrSetAsync(
    key: "products:featured:10",
    factory: async () => await _dbContext.Products
        .Where(p => p.IsFeatured && !p.IsDelete)
        .Take(10)
        .ToListAsync(),
    expiration: TimeSpan.FromMinutes(15)
);
```

#### 2. Manual Cache Operations

```csharp
// Get
var product = await _cacheService.GetAsync<Product>("product:123");

// Set
await _cacheService.SetAsync("product:123", product, TimeSpan.FromMinutes(30));

// Remove
await _cacheService.RemoveAsync("product:123");

// Remove by pattern
await _cacheService.RemoveByPatternAsync("products:*");
```

#### 3. Distributed Lock

```csharp
// Prevent race conditions
if (await _cacheService.AcquireLockAsync("order:process:123", TimeSpan.FromMinutes(5)))
{
    try
    {
        // Process order...
    }
    finally
    {
        await _cacheService.ReleaseLockAsync("order:process:123");
    }
}
```

---

## 📦 Product Repository with Cache

### Cache Keys Structure

| Key Pattern                  | TTL    | Description                |
| ---------------------------- | ------ | -------------------------- |
| `products:featured:{take}`   | 15 min | Featured products list     |
| `products:topselling:{take}` | 15 min | Best-selling products list |
| `products:newest:{take}`     | 15 min | Newest products list       |
| `products:detail:{id}`       | 30 min | Product details by ID      |
| `products:slug:{slug}`       | 30 min | Product details by slug    |

### Cache Invalidation Flow

```
┌────────────────────────────────────────────────────────────┐
│                    WRITE OPERATION                          │
│  (CreateAsync / UpdateAsync / DeleteAsync)                  │
└────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────────┐
│  1. Save to Database (CommitAsync)                          │
└────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────────┐
│  2. InvalidateProductCacheAsync(productId, slug)            │
│     ├── Remove: products:featured:*                         │
│     ├── Remove: products:topselling:*                       │
│     ├── Remove: products:newest:*                           │
│     ├── Remove: products:detail:{productId}                 │
│     └── Remove: products:slug:{slug}                        │
└────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────────┐
│  3. Next READ → Cache MISS → Query DB → Store in Cache      │
└────────────────────────────────────────────────────────────┘
```

### Code Example

```csharp
// ProductService.cs
public async Task<ProductDetailResponse> UpdateAsync(UpdateProductRequest request, CancellationToken ct)
{
    var product = await _productRepository.GetByIdWithDetailsAsync(request.Id, ct);

    // Update product...

    await _unitOfWork.BeginTransactionAsync();
    try
    {
        await _unitOfWork.CommitAsync();

        // ✅ Invalidate cache after successful commit
        await _productRepository.InvalidateProductCacheAsync(product.Id, product.Slug);
    }
    catch
    {
        await _unitOfWork.RollbackAsync();
        throw;
    }

    return MapToDetailResponse(product);
}
```

---

## 📊 Response Format

### Success Response

```json
{
  "success": true,
  "message": "Success",
  "data": { ... }
}
```

### Error Response

```json
{
  "success": false,
  "message": "Error message",
  "errorCode": 4001,
  "errors": ["Detail error 1", "Detail error 2"]
}
```

### Common Error Codes

| Code | Description           |
| ---- | --------------------- |
| 4001 | Invalid data          |
| 4010 | Unauthorized          |
| 4030 | Forbidden             |
| 4040 | Not found             |
| 4090 | Conflict (duplicate)  |
| 5000 | Internal server error |

---

## 🔒 Permission System

### Available Permissions

| Permission       | Description                                    |
| ---------------- | ---------------------------------------------- |
| `ViewAccounts`   | View list of accounts                          |
| `DeleteAccount`  | Delete accounts                                |
| `ManageRoles`    | Manage roles (assign/remove role from account) |
| `ViewRoles`      | View list of roles                             |
| `CreateRole`     | Create new role                                |
| `UpdateRole`     | Update role                                    |
| `DeleteRole`     | Delete role                                    |
| `ManageProducts` | Manage products (CRUD)                         |
| `ViewProducts`   | View products                                  |

### Usage in Controller

```csharp
[HttpGet]
[RequirePermission(Permissions.ViewAccounts)]  // Requires permission
public async Task<IActionResult> GetAllAccounts()
{
    // ...
}

[HttpGet("public")]
[AllowAnonymous]  // Anyone can access
public async Task<IActionResult> GetPublicData()
{
    // ...
}
```

---

## 🐳 Docker Services

```yaml
# docker-compose.yml
services:
  postgres_container:
    image: postgres:15
    ports: 5433:5432

  redis_container:
    image: redis:7-alpine
    ports: 6379:6379
    command: redis-server --appendonly yes

  pgadmin:
    image: dpage/pgadmin4
    ports: 5050:80
```

---

_Documentation auto-generated - Updated: February 2026_
