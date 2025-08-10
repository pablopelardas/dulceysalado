using DistriCatalogoAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class EmpresaFeatureConfiguration : IEntityTypeConfiguration<EmpresaFeature>
    {
        public void Configure(EntityTypeBuilder<EmpresaFeature> builder)
        {
            builder.ToTable("empresa_features");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .HasColumnName("id");
            
            builder.Property(e => e.EmpresaId)
                .HasColumnName("empresa_id")
                .IsRequired();
                
            builder.Property(e => e.FeatureId)
                .HasColumnName("feature_id")
                .IsRequired();
                
            builder.HasIndex(e => new { e.EmpresaId, e.FeatureId })
                .IsUnique()
                .HasDatabaseName("uk_empresa_feature");
                
            builder.Property(e => e.Habilitado)
                .HasColumnName("habilitado")
                .HasDefaultValue(true);
                
            builder.Property(e => e.Valor)
                .HasColumnName("valor")
                .HasColumnType("text");
                
            builder.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("json");
                
            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
                
            builder.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(255);
                
            builder.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(255);
                
            // Foreign Keys
            builder.HasOne(e => e.Empresa)
                .WithMany()
                .HasForeignKey(e => e.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(e => e.Feature)
                .WithMany(f => f.EmpresaFeatures)
                .HasForeignKey(e => e.FeatureId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Indices
            builder.HasIndex(e => e.EmpresaId)
                .HasDatabaseName("idx_empresa");
                
            builder.HasIndex(e => e.FeatureId)
                .HasDatabaseName("idx_feature");
                
            builder.HasIndex(e => e.Habilitado)
                .HasDatabaseName("idx_habilitado");
        }
    }
}