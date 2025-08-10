using MediatR;

namespace DistriCatalogoAPI.Application.Queries.Categories
{
    public class GetCategoryEmpresaByIdQuery : IRequest<GetCategoryEmpresaByIdQueryResult?>
    {
        public int Id { get; set; }
    }

    public class GetCategoryEmpresaByIdQueryResult
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int CodigoRubro { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public bool Visible { get; set; }
        public int Orden { get; set; }
        public string Color { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}