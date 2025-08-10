using MediatR;

namespace DistriCatalogoAPI.Application.Queries.ProductConfig
{
    public class GetProductsWebConfigQuery : IRequest<GetProductsWebConfigQueryResult>
    {
        public int EmpresaId { get; set; }
        public bool? VisibleOnly { get; set; }
        public bool? FeaturedOnly { get; set; }
        public int? CategoryId { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class GetProductsWebConfigQueryResult
    {
        public IReadOnlyList<ProductWebConfigDto> Products { get; set; } = new List<ProductWebConfigDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ProductWebConfigDto
    {
        public int Id { get; set; }
        public string ProductoCodigo { get; set; }
        public int EmpresaId { get; set; }
        public bool Visible { get; set; }
        public bool Destacado { get; set; }
        public int OrdenCategoria { get; set; }
        public string? DescripcionCorta { get; set; }        
        public List<string> Tags { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
        public string PrimaryImageUrl { get; set; } = string.Empty;
        public bool HasImages { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Información del producto base
        public string ProductoDescripcion { get; set; }
        public decimal ProductoPrecio { get; set; }
        public decimal ProductoStock { get; set; }
        public int? ProductoCategoriaId { get; set; }
        public string? ProductoCategoriaNombre { get; set; }
    }
}