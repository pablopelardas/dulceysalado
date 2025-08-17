using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class PedidoItemConfiguration : IEntityTypeConfiguration<PedidoItem>
    {
        public void Configure(EntityTypeBuilder<PedidoItem> builder)
        {
            builder.ToTable("pedido_items");
            
            builder.HasKey(pi => pi.Id);
            
            builder.Property(pi => pi.Id)
                .HasColumnName("id");
                
            builder.Property(pi => pi.PedidoId)
                .HasColumnName("pedido_id")
                .IsRequired();
                
            builder.Property(pi => pi.CodigoProducto)
                .HasColumnName("codigo_producto")
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(pi => pi.NombreProducto)
                .HasColumnName("nombre_producto")
                .HasMaxLength(255)
                .IsRequired();
                
            builder.Property(pi => pi.Cantidad)
                .HasColumnName("cantidad")
                .IsRequired();
                
            builder.Property(pi => pi.PrecioUnitario)
                .HasColumnName("precio_unitario")
                .HasColumnType("decimal(10,2)")
                .IsRequired();
                
            builder.Property(pi => pi.Observaciones)
                .HasColumnName("observaciones")
                .HasMaxLength(500);
                
            // Auditoría
            builder.Property(pi => pi.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(pi => pi.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            // Índices
            builder.HasIndex(pi => pi.PedidoId)
                .HasDatabaseName("idx_pedido_item_pedido_id");
                
            builder.HasIndex(pi => pi.CodigoProducto)
                .HasDatabaseName("idx_pedido_item_codigo_producto");
                
            // Relaciones
            builder.HasOne(pi => pi.Pedido)
                .WithMany(p => p.Items)
                .HasForeignKey(pi => pi.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}