using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class DeliveryScheduleConfiguration : IEntityTypeConfiguration<DeliverySchedule>
    {
        public void Configure(EntityTypeBuilder<DeliverySchedule> builder)
        {
            builder.ToTable("delivery_schedules");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.DeliverySettingsId)
                .HasColumnName("delivery_settings_id");

            builder.HasIndex(x => new { x.DeliverySettingsId, x.Date })
                .IsUnique()
                .HasDatabaseName("idx_delivery_schedule_settings_date");
            
            builder.Property(x => x.Date)
                .HasColumnName("date")
                .IsRequired()
                .HasColumnType("DATE")
                .HasConversion(
                    date => date.ToDateTime(TimeOnly.MinValue),
                    dateTime => DateOnly.FromDateTime(dateTime)
                );
                
            builder.Property(x => x.MorningEnabled)
                .HasColumnName("morning_enabled")
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(x => x.AfternoonEnabled)
                .HasColumnName("afternoon_enabled")
                .IsRequired()
                .HasDefaultValue(true);
                
            // Capacidades personalizadas
            builder.Property(x => x.CustomMaxCapacityMorning)
                .HasColumnName("custom_max_capacity_morning");
                
            builder.Property(x => x.CustomMaxCapacityAfternoon)
                .HasColumnName("custom_max_capacity_afternoon");
                
            // Horarios personalizados
            builder.Property(x => x.CustomMorningStartTime)
                .HasColumnName("custom_morning_start_time");
                
            builder.Property(x => x.CustomMorningEndTime)
                .HasColumnName("custom_morning_end_time");
                
            builder.Property(x => x.CustomAfternoonStartTime)
                .HasColumnName("custom_afternoon_start_time");
                
            builder.Property(x => x.CustomAfternoonEndTime)
                .HasColumnName("custom_afternoon_end_time");
                
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            builder.HasOne(x => x.DeliverySettings)
                .WithMany(ds => ds.Schedules)
                .HasForeignKey(x => x.DeliverySettingsId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(x => x.Slots)
                .WithOne(s => s.DeliverySchedule)
                .HasForeignKey(s => s.DeliveryScheduleId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}