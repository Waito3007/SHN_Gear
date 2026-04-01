using SHNGearBE.Models.Enums;

namespace SHNGearBE.Infrastructure.Payment;

public class PaymentContext
{
    public Guid AccountId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentToken { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public PaymentStatus PaymentStatus { get; set; }

    public static PaymentResult Succeeded(string? transactionId = null)
    {
        return new PaymentResult
        {
            Success = true,
            TransactionId = transactionId,
            PaymentStatus = PaymentStatus.Completed
        };
    }

    public static PaymentResult Failed(string errorMessage)
    {
        return new PaymentResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            PaymentStatus = PaymentStatus.Failed
        };
    }
}
