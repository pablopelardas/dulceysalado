using System;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class EmpresaFeature
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int FeatureId { get; set; }
        public bool Habilitado { get; set; } = true;
        public string? Valor { get; set; }
        public string? Metadata { get; set; } // JSON
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Navigation
        public virtual Company Empresa { get; set; } = null!;
        public virtual FeatureDefinition Feature { get; set; } = null!;
    }
}