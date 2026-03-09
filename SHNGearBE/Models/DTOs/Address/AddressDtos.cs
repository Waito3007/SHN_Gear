namespace SHNGearBE.Models.DTOs.Address;

public class AddressDto
{
    public Guid Id { get; set; }
    public string RecipientName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string District { get; set; } = null!;
    public string Ward { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string? Note { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateAddressRequest
{
    public string RecipientName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string District { get; set; } = null!;
    public string Ward { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string? Note { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateAddressRequest
{
    public string RecipientName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string District { get; set; } = null!;
    public string Ward { get; set; } = null!;
    public string Street { get; set; } = null!;
    public string? Note { get; set; }
    public bool IsDefault { get; set; }
}
