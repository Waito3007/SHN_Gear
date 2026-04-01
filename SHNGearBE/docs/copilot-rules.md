# GitHub Copilot Rules (SHNGear)

## Core Principles

- Follow SOLID and Clean Architecture: Controllers thin; Application/Domain hold business logic; Infrastructure only persistence/IO; UI never references EF types.
- Prefer async all the way; pass CancellationToken from controller to repo.
- No hardcoded secrets/endpoints/contact info; bind from configuration via options classes and validate at startup.
- Use dependency injection; avoid static singletons and service locator.

## Layering Contracts

- Controllers: validate request DTOs, call use-case/service, map response DTOs, do not touch DbContext or entities directly.
- Application Services: orchestrate repos + unit of work; log start/end/exception; no HTTP/EF/DB types leaking out.
- Domain: entities/logic, no framework dependencies.
- Infrastructure: EF Core repositories, configurations, external clients; keep logic persistence-specific only.

## Repositories & Unit of Work

- Use `IGenericRepository<T>` for CRUD; always filter out soft-deleted (`IsDelete == true`).
- `GetByIdAsync` returns null when not found or soft-deleted; do not nullify valid entities.
- Soft delete: set `IsDelete = true`, `UpdateAt = UtcNow`; provide `Restore` if needed.
- `IUnitOfWork` handles transactions; services call `BeginTransactionAsync/CommitAsync/RollbackAsync` when a use-case spans multiple repos. Controllers never call UoW directly.
- Keep timestamps at service layer if you want repos persistence-only; if repos set timestamps, keep it consistent and documented.

## DTO Pattern

- Each endpoint/use-case has paired DTOs: `XxxRequest`, `XxxResponse` (no entities as DTOs).
- Request DTO only contains necessary inputs; Response DTO includes data + metadata (paging, message) as needed.
- Mapping via mapper (AutoMapper/Mapster) in Application layer; no manual mapping in controllers unless trivial.
- Place DTOs under the relevant folder (e.g., Models/DTOs/Account). Do not share DTOs across bounded contexts without a contract.

## Logging (SANBGLog)

- Register `AddTypedLogService(configuration)`; inject `ILogService<T>` into services/controllers.
- Log at service level: start, success, exception; avoid spamming in repositories (only data anomalies).
- Respect config filters; do not log sensitive data unmasked.

## Configuration

- Define config sections (contact info/endpoints/third-party keys) in `appsettings.*` and bind to options classes.
- Validate options on startup; no magic strings for section names (use constants).

## Error Handling

- Use middleware to map exceptions to API responses; do not expose stack traces.
- Throw domain-specific exceptions with meaningful messages; do not swallow exceptions.

## Coding Style

- Naming: interfaces `IName`; repositories `XxxRepository`; services `XxxService`; async methods end with `Async`.
- Use `DateTime.UtcNow` not `DateTime.Now` for persistence.
- Keep methods small; extract private helpers when branching is non-trivial.

## Testing

- Unit test services/use-cases with mocked repos/UoW/log service.
- Integration test critical flows (transactions, soft-delete, auth).

## Pull Requests

- Include: what changed, why, tests run; keep diffs minimal and cohesive.

## Product DB Design (proposed)

