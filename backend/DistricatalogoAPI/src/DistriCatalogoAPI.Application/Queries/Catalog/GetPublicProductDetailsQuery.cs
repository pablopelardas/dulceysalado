using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Catalog
{
    public class GetPublicProductDetailsQuery : IRequest<GetPublicProductDetailsQueryResult?>
    {
        public int EmpresaId { get; set; }
        public string? ListaPrecioCodigo { get; set; }
        public string ProductoCodigo { get; set; } = string.Empty;
    }

    public class GetPublicProductDetailsQueryResult
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? DescripcionLarga { get; set; }
        public decimal? Precio { get; set; }
        public decimal? PrecioEspecial { get; set; }
        public bool Destacado { get; set; }
        public List<string> ImagenUrls { get; set; } = new();
        public string? VideoUrl { get; set; }
        public int? Stock { get; set; }
        public bool MostrarStock { get; set; }
        public bool MostrarPrecio { get; set; }
        public bool PermitirConsulta { get; set; }
        public string? Categoria { get; set; }
        public string? CategoriaColor { get; set; }
        public string? CategoriaIcono { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? Marca { get; set; }
        public string? Unidad { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }
        public List<ProductoRelacionadoDto> ProductosRelacionados { get; set; } = new();
    }

    public class ProductoRelacionadoDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal? Precio { get; set; }
        public string? ImagenUrl { get; set; }
        public bool Destacado { get; set; }
    }
}