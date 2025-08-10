using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Catalog
{
    public class GetPublicCategoriesQuery : IRequest<GetPublicCategoriesQueryResult>
    {
        public int EmpresaId { get; set; }
    }

    public class GetPublicCategoriesQueryResult
    {
        public List<CategoriaPublicaDto> Categorias { get; set; } = new();
    }
}