using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Application.DTOs
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string RazonSocial { get; set; }
        public string Cuit { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string TipoEmpresa { get; set; }
        public int? EmpresaPrincipalId { get; set; }
        public string LogoUrl { get; set; }
        public ThemeColorsDto ColoresTema { get; set; }
        public string FaviconUrl { get; set; }
        public string DominioPersonalizado { get; set; }
        public string UrlWhatsapp { get; set; }
        public string UrlFacebook { get; set; }
        public string UrlInstagram { get; set; }
        public bool MostrarPrecios { get; set; }
        public bool MostrarStock { get; set; }
        public bool PermitirPedidos { get; set; }
        public int ProductosPorPagina { get; set; }
        public bool PuedeAgregarProductos { get; set; }
        public bool PuedeAgregarCategorias { get; set; }
        public bool Activa { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Plan { get; set; }
        public int? ListaPrecioPredeterminadaId { get; set; }
        public string? ListaPrecioPredeterminadaNombre { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<FeatureConfigurationDto> Features { get; set; } = new List<FeatureConfigurationDto>();
    }

    public class ThemeColorsDto
    {
        public string Acento { get; set; }
        public string Primario { get; set; }
        public string Secundario { get; set; }
    }
}