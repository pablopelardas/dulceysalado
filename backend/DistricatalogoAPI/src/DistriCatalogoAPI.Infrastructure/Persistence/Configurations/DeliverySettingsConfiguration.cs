using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class DeliverySettingsConfiguration : IEntityTypeConfiguration<DeliverySettings>
    {
        public void Configure(EntityTypeBuilder<DeliverySettings> builder)
        {
            builder.ToTable("delivery_settings");
            
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.EmpresaId)
                .HasColumnName("empresa_id");

            builder.HasIndex(x => x.EmpresaId)
                .IsUnique()
                .HasDatabaseName("idx_delivery_settings_empresa");
            
            builder.Property(x => x.MinSlotsAhead)
                .HasColumnName("min_slots_ahead")
                .IsRequired()
                .HasDefaultValue(2);
                
            builder.Property(x => x.MaxCapacityMorning)
                .HasColumnName("max_capacity_morning")
                .IsRequired()
                .HasDefaultValue(10);
                
            builder.Property(x => x.MaxCapacityAfternoon)
                .HasColumnName("max_capacity_afternoon")
                .IsRequired()
                .HasDefaultValue(10);
                
            builder.Property(x => x.MondayEnabled)
                .HasColumnName("monday_enabled")
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(x => x.TuesdayEnabled)
                .HasColumnName("tuesday_enabled")
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(x => x.WednesdayEnabled)
                .HasColumnName("wednesday_enabled")
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(x => x.ThursdayEnabled)
                .HasColumnName("thursday_enabled")
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(x => x.FridayEnabled)
                .HasColumnName("friday_enabled")
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(x => x.SaturdayEnabled)
                .HasColumnName("saturday_enabled")
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(x => x.SundayEnabled)
                .HasColumnName("sunday_enabled")
                .IsRequired()
                .HasDefaultValue(false);
                
            // Horarios por día - Lunes
            builder.Property(x => x.MondayMorningStart).HasColumnName("monday_morning_start");
            builder.Property(x => x.MondayMorningEnd).HasColumnName("monday_morning_end");
            builder.Property(x => x.MondayAfternoonStart).HasColumnName("monday_afternoon_start");
            builder.Property(x => x.MondayAfternoonEnd).HasColumnName("monday_afternoon_end");
            
            // Horarios por día - Martes
            builder.Property(x => x.TuesdayMorningStart).HasColumnName("tuesday_morning_start");
            builder.Property(x => x.TuesdayMorningEnd).HasColumnName("tuesday_morning_end");
            builder.Property(x => x.TuesdayAfternoonStart).HasColumnName("tuesday_afternoon_start");
            builder.Property(x => x.TuesdayAfternoonEnd).HasColumnName("tuesday_afternoon_end");
            
            // Horarios por día - Miércoles
            builder.Property(x => x.WednesdayMorningStart).HasColumnName("wednesday_morning_start");
            builder.Property(x => x.WednesdayMorningEnd).HasColumnName("wednesday_morning_end");
            builder.Property(x => x.WednesdayAfternoonStart).HasColumnName("wednesday_afternoon_start");
            builder.Property(x => x.WednesdayAfternoonEnd).HasColumnName("wednesday_afternoon_end");
            
            // Horarios por día - Jueves
            builder.Property(x => x.ThursdayMorningStart).HasColumnName("thursday_morning_start");
            builder.Property(x => x.ThursdayMorningEnd).HasColumnName("thursday_morning_end");
            builder.Property(x => x.ThursdayAfternoonStart).HasColumnName("thursday_afternoon_start");
            builder.Property(x => x.ThursdayAfternoonEnd).HasColumnName("thursday_afternoon_end");
            
            // Horarios por día - Viernes
            builder.Property(x => x.FridayMorningStart).HasColumnName("friday_morning_start");
            builder.Property(x => x.FridayMorningEnd).HasColumnName("friday_morning_end");
            builder.Property(x => x.FridayAfternoonStart).HasColumnName("friday_afternoon_start");
            builder.Property(x => x.FridayAfternoonEnd).HasColumnName("friday_afternoon_end");
            
            // Horarios por día - Sábado
            builder.Property(x => x.SaturdayMorningStart).HasColumnName("saturday_morning_start");
            builder.Property(x => x.SaturdayMorningEnd).HasColumnName("saturday_morning_end");
            builder.Property(x => x.SaturdayAfternoonStart).HasColumnName("saturday_afternoon_start");
            builder.Property(x => x.SaturdayAfternoonEnd).HasColumnName("saturday_afternoon_end");
            
            // Horarios por día - Domingo
            builder.Property(x => x.SundayMorningStart).HasColumnName("sunday_morning_start");
            builder.Property(x => x.SundayMorningEnd).HasColumnName("sunday_morning_end");
            builder.Property(x => x.SundayAfternoonStart).HasColumnName("sunday_afternoon_start");
            builder.Property(x => x.SundayAfternoonEnd).HasColumnName("sunday_afternoon_end");
                
            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            builder.HasOne(x => x.Empresa)
                .WithMany()
                .HasForeignKey(x => x.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(x => x.Schedules)
                .WithOne(s => s.DeliverySettings)
                .HasForeignKey(s => s.DeliverySettingsId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(x => x.Slots)
                .WithOne(s => s.DeliverySettings)
                .HasForeignKey(s => s.DeliverySettingsId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}