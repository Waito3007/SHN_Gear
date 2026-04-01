using BackgroundLogService.Abstractions;
using Microsoft.EntityFrameworkCore;
using SHNGearBE.Models.DTOs.Common;
using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Product;
using SHNGearBE.UnitOfWork;
using SHNGearBE.Services.Interfaces;

namespace SHNGearBE.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogService<ProductService> _logService;

    public ProductService(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogService<ProductService> logService)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logService = logService;
    }

    public async Task<ProductDetailResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        ValidateVariants(request.Variants);

        var exists = await _productRepository.CodeOrSlugExistsAsync(request.Code, request.Slug, null, cancellationToken);
        if (exists)
        {
            throw new ProjectException(ResponseType.AlreadyExists, "Code hoặc slug đã tồn tại");
        }

        await EnsureVariantSkuUniqueAcrossProductsAsync(request.Variants, null, cancellationToken);

        var product = MapToEntity(request);

        await UpdateTagsAsync(product, request.Tags, cancellationToken);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _productRepository.AddAsync(product);
            await _unitOfWork.CommitAsync();

            // Invalidate cache after successful commit
            await _productRepository.InvalidateProductCacheAsync(product.Id, product.Slug);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        await _logService.WriteMessageAsync($"Product created: {product.Id} - {product.Name}");

        var created = await _productRepository.GetByIdWithDetailsAsync(product.Id, cancellationToken);
        return MapToDetailResponse(created!);
    }

    public async Task<ProductDetailResponse> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        ValidateVariants(request.Variants);

        for (var attempt = 1; attempt <= 2; attempt++)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);
            if (product == null)
            {
                throw new ProjectException(ResponseType.ProductNotFound);
            }

            var exists = await _productRepository.CodeOrSlugExistsAsync(request.Code, request.Slug, request.Id, cancellationToken);
            if (exists)
            {
                throw new ProjectException(ResponseType.AlreadyExists, "Code hoặc slug đã tồn tại");
            }

            await EnsureVariantSkuUniqueAcrossProductsAsync(request.Variants, product, cancellationToken);

            product.Code = request.Code;
            product.Name = request.Name;
            product.Slug = request.Slug;
            product.Description = request.Description;
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;
            product.UpdateAt = DateTime.UtcNow;

            await UpdateImagesAsync(product.Id, request.ImageUrls, cancellationToken);
            await UpdateTagsAsync(product, request.Tags, cancellationToken);
            UpdateAttributes(product, request.Attributes);
            UpdateVariants(product, request.Variants);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.CommitAsync();

                // Invalidate cache after successful commit
                await _productRepository.InvalidateProductCacheAsync(product.Id, product.Slug);

                await _logService.WriteMessageAsync($"Product updated: {product.Id} - {product.Name}");

                var updated = await _productRepository.GetByIdWithDetailsAsync(product.Id, cancellationToken);
                return MapToDetailResponse(updated!);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var conflictInfo = string.Join("; ", ex.Entries.Select(e => $"{e.Entity.GetType().Name}({e.State})"));
                await _logService.WriteMessageAsync($"CONCURRENCY CONFLICT attempt {attempt}: Entries=[{conflictInfo}] Message={ex.Message}");

                await _unitOfWork.RollbackAsync();
                _unitOfWork.ClearChangeTracker();

                // Retry once with completely fresh state.
                if (attempt < 2)
                {
                    await _logService.WriteMessageAsync($"Product update concurrency conflict, retrying once. ProductId: {request.Id}. Details: {ex.Message}");
                    continue;
                }

                throw new ProjectException(ResponseType.Conflict, "Dữ liệu sản phẩm đã thay đổi, vui lòng tải lại và thử lại");
            }
            catch (Exception ex) when (ex is not DbUpdateConcurrencyException)
            {
                await _logService.WriteMessageAsync($"UNEXPECTED ERROR attempt {attempt}: {ex.GetType().Name}: {ex.Message}");
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        throw new ProjectException(ResponseType.Conflict, "Dữ liệu sản phẩm đã thay đổi, vui lòng tải lại và thử lại");
    }

    public async Task<ProductDetailResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdWithDetailsCachedAsync(id, cancellationToken);
        return product == null ? null : MapToDetailResponse(product);
    }

    public async Task<ProductDetailResponse?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ProjectException(ResponseType.InvalidData, "Slug không hợp lệ");
        }

        var product = await _productRepository.GetBySlugCachedAsync(slug, cancellationToken);
        return product == null ? null : MapToDetailResponse(product);
    }

    public async Task<PagedResult<ProductListItemResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page <= 0 || pageSize <= 0)
        {
            throw new ProjectException(ResponseType.InvalidData, "Page và pageSize phải lớn hơn 0");
        }

        var totalCount = await _productRepository.CountActiveAsync(cancellationToken);
        var skip = (page - 1) * pageSize;
        var products = await _productRepository.GetPagedAsync(skip, pageSize, cancellationToken);

        var items = products.Select(p =>
        {
            var price = GetPrimaryPrice(p);
            return new ProductListItemResponse
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Slug = p.Slug,
                BrandName = p.Brand?.Name ?? string.Empty,
                CategoryName = p.Category?.Name ?? string.Empty,
                BasePrice = price.Base,
                SalePrice = price.Sale,
                Currency = price.Currency
            };
        }).ToList();

        return new PagedResult<ProductListItemResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ProductListItemResponse>> SearchAsync(ProductFilterRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Page <= 0 || request.PageSize <= 0)
        {
            throw new ProjectException(ResponseType.InvalidData, "Page và pageSize phải lớn hơn 0");
        }

        var totalCount = await _productRepository.CountFilteredAsync(request.SearchTerm, request.CategoryId, request.BrandId, cancellationToken);
        var skip = (request.Page - 1) * request.PageSize;
        var products = await _productRepository.SearchPagedAsync(request.SearchTerm, request.CategoryId, request.BrandId, skip, request.PageSize, cancellationToken);

        var items = products.Select(p =>
        {
            var price = GetPrimaryPrice(p);
            return new ProductListItemResponse
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Slug = p.Slug,
                BrandName = p.Brand?.Name ?? string.Empty,
                CategoryName = p.Category?.Name ?? string.Empty,
                BasePrice = price.Base,
                SalePrice = price.Sale,
                Currency = price.Currency
            };
        }).ToList();

        return new PagedResult<ProductListItemResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        if (product == null)
        {
            throw new ProjectException(ResponseType.ProductNotFound);
        }

        var slug = product.Slug; // Capture slug before changes
        product.IsDelete = true;
        product.UpdateAt = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.CommitAsync();

            // Invalidate cache after successful commit
            await _productRepository.InvalidateProductCacheAsync(product.Id, slug);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        await _logService.WriteMessageAsync($"Product soft-deleted: {product.Id}");
    }

    private static void ValidateVariants(IReadOnlyCollection<ProductVariantRequest> variants)
    {
        if (variants == null || variants.Count == 0)
        {
            throw new ProjectException(ResponseType.InvalidData, "Cần ít nhất một biến thể");
        }

        var skuSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var variant in variants)
        {
            if (string.IsNullOrWhiteSpace(variant.Sku))
            {
                throw new ProjectException(ResponseType.SkuCannotBeEmpty);
            }

            if (!skuSet.Add(variant.Sku.Trim()))
            {
                throw new ProjectException(ResponseType.Conflict, "SKU bị trùng trong cùng request");
            }

            if (variant.Quantity < 0)
            {
                throw new ProjectException(ResponseType.StockCannotBeNegative);
            }

            if (variant.BasePrice <= 0)
            {
                throw new ProjectException(ResponseType.PriceMustBePositive);
            }

            if (variant.SalePrice.HasValue && variant.SalePrice.Value > variant.BasePrice)
            {
                throw new ProjectException(ResponseType.SalePriceCannotExceedOriginalPrice);
            }
        }
    }

    private async Task EnsureVariantSkuUniqueAcrossProductsAsync(IEnumerable<ProductVariantRequest> variants, Product? currentProduct, CancellationToken cancellationToken)
    {
        foreach (var variant in variants)
        {
            var excludeId = variant.Id;
            var exists = await _productRepository.VariantSkuExistsAsync(variant.Sku, excludeId, cancellationToken);

            if (exists)
            {
                throw new ProjectException(ResponseType.AlreadyExists, $"SKU đã tồn tại: {variant.Sku}");
            }
        }
    }

    private Product MapToEntity(CreateProductRequest request)
    {
        var now = DateTime.UtcNow;
        var productId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Code = request.Code,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Images = MapImages(request.ImageUrls, now),
            ProductTags = new List<ProductTag>(),
            ProductAttributes = MapAttributes(request.Attributes, now),
            CreateAt = now,
            Variants = MapVariants(request.Variants, productId, now)
        };

        return product;
    }

    private static List<ProductImage> MapImages(IEnumerable<string>? imageUrls, DateTime now)
    {
        if (imageUrls == null)
        {
            return new List<ProductImage>();
        }

        var urls = imageUrls
            .Select((url, index) => new { url = url?.Trim(), index })
            .Where(x => !string.IsNullOrWhiteSpace(x.url))
            .ToList();

        return urls.Select(x => new ProductImage
        {
            Id = Guid.NewGuid(),
            Url = x.url!,
            IsPrimary = x.index == 0,
            SortOrder = x.index,
            CreateAt = now
        }).ToList();
    }

    private static List<ProductAttribute> MapAttributes(Dictionary<Guid, string>? attributes, DateTime now)
    {
        if (attributes == null)
        {
            return new List<ProductAttribute>();
        }

        return attributes
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => new ProductAttribute
            {
                Id = Guid.NewGuid(),
                ProductAttributeDefinitionId = kvp.Key,
                Value = kvp.Value,
                CreateAt = now
            }).ToList();
    }

    private static List<ProductVariant> MapVariants(IEnumerable<ProductVariantRequest> variantRequests, Guid productId, DateTime now)
    {
        var variants = new List<ProductVariant>();

        foreach (var request in variantRequests)
        {
            var variantId = request.Id ?? Guid.NewGuid();
            var variant = new ProductVariant
            {
                Id = variantId,
                ProductId = productId,
                Sku = request.Sku,
                Name = request.Name,
                Quantity = request.Quantity,
                SafetyStock = request.SafetyStock,
                CreateAt = now,
                Prices =
                {
                    new ProductVariantPrice
                    {
                        Id = Guid.NewGuid(),
                        ProductVariantId = variantId,
                        BasePrice = request.BasePrice,
                        SalePrice = request.SalePrice,
                        Currency = request.Currency,
                        ValidFrom = now,
                        CreateAt = now
                    }
                },
                VariantAttributes = MapVariantAttributes(request.Attributes, variantId, now)
            };

            variants.Add(variant);
        }

        return variants;
    }

    private static List<ProductVariantAttribute> MapVariantAttributes(Dictionary<Guid, string>? attributes, Guid variantId, DateTime now)
    {
        if (attributes == null)
        {
            return new List<ProductVariantAttribute>();
        }

        return attributes
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp => new ProductVariantAttribute
            {
                Id = Guid.NewGuid(),
                ProductVariantId = variantId,
                ProductAttributeDefinitionId = kvp.Key,
                Value = kvp.Value,
                CreateAt = now
            }).ToList();
    }

    private async Task UpdateImagesAsync(Guid productId, IEnumerable<string>? imageUrls, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var normalizedUrls = (imageUrls ?? Enumerable.Empty<string>())
            .Select((url, index) => new { Url = url?.Trim(), Index = index })
            .Where(x => !string.IsNullOrWhiteSpace(x.Url))
            .Select(x => new { Url = x.Url!, x.Index })
            .ToList();

        // Detach all tracked ProductImage entries for this product to avoid stale Modified state.
        var trackedImageEntries = _unitOfWork.Context.ChangeTracker
            .Entries<ProductImage>()
            .Where(e => e.Entity.ProductId == productId)
            .ToList();

        foreach (var entry in trackedImageEntries)
        {
            entry.State = EntityState.Detached;
        }

        // Replace image set atomically in this DbContext: delete old rows, then insert requested rows.
        await _unitOfWork.Context.ProductImages
            .Where(i => i.ProductId == productId)
            .ExecuteDeleteAsync(cancellationToken);

        if (!normalizedUrls.Any())
        {
            return;
        }

        var newImages = normalizedUrls.Select(item => new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Url = item.Url,
            IsPrimary = item.Index == 0,
            SortOrder = item.Index,
            CreateAt = now,
            IsDelete = false
        }).ToList();

        await _unitOfWork.Context.ProductImages.AddRangeAsync(newImages, cancellationToken);
    }

    private async Task UpdateTagsAsync(Product product, IEnumerable<string>? tags, CancellationToken cancellationToken)
    {
        var normalized = (tags ?? Enumerable.Empty<string>())
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Remove tags that are no longer in the request
        var toRemove = product.ProductTags
            .Where(pt => !normalized.Any(n => string.Equals(n, pt.Tag.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        foreach (var pt in toRemove)
        {
            product.ProductTags.Remove(pt);
        }

        // Determine which tag names are already linked
        var currentTagNames = product.ProductTags
            .Select(pt => pt.Tag.Name.ToLowerInvariant())
            .ToHashSet();

        var newTagNames = normalized.Where(n => !currentTagNames.Contains(n.ToLowerInvariant())).ToList();
        if (!newTagNames.Any())
        {
            return;
        }

        var existingTags = await _productRepository.GetTagsByNamesAsync(newTagNames, cancellationToken);
        var existingNames = existingTags.Select(t => t.Name.ToLowerInvariant()).ToHashSet();

        foreach (var tag in existingTags)
        {
            product.ProductTags.Add(new ProductTag
            {
                Tag = tag,
                Product = product
            });
        }

        var now = DateTime.UtcNow;
        foreach (var tagName in newTagNames)
        {
            if (existingNames.Contains(tagName.ToLowerInvariant()))
            {
                continue;
            }

            var newTag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = tagName,
                CreateAt = now
            };

            product.ProductTags.Add(new ProductTag
            {
                Tag = newTag,
                Product = product
            });
        }
    }

    private void UpdateAttributes(Product product, Dictionary<Guid, string>? attributes)
    {
        if (attributes == null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var existingByDefinition = product.ProductAttributes
            .ToDictionary(x => x.ProductAttributeDefinitionId, x => x);

        foreach (var kvp in attributes)
        {
            if (string.IsNullOrWhiteSpace(kvp.Value))
            {
                continue;
            }

            if (existingByDefinition.TryGetValue(kvp.Key, out var existing))
            {
                if (existing.Value != kvp.Value)
                {
                    existing.Value = kvp.Value;
                    existing.UpdateAt = now;
                }
                continue;
            }

            product.ProductAttributes.Add(new ProductAttribute
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductAttributeDefinitionId = kvp.Key,
                Value = kvp.Value,
                CreateAt = now
            });
        }
    }

    private void UpdateVariants(Product product, List<ProductVariantRequest> variantRequests)
    {
        var now = DateTime.UtcNow;
        var variantById = product.Variants.ToDictionary(v => v.Id, v => v);

        foreach (var variantRequest in variantRequests)
        {
            if (variantRequest.Id.HasValue && variantById.TryGetValue(variantRequest.Id.Value, out var existing))
            {
                var changed = false;
                if (existing.Sku != variantRequest.Sku) { existing.Sku = variantRequest.Sku; changed = true; }
                if (existing.Name != variantRequest.Name) { existing.Name = variantRequest.Name; changed = true; }
                if (existing.Quantity != variantRequest.Quantity) { existing.Quantity = variantRequest.Quantity; changed = true; }
                if (existing.SafetyStock != variantRequest.SafetyStock) { existing.SafetyStock = variantRequest.SafetyStock; changed = true; }
                if (changed) existing.UpdateAt = now;

                UpdateVariantAttributes(existing, variantRequest.Attributes);
                UpdateVariantPrices(existing, variantRequest.BasePrice, variantRequest.SalePrice, variantRequest.Currency);
            }
            else
            {
                var newVariantId = variantRequest.Id ?? Guid.NewGuid();

                var newVariant = new ProductVariant
                {
                    Id = newVariantId,
                    ProductId = product.Id,
                    Sku = variantRequest.Sku,
                    Name = variantRequest.Name,
                    Quantity = variantRequest.Quantity,
                    SafetyStock = variantRequest.SafetyStock,
                    CreateAt = now,
                    VariantAttributes = MapVariantAttributes(variantRequest.Attributes, newVariantId, now)
                };

                newVariant.Prices.Add(new ProductVariantPrice
                {
                    Id = Guid.NewGuid(),
                    ProductVariantId = newVariant.Id,
                    BasePrice = variantRequest.BasePrice,
                    SalePrice = variantRequest.SalePrice,
                    Currency = variantRequest.Currency,
                    ValidFrom = now,
                    CreateAt = now
                });

                product.Variants.Add(newVariant);
            }
        }
    }

    private void UpdateVariantAttributes(ProductVariant variant, Dictionary<Guid, string>? attributes)
    {
        if (attributes == null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var existingByDefinition = variant.VariantAttributes
            .ToDictionary(x => x.ProductAttributeDefinitionId, x => x);

        foreach (var kvp in attributes)
        {
            if (string.IsNullOrWhiteSpace(kvp.Value))
            {
                continue;
            }

            if (existingByDefinition.TryGetValue(kvp.Key, out var existing))
            {
                if (existing.Value != kvp.Value)
                {
                    existing.Value = kvp.Value;
                    existing.UpdateAt = now;
                }
                continue;
            }

            variant.VariantAttributes.Add(new ProductVariantAttribute
            {
                Id = Guid.NewGuid(),
                ProductVariantId = variant.Id,
                ProductAttributeDefinitionId = kvp.Key,
                Value = kvp.Value,
                CreateAt = now
            });
        }
    }

    private void UpdateVariantPrices(ProductVariant variant, decimal basePrice, decimal? salePrice, string currency)
    {
        var now = DateTime.UtcNow;
        var latest = variant.Prices.OrderByDescending(p => p.ValidFrom).FirstOrDefault();

        if (latest != null && latest.BasePrice == basePrice && latest.SalePrice == salePrice && latest.Currency == currency)
        {
            return;
        }

        if (latest != null && !latest.ValidTo.HasValue)
        {
            latest.ValidTo = now;
            latest.UpdateAt = now;
        }

        variant.Prices.Add(new ProductVariantPrice
        {
            Id = Guid.NewGuid(),
            ProductVariantId = variant.Id,
            BasePrice = basePrice,
            SalePrice = salePrice,
            Currency = currency,
            ValidFrom = now,
            CreateAt = now
        });
    }

    private static ProductDetailResponse MapToDetailResponse(Product product)
    {
        return new ProductDetailResponse
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name ?? string.Empty,
            ImageUrls = product.Images.OrderBy(i => i.SortOrder).Select(i => i.Url).ToList(),
            Tags = product.ProductTags.Select(pt => pt.Tag.Name).ToList(),
            Attributes = product.ProductAttributes.Select(pa => new ProductAttributeResponse
            {
                AttributeDefinitionId = pa.ProductAttributeDefinitionId,
                Name = pa.AttributeDefinition?.Name ?? string.Empty,
                Value = pa.Value
            }).ToList(),
            Variants = product.Variants.Select(v =>
            {
                var latestPrice = v.Prices.OrderByDescending(p => p.ValidFrom).FirstOrDefault();
                return new ProductVariantResponse
                {
                    Id = v.Id,
                    Sku = v.Sku,
                    Name = v.Name,
                    Quantity = v.Quantity,
                    SafetyStock = v.SafetyStock,
                    BasePrice = latestPrice?.BasePrice ?? 0,
                    SalePrice = latestPrice?.SalePrice,
                    Currency = latestPrice?.Currency ?? "USD",
                    Attributes = v.VariantAttributes.Select(va => new ProductAttributeResponse
                    {
                        AttributeDefinitionId = va.ProductAttributeDefinitionId,
                        Name = va.AttributeDefinition?.Name ?? string.Empty,
                        Value = va.Value
                    }).ToList()
                };
            }).ToList()
        };
    }

    private static (decimal Base, decimal? Sale, string Currency) GetPrimaryPrice(Product product)
    {
        var latestPrices = product.Variants
            .Select(v => v.Prices.OrderByDescending(p => p.ValidFrom).FirstOrDefault())
            .Where(p => p != null)
            .Cast<ProductVariantPrice>()
            .ToList();

        if (!latestPrices.Any())
        {
            return (0, null, "USD");
        }

        var lowest = latestPrices.OrderBy(p => p.SalePrice ?? p.BasePrice).First();
        return (lowest.BasePrice, lowest.SalePrice, lowest.Currency);
    }
}
