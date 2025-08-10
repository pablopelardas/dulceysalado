using DistriCatalogoAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Configurations
{
    public class FeatureDefinitionConfiguration : IEntityTypeConfiguration<FeatureDefinition>
    {
        public void Configure(EntityTypeBuilder<FeatureDefinition> builder)
        {
            builder.ToTable("feature_definitions");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Id)
                .HasColumnName("id");
            
            builder.Property(e => e.Codigo)
                .HasColumnName("codigo")
                .HasMaxLength(100)
                .IsRequired();
                
            builder.HasIndex(e => e.Codigo)
                .IsUnique()
                .HasDatabaseName("idx_codigo");
                
            builder.Property(e => e.Nombre)
                .HasColumnName("nombre")
                .HasMaxLength(255)
                .IsRequired();
                
            builder.Property(e => e.Descripcion)
                .HasColumnName("descripcion")
                .HasColumnType("text");
                
            builder.Property(e => e.TipoValor)
                .HasColumnName("tipo_valor")
                .HasConversion<string>()
                .HasDefaultValue(FeatureValueType.Boolean)
                .HasMaxLength(20);
                
            builder.Property(e => e.ValorDefecto)
                .HasColumnName("valor_defecto")
                .HasColumnType("text");
                
            builder.Property(e => e.Categoria)
                .HasColumnName("categoria")
                .HasMaxLength(100);
                
            builder.HasIndex(e => e.Categoria)
                .HasDatabaseName("idx_categoria");
                
            builder.Property(e => e.Activo)
                .HasColumnName("activo")
                .HasDefaultValue(true);
                
            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            builder.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();
                
            builder.HasIndex(e => e.Activo)
                .HasDatabaseName("idx_activo");
        }
    }
}