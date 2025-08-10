using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class FeatureDefinition
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public FeatureValueType TipoValor { get; set; } = FeatureValueType.Boolean;
        public string? ValorDefecto { get; set; }
        public string? Categoria { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation
        public virtual ICollection<EmpresaFeature> EmpresaFeatures { get; set; } = new List<EmpresaFeature>();
    }
    
    public enum FeatureValueType
    {
        Boolean,
        String,
        Number,
        Json
    }
}