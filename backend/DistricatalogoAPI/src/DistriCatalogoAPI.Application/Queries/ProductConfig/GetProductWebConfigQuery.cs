using MediatR;

namespace DistriCatalogoAPI.Application.Queries.ProductConfig
{
    public class GetProductWebConfigQuery : IRequest<GetProductWebConfigQueryResult?>
    {
        public string ProductoCodigo { get; set; }
    }

    public class GetProductWebConfigQueryResult
    {
        public int Id { get; set; }
        public string ProductoCodigo { get; set; }
        public int EmpresaId { get; set; }
        public bool Visible { get; set; }
        public bool Destacado { get; set; }
        public int OrdenCategoria { get; set; }
        public string? DescripcionCorta { get; set; }
        public string? DescripcionLarga { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? SeoTitle { get; set; }
        public string? SeoDescription { get; set; }
        public string? SeoKeywords { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string PrimaryImageUrl { get; set; } = string.Empty;
        public bool HasImages { get; set; }
        public bool HasSeoConfiguration { get; set; }
        public bool HasExtendedDescription { get; set; }
        public bool HasTags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Información del producto base (para mostrar contexto)
        public ProductBaseInfo? ProductBase { get; set; }
    }

    public class ProductBaseInfo
    {
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public decimal Stock { get; set; }
        public int? CategoriaId { get; set; }
        public string? CategoriaNombre { get; set; }
    }
}