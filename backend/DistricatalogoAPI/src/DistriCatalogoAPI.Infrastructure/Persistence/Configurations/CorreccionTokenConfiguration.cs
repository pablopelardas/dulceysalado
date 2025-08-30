using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class CorreccionTokenConfiguration : IEntityTypeConfiguration<CorreccionToken>
    {
        public void Configure(EntityTypeBuilder<CorreccionToken> builder)
        {
            builder.ToTable("correccion_tokens");

            builder.HasKey(ct => ct.Id);
            builder.Property(ct => ct.Id).HasColumnName("id");

            builder.Property(ct => ct.Token)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("token");

            builder.Property(ct => ct.PedidoId)
                .IsRequired()
                .HasColumnName("pedido_id");

            builder.Property(ct => ct.FechaCreacion)
                .IsRequired()
                .HasColumnName("fecha_creacion")
                .HasColumnType("datetime");

            builder.Property(ct => ct.FechaExpiracion)
                .IsRequired()
                .HasColumnName("fecha_expiracion")
                .HasColumnType("datetime");

            builder.Property(ct => ct.Usado)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("usado");

            builder.Property(ct => ct.FechaUso)
                .HasColumnName("fecha_uso")
                .HasColumnType("datetime");

            builder.Property(ct => ct.RespuestaCliente)
                .HasMaxLength(20)
                .HasColumnName("respuesta_cliente");

            builder.Property(ct => ct.ComentarioCliente)
                .HasMaxLength(500)
                .HasColumnName("comentario_cliente");

            builder.Property(ct => ct.MotivoCorreccion)
                .HasColumnType("longtext")
                .HasColumnName("motivo_correccion");

            builder.Property(ct => ct.PedidoOriginalJson)
                .IsRequired()
                .HasColumnType("longtext")
                .HasColumnName("pedido_original_json");

            // Relaciones
            builder.HasOne(ct => ct.Pedido)
                .WithMany()
                .HasForeignKey(ct => ct.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(ct => ct.Token)
                .IsUnique()
                .HasDatabaseName("IX_correccion_tokens_token");

            builder.HasIndex(ct => ct.PedidoId)
                .HasDatabaseName("IX_correccion_tokens_pedido_id");

            builder.HasIndex(ct => ct.FechaExpiracion)
                .HasDatabaseName("IX_correccion_tokens_fecha_expiracion");
        }
    }
}