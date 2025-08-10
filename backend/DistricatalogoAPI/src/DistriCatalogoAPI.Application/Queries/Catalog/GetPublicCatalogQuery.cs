using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Catalog
{
    public class GetPublicCatalogQuery : IRequest<GetPublicCatalogQueryResult>
    {
        public int EmpresaId { get; set; }
        public string? ListaPrecioCodigo { get; set; }
        public string? Categoria { get; set; }
        public string? Busqueda { get; set; }
        public bool? Destacados { get; set; }
        public int? CodigoRubro { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? OrdenarPor { get; set; } // "precio_asc", "precio_desc", "nombre_asc", "nombre_desc"
    }

    public class GetPublicCatalogQueryResult
    {
        public List<ProductoCatalogoDto> Productos { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<CategoriaPublicaDto> Categorias { get; set; } = new();
    }

    public class ProductoCatalogoDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? DescripcionCorta { get; set; }
        public decimal? Precio { get; set; }
        public bool Destacado { get; set; }
        public List<string> ImagenUrls { get; set; } = new();
        public int? Stock { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? Marca { get; set; }
        public string? Unidad { get; set; }
        public string? CodigoBarras { get; set; }
        public int? CodigoRubro { get; set; }
        public string? ImagenAlt { get; set; }
        public string TipoProducto { get; set; } = string.Empty;
        
        // Price list information
        public int? ListaPrecioId { get; set; }
        public string? ListaPrecioNombre { get; set; }
        public string? ListaPrecioCodigo { get; set; }
    }

    public class CategoriaPublicaDto
    {
        public int Id { get; set; }
        public int CodigoRubro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Icono { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Orden { get; set; }
        public int ProductCount { get; set; }
    }
}