- Tables: `Products`, `Categories`, `Brands`, `ProductImages`, `ProductPrices`, `Inventories`, `Reviews`, `Tags`, `ProductTags` (bridge), `Attributes`, `ProductAttributes` (value per product), `Orders`/`OrderItems` (if in scope), `Suppliers` (optional).
- Keys & relations:
  - `Products`: Id (PK, GUID), `CategoryId` FK, `BrandId` FK, `Code` (unique), `Name`, `Slug`, `Description`, `IsDelete`, `CreateAt`, `UpdateAt`.
  - `Categories`: Id PK, `ParentCategoryId` nullable FK, `Name`, `Slug` unique.
  - `Brands`: Id PK, `Name` unique, `Description`.
  - `ProductImages`: Id PK, `ProductId` FK, `Url`, `IsPrimary`, `SortOrder`.
  - `ProductPrices`: Id PK, `ProductId` FK, `Currency`, `BasePrice`, `SalePrice` nullable, `ValidFrom`, `ValidTo` nullable (supports price history).
  - `Inventories`: Id PK, `ProductId` FK, `Sku` unique, `Quantity`, `Location`/`WarehouseId`, `SafetyStock`.
  - `Reviews`: Id PK, `ProductId` FK, `UserId`, `Rating` (1-5), `Comment`, `IsApproved`, `CreateAt`.
  - `Tags`: Id PK, `Name` unique; `ProductTags` bridge (ProductId, TagId) PK.
  - `Attributes`: Id PK, `Name`, `DataType` (text/number/bool/options); `ProductAttributes`: Id PK, ProductId FK, AttributeId FK, `Value` (string) to allow flexible specs.
- Indexing: unique indexes on `Products.Code`, `Products.Slug`, `Inventories.Sku`; indexes on FK columns; full-text/index on `Products.Name/Description` if supported.
- Soft-delete: `IsDelete` on main entities; queries must filter it.
- Auditing: `CreateAt`, `UpdateAt`, optionally `CreatedBy`, `UpdatedBy`.

## Sample EF Config Hints

- Use separate `EntityTypeConfiguration` classes per aggregate (e.g., `ProductConfiguration`) with Fluent API.
- Configure decimal precision for prices (e.g., `HasColumnType("decimal(18,2)")`).
- Use `OnDelete(DeleteBehavior.Restrict)` for critical relations to prevent cascade loss; soft-delete instead.
- Seed minimal lookup data via configuration classes when needed.

## API Integration & Frontend Mapping

**CRITICAL RULE: Always read backend response structure before writing frontend mapping code**

1. **Before writing any FE component that consumes an API endpoint:**
   - Locate the backend controller method and trace its return type
   - Check if it returns `Ok(new ApiResponse<T>(...))` wrapper or raw data directly
   - Use Postman/PowerShell/browser DevTools to inspect actual JSON response from backend
   - Document the full response structure including all field names, types, and nesting levels

2. **Response Patterns in This Codebase:**
   - **Admin APIs (Account/Role/Permission):** Return `ApiResponse<T>` wrapper
     - Access path in FE: `response.data.data` (one data = DTO array, second data = array content)
     - Example: `const accounts = response.data.data` (array of accounts)
   - **Product API:** Returns raw `PagedResult<T>` without wrapper (EXCEPTION to admin pattern)
     - Access path in FE: `response.data` (direct access to PagedResult with items array)
     - Example: `const items = response.data.items`
     - Root cause: ProductController does not wrap response in ApiResponse<T>

3. **Type declarations must match backend response exactly:**
   - If backend wraps: `api.get<ApiResponse<T>>()` 
   - If backend returns raw: `api.get<PagedResult<T>>()`
   - Verify by checking the actual HTTP response, not assumptions

4. **Before committing FE code consuming a new or modified endpoint:**
   - Run backend locally and make test requests to verify response structure
   - Compare response structure with API type declarations
   - Test components in browser to catch runtime `n.map is not a function` errors early
   - Use safe navigation operators (`?.`) while developing until response structure is fully validated

5. **Common mistakes to avoid:**
   - Assuming all endpoints use the same response wrapper (Product is different)
   - Reading `.data` once when endpoint wraps in ApiResponse (need `.data.data`)
   - Reading `.data.data` when endpoint returns raw (should be `.data`)
   - Not checking if response contains array vs object (affects `.map()` calls)
   - Duplicating API prefix in endpoint path (axios baseURL is `/api`, don't add `/api` again)

## Copilot Prompting Tips

- When asking Copilot, remind: "Use request/response DTOs, soft-delete filter, ILogService<T> for logging, use options for config, follow Clean Architecture layers." Keep snippets inside Application/Infra boundaries.
- When building FE API integration: "First show me the backend response structure, then write the FE consumption code." Always verify response type before writing components.
