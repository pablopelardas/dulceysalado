using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class ClienteLoginHistoryConfiguration : IEntityTypeConfiguration<ClienteLoginHistory>
    {
        public void Configure(EntityTypeBuilder<ClienteLoginHistory> builder)
        {
            builder.ToTable("cliente_login_history");
            
            builder.HasKey(h => h.Id);
            
            builder.Property(h => h.Id)
                .HasColumnName("id");
                
            builder.Property(h => h.ClienteId)
                .HasColumnName("cliente_id")
                .IsRequired();
                
            builder.Property(h => h.LoginAt)
                .HasColumnName("login_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(h => h.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(45);
                
            builder.Property(h => h.UserAgent)
                .HasColumnName("user_agent")
                .HasMaxLength(500);
                
            builder.Property(h => h.Success)
                .HasColumnName("success")
                .HasDefaultValue(true);
                
            // Índices
            builder.HasIndex(h => new { h.ClienteId, h.LoginAt })
                .HasDatabaseName("idx_cliente_login");
                
            // Relaciones
            builder.HasOne(h => h.Cliente)
                .WithMany()
                .HasForeignKey(h => h.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}