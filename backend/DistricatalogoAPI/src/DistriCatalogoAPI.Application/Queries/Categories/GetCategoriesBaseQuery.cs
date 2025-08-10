using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Categories
{
    public class GetCategoriesBaseQuery : IRequest<GetCategoriesBaseQueryResult>
    {
        public bool? VisibleOnly { get; set; }
        public int? EmpresaId { get; set; }
    }

    public class GetCategoriesBaseQueryResult
    {
        public IReadOnlyList<CategoryBaseDto> Categories { get; set; } = new List<CategoryBaseDto>();
    }

    public class CategoryBaseDto
    {
        public int Id { get; set; }
        public int CodigoRubro { get; set; }
        public string Nombre { get; set; }
        public string Icono { get; set; }
        public bool Visible { get; set; }
        public int Orden { get; set; }
        public string Color { get; set; }
        public string? Descripcion { get; set; }
        public int CreatedByEmpresaId { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}