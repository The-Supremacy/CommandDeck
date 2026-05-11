using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommandDeck.Persistence.DomainEvents;

public sealed class StoredDomainEventConfiguration : IEntityTypeConfiguration<StoredDomainEvent>
{
    public void Configure(EntityTypeBuilder<StoredDomainEvent> builder)
    {
        builder.ToTable("domain_events", "platform");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.AggregateType).HasMaxLength(256).IsRequired();
        builder.Property(x => x.AggregateId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(256).IsRequired();
        builder.Property(x => x.EventVersion).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Metadata).HasColumnType("jsonb");
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => new { x.AggregateType, x.AggregateId });
    }
}
