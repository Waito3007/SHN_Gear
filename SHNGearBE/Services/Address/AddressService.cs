using BackgroundLogService.Abstractions;
using SHNGearBE.Models.DTOs.Address;
using SHNGearBE.Models.Exceptions;
using SHNGearBE.Repositorys.Interface.Address;
using SHNGearBE.Services.Interfaces.Address;
using SHNGearBE.UnitOfWork;
using AddressEntity = SHNGearBE.Models.Entities.Account.Address;

namespace SHNGearBE.Services.Address;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogService<AddressService> _logService;
    private const int MaxAddressesPerAccount = 10;

    public AddressService(IAddressRepository addressRepository, IUnitOfWork unitOfWork, ILogService<AddressService> logService)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
        _logService = logService;
    }

    public async Task<IReadOnlyList<AddressDto>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        var addresses = await _addressRepository.GetByAccountIdAsync(accountId, cancellationToken);
        return addresses.Select(MapToDto).ToList();
    }

    public async Task<AddressDto?> GetByIdAsync(Guid id, Guid accountId, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAndAccountAsync(id, accountId, cancellationToken);
        return address == null ? null : MapToDto(address);
    }

    public async Task<AddressDto> CreateAsync(Guid accountId, CreateAddressRequest request, CancellationToken cancellationToken = default)
    {
        var count = await _addressRepository.CountByAccountIdAsync(accountId, cancellationToken);
        if (count >= MaxAddressesPerAccount)
            throw new ProjectException(ResponseType.BadRequest, $"Tối đa {MaxAddressesPerAccount} địa chỉ");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // If this is the first address or marked as default, clear existing defaults
            if (request.IsDefault || count == 0)
            {
                await _addressRepository.ClearDefaultAsync(accountId, cancellationToken);
            }

            var address = new AddressEntity
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                RecipientName = request.RecipientName.Trim(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Province = request.Province.Trim(),
                District = request.District.Trim(),
                Ward = request.Ward.Trim(),
                Street = request.Street.Trim(),
                Note = request.Note?.Trim(),
                IsDefault = request.IsDefault || count == 0 // First address is always default
            };

            await _addressRepository.AddAsync(address);
            await _unitOfWork.CommitAsync();

            await _logService.WriteMessageAsync($"Address created: {address.Id} for account {accountId}");
            return MapToDto(address);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<AddressDto> UpdateAsync(Guid id, Guid accountId, UpdateAddressRequest request, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAndAccountAsync(id, accountId, cancellationToken);
        if (address == null)
            throw new ProjectException(ResponseType.NotFound, "Địa chỉ không tồn tại");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (request.IsDefault && !address.IsDefault)
            {
                await _addressRepository.ClearDefaultAsync(accountId, cancellationToken);
            }

            address.RecipientName = request.RecipientName.Trim();
            address.PhoneNumber = request.PhoneNumber.Trim();
            address.Province = request.Province.Trim();
            address.District = request.District.Trim();
            address.Ward = request.Ward.Trim();
            address.Street = request.Street.Trim();
            address.Note = request.Note?.Trim();
            address.IsDefault = request.IsDefault;

            await _addressRepository.UpdateAsync(address);
            await _unitOfWork.CommitAsync();

            await _logService.WriteMessageAsync($"Address updated: {id} for account {accountId}");
            return MapToDto(address);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, Guid accountId, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAndAccountAsync(id, accountId, cancellationToken);
        if (address == null)
            throw new ProjectException(ResponseType.NotFound, "Địa chỉ không tồn tại");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var wasDefault = address.IsDefault;
            await _addressRepository.DeleteAsync(id);

            // If deleted address was default, assign default to the most recent remaining
            if (wasDefault)
            {
                var remaining = await _addressRepository.GetByAccountIdAsync(accountId, cancellationToken);
                var next = remaining.FirstOrDefault(a => a.Id != id);
                if (next != null)
                {
                    next.IsDefault = true;
                    next.UpdateAt = DateTime.UtcNow;
                }
            }

            await _unitOfWork.CommitAsync();
            await _logService.WriteMessageAsync($"Address deleted: {id} for account {accountId}");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<AddressDto> SetDefaultAsync(Guid id, Guid accountId, CancellationToken cancellationToken = default)
    {
        var address = await _addressRepository.GetByIdAndAccountAsync(id, accountId, cancellationToken);
        if (address == null)
            throw new ProjectException(ResponseType.NotFound, "Địa chỉ không tồn tại");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _addressRepository.ClearDefaultAsync(accountId, cancellationToken);
            address.IsDefault = true;
            address.UpdateAt = DateTime.UtcNow;

            await _unitOfWork.CommitAsync();
            await _logService.WriteMessageAsync($"Address set as default: {id} for account {accountId}");
            return MapToDto(address);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private static AddressDto MapToDto(AddressEntity entity) => new()
    {
        Id = entity.Id,
        RecipientName = entity.RecipientName,
        PhoneNumber = entity.PhoneNumber,
        Province = entity.Province,
        District = entity.District,
        Ward = entity.Ward,
        Street = entity.Street,
        Note = entity.Note,
        IsDefault = entity.IsDefault
    };
}
