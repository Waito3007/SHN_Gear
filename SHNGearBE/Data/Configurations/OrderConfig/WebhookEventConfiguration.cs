using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SHNGearBE.Models.Entities.Order;

namespace SHNGearBE.Data.Configurations.OrderConfig;

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.ToTable("WebhookEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.EventId)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.CaptureId)
            .HasMaxLength(120);

        builder.HasIndex(x => new { x.Provider, x.EventId })
            .IsUnique();

        builder.HasOne(x => x.Order)
            .WithMany(o => o.WebhookEvents)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
