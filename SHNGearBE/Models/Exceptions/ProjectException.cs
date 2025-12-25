using SHNGearBE.Extensions;

namespace SHNGearBE.Models.Exceptions;

public class ProjectException : Exception
{
    public ResponseType ResponseType { get; }
    public bool Success { get; } = false;

    public ProjectException(ResponseType responseType)
        : base(responseType.GetDescription())
    {
        ResponseType = responseType;
    }

    public ProjectException(ResponseType responseType, string additionalMessage)
        : base($"{responseType.GetDescription()} - {additionalMessage}")
    {
        ResponseType = responseType;
    }

    public ProductResponse ToResponse()
    {
        return new ProductResponse
        {
            Success = Success,
            ErrorCode = ResponseType,
            ErrorMessage = Message
        };
    }
}

public class ProductResponse
{
    public bool Success { get; set; } = true;
    public ResponseType? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Data { get; set; }
}