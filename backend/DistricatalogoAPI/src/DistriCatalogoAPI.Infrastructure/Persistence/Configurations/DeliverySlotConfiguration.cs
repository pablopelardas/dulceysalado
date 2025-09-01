using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class DeliverySlotConfiguration : IEntityTypeConfiguration<DeliverySlot>
    {
        public void Configure(EntityTypeBuilder<DeliverySlot> builder)
        {
            builder.ToTable("delivery_slots");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.DeliverySettingsId)
                .HasColumnName("delivery_settings_id");

            builder.Property(x => x.DeliveryScheduleId)
                .HasColumnName("delivery_schedule_id");

            builder.HasIndex(x => new { x.DeliverySettingsId, x.Date, x.SlotType })
                .IsUnique()
                .HasDatabaseName("idx_delivery_slot_unique");
                
            builder.HasIndex(x => new { x.Date, x.SlotType })
                .HasDatabaseName("idx_delivery_slot_date_type");
            
            builder.Property(x => x.Date)
                .HasColumnName("date")
                .IsRequired()
                .HasColumnType("DATE")
                .HasConversion(
                    date => date.ToDateTime(TimeOnly.MinValue),
                    dateTime => DateOnly.FromDateTime(dateTime)
                );
                
            builder.Property(x => x.SlotType)
                .HasColumnName("slot_type")
                .IsRequired()
                .HasConversion<int>();
                
            builder.Property(x => x.CurrentCapacity)
                .HasColumnName("current_capacity")
                .IsRequired()
                .HasDefaultValue(0);
                
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            builder.HasOne(x => x.DeliverySettings)
                .WithMany(ds => ds.Slots)
                .HasForeignKey(x => x.DeliverySettingsId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(x => x.DeliverySchedule)
                .WithMany(ds => ds.Slots)
                .HasForeignKey(x => x.DeliveryScheduleId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}