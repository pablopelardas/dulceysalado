using System;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Companies
{
    public class UpdateCompanyCommand : IRequest<CompanyDto>
    {
        public int CompanyId { get; set; }
        public string? Nombre { get; set; }
        public string? RazonSocial { get; set; }
        public string? Cuit { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Direccion { get; set; }
        public string? LogoUrl { get; set; }
        public ThemeColorsDto? ColoresTema { get; set; }
        public string? FaviconUrl { get; set; }
        public string? DominioPersonalizado { get; set; }
        public string? UrlWhatsapp { get; set; }
        public string? UrlFacebook { get; set; }
        public string? UrlInstagram { get; set; }
        public bool? MostrarPrecios { get; set; }
        public bool? MostrarStock { get; set; }
        public bool? PermitirPedidos { get; set; }
        public int? ProductosPorPagina { get; set; }
        public bool? PuedeAgregarProductos { get; set; }
        public bool? PuedeAgregarCategorias { get; set; }
        public string? Plan { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public int? ListaPrecioPredeterminadaId { get; set; }
        public int? RequestingUserId { get; set; }
    }
}