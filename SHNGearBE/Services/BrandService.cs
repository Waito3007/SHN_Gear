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

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogService<BrandService> _logService;

    public BrandService(
        IBrandRepository brandRepository,
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogService<BrandService> logService)
    {
        _brandRepository = brandRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _logService = logService;
    }

    public async Task<BrandDto?> GetBrandByIdAsync(Guid id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
            return null;

        return MapToBrandDto(brand);
    }

    public async Task<IEnumerable<BrandDto>> GetAllBrandsAsync()
    {
        var brands = await _context.Brands
            .Where(x => !x.IsDelete)
            .AsNoTracking()
            .ToListAsync();
        return brands.Select(MapToBrandDto).ToList();
    }

    public async Task<IEnumerable<BrandDto>> GetActiveBrandsAsync()
    {
        var brands = await _brandRepository.GetActiveAsync();
        return brands.Select(MapToBrandDto).ToList();
    }

    public async Task<BrandDto> CreateBrandAsync(CreateBrandRequest request)
    {
        // Validate name is not empty
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ProjectException(ResponseType.BadRequest, "Brand name is required");
        }

        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreateAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _brandRepository.AddAsync(brand);
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            return MapToBrandDto(brand);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            await _logService.WriteExceptionAsync(ex);
            throw new ProjectException(ResponseType.InternalServerError, "Failed to create brand");
        }
    }

    public async Task<BrandDto> UpdateBrandAsync(Guid id, UpdateBrandRequest request)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
        {
            throw new ProjectException(ResponseType.NotFound, "Brand not found");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ProjectException(ResponseType.BadRequest, "Brand name is required");
        }

        brand.Name = request.Name.Trim();
        brand.Description = request.Description?.Trim();
        brand.UpdateAt = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _brandRepository.UpdateAsync(brand);
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            return MapToBrandDto(brand);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            await _logService.WriteExceptionAsync(ex);
            throw new ProjectException(ResponseType.InternalServerError, "Failed to update brand");
        }
    }

    public async Task<bool> DeleteBrandAsync(Guid id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
        {
            return false;
        }

        // Soft delete
        brand.IsDelete = true;
        brand.UpdateAt = DateTime.UtcNow;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _brandRepository.UpdateAsync(brand);
            await _unitOfWork.SaveAsync();
            await _unitOfWork.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            await _logService.WriteExceptionAsync(ex);
            throw new ProjectException(ResponseType.InternalServerError, "Failed to delete brand");
        }
    }

    private static BrandDto MapToBrandDto(Brand brand)
    {
        return new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            IsActive = !brand.IsDelete
        };
    }
}
