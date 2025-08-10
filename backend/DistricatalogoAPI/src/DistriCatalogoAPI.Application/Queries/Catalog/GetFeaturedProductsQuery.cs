using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Catalog
{
    public class GetFeaturedProductsQuery : IRequest<GetFeaturedProductsQueryResult>
    {
        public int EmpresaId { get; set; }
        public string? ListaPrecioCodigo { get; set; }
        public int Limit { get; set; } = 10;
    }

    public class GetFeaturedProductsQueryResult
    {
        public List<ProductoCatalogoDto> Productos { get; set; } = new();
        public int TotalCount { get; set; }
    }
}