using BackgroundLogService.Abstractions;
using Microsoft.EntityFrameworkCore;
using SHNGearBE.Data;
using SHNGearBE.Models.DTOs.Product;
using SHNGearBE.Models.Entities.Product;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Product;
using SHNGearBE.Services.Interfaces.Product;
using SHNGearBE.UnitOfWork;

namespace SHNGearBE.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogService<CategoryService> _logService;

    public CategoryService(
        ICategoryRepository categoryRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogService<CategoryService> logService)
    {
        _categoryRepository = categoryRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _logService = logService;
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
            return null;

        return MapToCategoryDto(category);
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .Where(x => !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
        return categories.Select(MapToCategoryDto).ToList();
    }

    public async Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync()
    {
        var categories = await _context.Categories
            .Where(x => !x.IsDelete)
            .Include(x => x.Children)
            .AsNoTracking()
            .ToListAsync();

        // Build tree structure (only root categories)
        var rootCategories = categories
            .Where(x => x.ParentCategoryId == null)
            .Select(x => MapToCategoryTreeDto(x, categories))
            .ToList();

        return rootCategories;
    }

    public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await _categoryRepository.GetActiveAsync();
        return categories.Select(MapToCategoryDto).ToList();
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug))
        {
            throw new ProjectException(ResponseType.BadRequest, "Category name and slug are required");
        }

        // Check if slug already exists
        var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(x => x.Slug == request.Slug && !x.IsDelete);
        if (existingCategory != null)
        {
            throw new ProjectException(ResponseType.AlreadyExists, "Category slug already exists");
        }

        // If parent category is specified, verify it exists
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value);
            if (parentCategory == null)
            {
                throw new ProjectException(ResponseType.NotFound, "Parent category not found");
            }
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = request.Slug.Trim(),
            ParentCategoryId = request.ParentCategoryId,
            CreateAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            return MapToCategoryDto(category);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            await _logService.WriteExceptionAsync(ex);
            throw new ProjectException(ResponseType.InternalServerError, "Failed to create category");
        }
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Category not found");
        }

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Slug))
        {
            throw new ProjectException(ResponseType.BadRequest, "Category name and slug are required");
        }

        // Check if slug is being changed and if new slug already exists
        if (category.Slug != request.Slug)
        {
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(x => x.Slug == request.Slug && x.Id != id && !x.IsDelete);
            if (existingCategory != null)
            {
                throw new ProjectException(ResponseType.AlreadyExists, "Category slug already exists");
            }
        }

        // If parent category is being changed, verify it exists
        if (category.ParentCategoryId != request.ParentCategoryId && request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value);
            if (parentCategory == null)
            {
                throw new ProjectException(ResponseType.NotFound, "Parent category not found");
            }
        }

        category.Name = request.Name.Trim();
        category.Slug = request.Slug.Trim();
        category.ParentCategoryId = request.ParentCategoryId;
        category.UpdateAt = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _categoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            return MapToCategoryDto(category);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            await _logService.WriteExceptionAsync(ex);
            throw new ProjectException(ResponseType.InternalServerError, "Failed to update category");
        }
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            return false;
        }

        // Check if category has child categories
        var hasChildren = await _context.Categories
            .AnyAsync(x => x.ParentCategoryId == id && !x.IsDelete);
        if (hasChildren)
        {
            throw new ProjectException(ResponseType.BadRequest, "Cannot delete category that has subcategories");
        }

        // Soft delete
        category.IsDelete = true;
        category.UpdateAt = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _categoryRepository.UpdateAsync(category);
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            await _logService.WriteExceptionAsync(ex);
            throw new ProjectException(ResponseType.InternalServerError, "Failed to delete category");
        }
    }

    private static CategoryDto MapToCategoryDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = !category.IsDelete
        };
    }

    private static CategoryTreeDto MapToCategoryTreeDto(Category category, List<Category> allCategories)
    {
        var children = allCategories
            .Where(x => x.ParentCategoryId == category.Id)
            .Select(x => MapToCategoryTreeDto(x, allCategories))
            .ToList();

        return new CategoryTreeDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            ParentCategoryId = category.ParentCategoryId,
            Children = children,
            IsActive = !category.IsDelete
        };
    }
}
