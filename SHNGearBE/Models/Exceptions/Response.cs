using SHNGearBE.Extensions;

namespace SHNGearBE.Models.Exceptions;

public class ApiResponse
{
    public bool Success { get; set; } = true;
    public int Code { get; set; } = (int)ResponseType.Success;
    public string Message { get; set; } = ResponseType.Success.GetDescription();
    public object? Data { get; set; }
    public object? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponse(object data)
    {
        Success = true;
        Code = (int)ResponseType.Success;
        Message = ResponseType.Success.GetDescription();
        Data = data;
    }

    public ApiResponse(ResponseType responseType)
    {
        Success = (int)responseType < 1000;
        Code = (int)responseType;
        Message = responseType.GetDescription();
    }

    public ApiResponse(object data, ResponseType responseType)
    {
        Success = (int)responseType < 1000;
        Code = (int)responseType;
        Message = responseType.GetDescription();
        Data = data;
    }

    public ApiResponse(ResponseType responseType, object errors)
    {
        Success = false;
        Code = (int)responseType;
        Message = responseType.GetDescription();
        Errors = errors;
    }
}