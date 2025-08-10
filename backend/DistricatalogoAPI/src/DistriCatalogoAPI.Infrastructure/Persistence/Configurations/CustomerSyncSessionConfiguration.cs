using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class CustomerSyncSessionConfiguration : IEntityTypeConfiguration<CustomerSyncSession>
    {
        public void Configure(EntityTypeBuilder<CustomerSyncSession> builder)
        {
            builder.ToTable("customer_sync_sessions");
            
            builder.HasKey(s => s.Id);
            
            builder.Property(s => s.Id)
                .HasColumnName("id")
                .HasMaxLength(36);
                
            builder.Property(s => s.EmpresaId)
                .HasColumnName("empresa_id")
                .IsRequired();
                
            builder.Property(s => s.Source)
                .HasColumnName("source")
                .HasMaxLength(50);
                
            builder.Property(s => s.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .HasDefaultValue("active");
                
            builder.Property(s => s.TotalProcessed)
                .HasColumnName("total_processed")
                .HasDefaultValue(0);
                
            builder.Property(s => s.TotalCreated)
                .HasColumnName("total_created")
                .HasDefaultValue(0);
                
            builder.Property(s => s.TotalUpdated)
                .HasColumnName("total_updated")
                .HasDefaultValue(0);
                
            builder.Property(s => s.TotalUnchanged)
                .HasColumnName("total_unchanged")
                .HasDefaultValue(0);
                
            builder.Property(s => s.TotalErrors)
                .HasColumnName("total_errors")
                .HasDefaultValue(0);
                
            builder.Property(s => s.StartedAt)
                .HasColumnName("started_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(s => s.CompletedAt)
                .HasColumnName("completed_at");
                
            builder.Property(s => s.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(100);
                
            // Índices
            builder.HasIndex(s => new { s.EmpresaId, s.Status })
                .HasDatabaseName("idx_empresa_status");
                
            builder.HasIndex(s => s.StartedAt)
                .HasDatabaseName("idx_started_at");
                
            // Relaciones - Las FKs se configuran en el esquema SQL
            // builder.HasOne... se configurará cuando las entidades de navegación estén disponibles
        }
    }
}