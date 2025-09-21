using DistriCatalogoAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistriCatalogoAPI.Infrastructure.Data.Configurations
{
    public class SolicitudReventaConfiguration : IEntityTypeConfiguration<SolicitudReventa>
    {
        public void Configure(EntityTypeBuilder<SolicitudReventa> builder)
        {
            builder.ToTable("solicitudes_reventa");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(s => s.ClienteId)
                .HasColumnName("cliente_id")
                .IsRequired();

            builder.Property(s => s.EmpresaId)
                .HasColumnName("empresa_id")
                .IsRequired();

            builder.Property(s => s.Cuit)
                .HasColumnName("cuit")
                .HasMaxLength(20);

            builder.Property(s => s.RazonSocial)
                .HasColumnName("razon_social")
                .HasMaxLength(200);

            builder.Property(s => s.DireccionComercial)
                .HasColumnName("direccion_comercial")
                .HasMaxLength(300);

            builder.Property(s => s.Localidad)
                .HasColumnName("localidad")
                .HasMaxLength(100);

            builder.Property(s => s.Provincia)
                .HasColumnName("provincia")
                .HasMaxLength(100);

            builder.Property(s => s.CodigoPostal)
                .HasColumnName("codigo_postal")
                .HasMaxLength(20);

            builder.Property(s => s.TelefonoComercial)
                .HasColumnName("telefono_comercial")
                .HasMaxLength(50);

            builder.Property(s => s.CategoriaIva)
                .HasColumnName("categoria_iva")
                .HasMaxLength(50);

            builder.Property(s => s.EmailComercial)
                .HasColumnName("email_comercial")
                .HasMaxLength(200);

            builder.Property(s => s.Estado)
                .HasColumnName("estado")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(s => s.ComentarioRespuesta)
                .HasColumnName("comentario_respuesta")
                .HasMaxLength(500);

            builder.Property(s => s.FechaRespuesta)
                .HasColumnName("fecha_respuesta");

            builder.Property(s => s.RespondidoPor)
                .HasColumnName("respondido_por")
                .HasMaxLength(100);

            builder.Property(s => s.FechaSolicitud)
                .HasColumnName("fecha_solicitud")
                .IsRequired();

            builder.Property(s => s.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            // Relación con Cliente
            builder.HasOne(s => s.Cliente)
                .WithMany()
                .HasForeignKey(s => s.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(s => s.ClienteId);
            builder.HasIndex(s => s.EmpresaId);
            builder.HasIndex(s => new { s.ClienteId, s.Estado });
        }
    }
}