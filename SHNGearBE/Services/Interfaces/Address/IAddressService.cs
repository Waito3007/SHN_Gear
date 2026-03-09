using SHNGearBE.Models.DTOs.Address;

namespace SHNGearBE.Services.Interfaces.Address;

public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<AddressDto?> GetByIdAsync(Guid id, Guid accountId, CancellationToken cancellationToken = default);
    Task<AddressDto> CreateAsync(Guid accountId, CreateAddressRequest request, CancellationToken cancellationToken = default);
    Task<AddressDto> UpdateAsync(Guid id, Guid accountId, UpdateAddressRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, Guid accountId, CancellationToken cancellationToken = default);
    Task<AddressDto> SetDefaultAsync(Guid id, Guid accountId, CancellationToken cancellationToken = default);
}
