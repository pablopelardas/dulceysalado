using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class ClienteRefreshTokenConfiguration : IEntityTypeConfiguration<ClienteRefreshToken>
    {
        public void Configure(EntityTypeBuilder<ClienteRefreshToken> builder)
        {
            builder.ToTable("cliente_refresh_tokens");
            
            builder.HasKey(t => t.Id);
            
            builder.Property(t => t.Id)
                .HasColumnName("id");
                
            builder.Property(t => t.ClienteId)
                .HasColumnName("cliente_id")
                .IsRequired();
                
            builder.Property(t => t.Token)
                .HasColumnName("token")
                .HasMaxLength(500)
                .IsRequired();
                
            builder.Property(t => t.ExpiresAt)
                .HasColumnName("expires_at")
                .IsRequired();
                
            builder.Property(t => t.CreatedAt)
                .HasColumnName("created_at");
                
            // Índices
            builder.HasIndex(t => t.Token)
                .HasDatabaseName("idx_token");
                
            builder.HasIndex(t => t.ExpiresAt)
                .HasDatabaseName("idx_expires");
                
            // Relaciones
            builder.HasOne(t => t.Cliente)
                .WithMany()
                .HasForeignKey(t => t.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}