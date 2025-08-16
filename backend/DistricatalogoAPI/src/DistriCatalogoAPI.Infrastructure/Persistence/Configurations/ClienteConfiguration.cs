using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("clientes");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Id)
                .HasColumnName("id");
                
            builder.Property(c => c.EmpresaId)
                .HasColumnName("empresa_id")
                .IsRequired();
                
            builder.Property(c => c.Codigo)
                .HasColumnName("codigo")
                .HasMaxLength(20)
                .IsRequired();
                
            builder.Property(c => c.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(255);
                
            builder.Property(c => c.Direccion)
                .HasColumnName("direccion")
                .HasMaxLength(500);
                
            builder.Property(c => c.Localidad)
                .HasColumnName("localidad")
                .HasMaxLength(255);
                
            builder.Property(c => c.Telefono)
                .HasColumnName("telefono")
                .HasMaxLength(100);
                
            builder.Property(c => c.Cuit)
                .HasColumnName("cuit")
                .HasMaxLength(50);
                
            builder.Property(c => c.Altura)
                .HasColumnName("altura")
                .HasMaxLength(50);
                
            builder.Property(c => c.Provincia)
                .HasColumnName("provincia")
                .HasMaxLength(100);
                
            builder.Property(c => c.Email)
                .HasColumnName("email")
                .HasMaxLength(255);
                
            builder.Property(c => c.TipoIva)
                .HasColumnName("tipo_iva")
                .HasMaxLength(50);
                
            // Campos de autenticación
            builder.Property(c => c.Username)
                .HasColumnName("username")
                .HasMaxLength(100);
                
            builder.Property(c => c.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255);
                
            builder.Property(c => c.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(false);
                
            builder.Property(c => c.LastLogin)
                .HasColumnName("last_login");
                
            // Campos para autenticación social
            builder.Property(c => c.GoogleId)
                .HasColumnName("google_id")
                .HasMaxLength(255);
                
            builder.Property(c => c.FotoUrl)
                .HasColumnName("foto_url");
                
            builder.Property(c => c.EmailVerificado)
                .HasColumnName("email_verificado")
                .HasDefaultValue(false);
                
            builder.Property(c => c.ProveedorAuth)
                .HasColumnName("proveedor_auth")
                .HasMaxLength(50);
                
            builder.Property(c => c.ListaPrecioId)
                .HasColumnName("lista_precio_id");
                
            // Auditoría
            builder.Property(c => c.Activo)
                .HasColumnName("activo")
                .HasDefaultValue(true);
                
            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(c => c.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(c => c.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(100);
                
            builder.Property(c => c.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(100);
                
            // Índices
            builder.HasIndex(c => new { c.EmpresaId, c.Codigo })
                .HasDatabaseName("idx_empresa_codigo")
                .IsUnique();
                
            builder.HasIndex(c => new { c.EmpresaId, c.Username })
                .HasDatabaseName("idx_empresa_username")
                .IsUnique()
                .HasFilter("username IS NOT NULL");
                
            builder.HasIndex(c => c.Nombre)
                .HasDatabaseName("idx_nombre");
                
            builder.HasIndex(c => c.Cuit)
                .HasDatabaseName("idx_cuit");
                
            builder.HasIndex(c => c.Localidad)
                .HasDatabaseName("idx_localidad");
                
            builder.HasIndex(c => c.Activo)
                .HasDatabaseName("idx_activo");
                
            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("idx_is_active");
                
            // Índices para autenticación social
            builder.HasIndex(c => c.GoogleId)
                .HasDatabaseName("idx_clientes_google_id");
                
            builder.HasIndex(c => new { c.Email, c.EmpresaId })
                .HasDatabaseName("idx_clientes_email_empresa");
                
            builder.HasIndex(c => c.ProveedorAuth)
                .HasDatabaseName("idx_clientes_proveedor_auth");
                
            // Relaciones
            builder.HasOne<Infrastructure.Models.Empresa>()
                .WithMany()
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne<Infrastructure.Models.ListasPrecio>()
                .WithMany()
                .HasForeignKey(c => c.ListaPrecioId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}