namespace SHNGearBE.Models.DTOs.Cart;

/// <summary>
/// Represents a shopping cart stored in Redis
/// </summary>
public class CartDto
{
    public Guid AccountId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartItemDto
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = null!;
    public string VariantName { get; set; } = null!;
    public string Sku { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }
    public int AvailableStock { get; set; }
}

/// <summary>
/// Internal model stored in Redis (minimal data, enriched on read)
/// </summary>
public class CartEntry
{
    public List<CartItemEntry> Items { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class CartItemEntry
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
}

public class AddToCartRequest
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}
