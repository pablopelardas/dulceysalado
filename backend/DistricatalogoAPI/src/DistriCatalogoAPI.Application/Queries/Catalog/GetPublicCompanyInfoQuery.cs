using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Catalog
{
    public class GetPublicCompanyInfoQuery : IRequest<GetPublicCompanyInfoQueryResult>
    {
        public int EmpresaId { get; set; }
    }

    public class GetPublicCompanyInfoQueryResult
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string ColoresTema { get; set; } = string.Empty;
        public string FaviconUrl { get; set; } = string.Empty;
        public string DominioPersonalizado { get; set; } = string.Empty;
        public string UrlWhatsapp { get; set; } = string.Empty;
        public string UrlFacebook { get; set; } = string.Empty;
        public string UrlInstagram { get; set; } = string.Empty;
        
        // Configuraciones del catálogo
        public bool MostrarPrecios { get; set; }
        public bool MostrarStock { get; set; }
        public bool PermitirPedidos { get; set; }
        public int ProductosPorPagina { get; set; }
        
        // Información del plan
        public string Plan { get; set; } = string.Empty;
        public bool Activa { get; set; }
        
        // Features habilitadas
        public List<FeatureConfigurationDto> Features { get; set; } = new();
    }
}