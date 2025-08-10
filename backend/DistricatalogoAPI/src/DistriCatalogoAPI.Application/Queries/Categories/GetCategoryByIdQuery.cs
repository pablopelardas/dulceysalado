using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Categories
{
    public class GetCategoryByIdQuery : IRequest<GetCategoryByIdQueryResult?>
    {
        public int Id { get; set; }
        public bool IsBaseCategory { get; set; }
    }

    public class GetCategoryByIdQueryResult
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; } // Null for base categories
        public int CodigoRubro { get; set; }
        public string Nombre { get; set; }
        public string Icono { get; set; }
        public bool Visible { get; set; }
        public int Orden { get; set; }
        public string Color { get; set; }
        public string? Descripcion { get; set; }
        public int? CreatedByEmpresaId { get; set; } // Only for base categories
        public int ProductCount { get; set; }
        public bool IsBaseCategory { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}