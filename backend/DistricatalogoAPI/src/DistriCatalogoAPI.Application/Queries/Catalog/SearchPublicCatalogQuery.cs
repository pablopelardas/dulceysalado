using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Catalog
{
    public class SearchPublicCatalogQuery : IRequest<SearchPublicCatalogQueryResult>
    {
        public int EmpresaId { get; set; }
        public string? Texto { get; set; }
        public List<int>? CodigosRubro { get; set; }
        public List<string>? Categorias { get; set; }
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }
        public bool? SoloDestacados { get; set; }
        public bool? SoloConStock { get; set; }
        public List<string>? Tags { get; set; }
        public string? Marca { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? OrderBy { get; set; } // "precio", "nombre", "destacado", "fecha"
        public bool Ascending { get; set; } = true;
    }

    public class SearchPublicCatalogQueryResult
    {
        public List<ProductoCatalogoDto> Productos { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<FiltroDisponibleDto> FiltrosDisponibles { get; set; } = new();
    }

    public class FiltroDisponibleDto
    {
        public string Tipo { get; set; } = string.Empty; // "categoria", "marca", "tag", "precio"
        public string Nombre { get; set; } = string.Empty;
        public int Count { get; set; }
        public object? Valor { get; set; }
    }
}