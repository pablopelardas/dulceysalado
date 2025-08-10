using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Application.DTOs
{
    public class FeatureConfigurationDto
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string TipoValor { get; set; }
        public string? Categoria { get; set; }
        public bool Habilitado { get; set; }
        public string? Valor { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class FeatureDefinitionDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string TipoValor { get; set; }
        public string? ValorDefecto { get; set; }
        public string? Categoria { get; set; }
        public bool Activo { get; set; }
    }

    public class EmpresaFeatureDto
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int FeatureId { get; set; }
        public string FeatureCodigo { get; set; }
        public string FeatureNombre { get; set; }
        public bool Habilitado { get; set; }
        public string? Valor { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class PublicFeatureDto
    {
        public string Codigo { get; set; }
        public bool Habilitado { get; set; }
        public string? Valor { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }
}