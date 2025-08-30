using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class UserNotificationPreferencesConfiguration : IEntityTypeConfiguration<UserNotificationPreferences>
    {
        public void Configure(EntityTypeBuilder<UserNotificationPreferences> builder)
        {
            builder.ToTable("user_notification_preferences");
            
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Id)
                .HasColumnName("id");
                
            builder.Property(p => p.UserId)
                .HasColumnName("user_id")
                .IsRequired();
                
            builder.Property(p => p.NotificacionNuevosPedidos)
                .HasColumnName("notificacion_nuevos_pedidos")
                .HasDefaultValue(true);
                
            builder.Property(p => p.NotificacionCorreccionesAprobadas)
                .HasColumnName("notificacion_correcciones_aprobadas")
                .HasDefaultValue(true);
                
            builder.Property(p => p.NotificacionCorreccionesRechazadas)
                .HasColumnName("notificacion_correcciones_rechazadas")
                .HasDefaultValue(true);
                
            builder.Property(p => p.NotificacionPedidosCancelados)
                .HasColumnName("notificacion_pedidos_cancelados")
                .HasDefaultValue(true);
                
            // Campos de auditoría
            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(p => p.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(255)
                .HasDefaultValue("SYSTEM");
                
            builder.Property(p => p.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(255)
                .HasDefaultValue("SYSTEM");
                
            // Índices
            builder.HasIndex(p => p.UserId)
                .HasDatabaseName("unique_user_preferences")
                .IsUnique();
                
            builder.HasIndex(p => p.UserId)
                .HasDatabaseName("idx_user_notification_prefs_user_id");
                
            // Relaciones
            builder.HasOne<Infrastructure.Models.Usuario>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}