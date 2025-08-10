using MediatR;

namespace DistriCatalogoAPI.Application.Queries.ListasPrecios
{
    public class GetAllListasPreciosQuery : IRequest<GetAllListasPreciosQueryResult>
    {
    }

    public class GetAllListasPreciosQueryResult
    {
        public List<ListaPrecioDto> Listas { get; set; } = new();
    }

    public class ListaPrecioDto
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