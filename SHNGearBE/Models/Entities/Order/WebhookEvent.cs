using SHNGearBE.Models.Entities;

namespace SHNGearBE.Models.Entities.Order;

public class WebhookEvent : BaseEntity
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = null!;
    public string EventId { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public string? CaptureId { get; set; }
    public DateTime ProcessedAt { get; set; }
    public Guid? OrderId { get; set; }

    public virtual Order? Order { get; set; }
}
