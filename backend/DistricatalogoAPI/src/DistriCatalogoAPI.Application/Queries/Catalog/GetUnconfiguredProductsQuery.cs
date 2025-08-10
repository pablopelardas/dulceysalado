using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Catalog
{
    public class GetUnconfiguredProductsQuery : IRequest<GetUnconfiguredProductsQueryResult>
    {
        public int EmpresaId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetUnconfiguredProductsQueryResult
    {
        public IReadOnlyList<UnconfiguredProductDto> Products { get; set; } = new List<UnconfiguredProductDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UnconfiguredProductDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Descripcion { get; set; } = string.Empty;
        public decimal? Precio { get; set; }
        public decimal? Existencia { get; set; }
        public int? CodigoRubro { get; set; }
        public string? Marca { get; set; }
        public string? UnidadMedida { get; set; }
        public string TipoProducto { get; set; } = string.Empty;
        
        // Campos de configuración web que faltan
        public bool TieneImagen { get; set; }
        public bool TieneDescripcionCorta { get; set; }
        public bool TieneDescripcionLarga { get; set; }
        public bool TieneTags { get; set; }
        public bool EsVisible { get; set; }
        public bool EsDestacado { get; set; }
        
        // Indicadores de qué necesita configuración
        public IReadOnlyList<string> ConfiguracionFaltante { get; set; } = new List<string>();
    }
}