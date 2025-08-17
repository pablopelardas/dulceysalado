using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
    {
        public void Configure(EntityTypeBuilder<Pedido> builder)
        {
            builder.ToTable("pedidos");
            
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Id)
                .HasColumnName("id");
                
            builder.Property(p => p.ClienteId)
                .HasColumnName("cliente_id")
                .IsRequired();
                
            builder.Property(p => p.EmpresaId)
                .HasColumnName("empresa_id")
                .IsRequired();
                
            builder.Property(p => p.Numero)
                .HasColumnName("numero")
                .HasMaxLength(50)
                .IsRequired();
                
            builder.Property(p => p.FechaPedido)
                .HasColumnName("fecha_pedido")
                .IsRequired();
                
            builder.Property(p => p.FechaEntrega)
                .HasColumnName("fecha_entrega");
                
            builder.Property(p => p.HorarioEntrega)
                .HasColumnName("horario_entrega")
                .HasMaxLength(100);
                
            builder.Property(p => p.DireccionEntrega)
                .HasColumnName("direccion_entrega")
                .HasMaxLength(500);
                
            builder.Property(p => p.Observaciones)
                .HasColumnName("observaciones")
                .HasMaxLength(1000);
                
            builder.Property(p => p.MontoTotal)
                .HasColumnName("monto_total")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
                
            builder.Property(p => p.Estado)
                .HasColumnName("estado")
                .HasConversion<int>()
                .IsRequired();
                
            builder.Property(p => p.MotivoRechazo)
                .HasColumnName("motivo_rechazo")
                .HasMaxLength(500);
                
            builder.Property(p => p.UsuarioGestionId)
                .HasColumnName("usuario_gestion_id");
                
            builder.Property(p => p.FechaGestion)
                .HasColumnName("fecha_gestion");
                
            // Auditoría
            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(p => p.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(100);
                
            builder.Property(p => p.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(100);
                
            // Índices
            builder.HasIndex(p => p.Numero)
                .HasDatabaseName("idx_pedido_numero")
                .IsUnique();
                
            builder.HasIndex(p => new { p.ClienteId, p.EmpresaId })
                .HasDatabaseName("idx_pedido_cliente_empresa");
                
            builder.HasIndex(p => p.Estado)
                .HasDatabaseName("idx_pedido_estado");
                
            builder.HasIndex(p => p.FechaPedido)
                .HasDatabaseName("idx_pedido_fecha");
                
            builder.HasIndex(p => new { p.EmpresaId, p.Estado, p.FechaPedido })
                .HasDatabaseName("idx_pedido_empresa_estado_fecha");
                
            // Relaciones
            builder.HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasMany(p => p.Items)
                .WithOne(i => i.Pedido)
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}