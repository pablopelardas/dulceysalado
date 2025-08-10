using MediatR;

namespace DistriCatalogoAPI.Application.Queries.ListasPrecios
{
    public class GetListaPrecioByIdQuery : IRequest<GetListaPrecioByIdQueryResult?>
    {
        public int Id { get; set; }
    }

    public class GetListaPrecioByIdQueryResult
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public bool Activa { get; set; }
        public bool EsPredeterminada { get; set; }
        public int? Orden { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}