using SHNGearBE.Models.DTOs.Cart;

namespace SHNGearBE.Services.Interfaces.Cart;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<CartDto> AddItemAsync(Guid accountId, AddToCartRequest request, CancellationToken cancellationToken = default);
    Task<CartDto> UpdateItemQuantityAsync(Guid accountId, Guid productVariantId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);
    Task<CartDto> RemoveItemAsync(Guid accountId, Guid productVariantId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(Guid accountId);
}
