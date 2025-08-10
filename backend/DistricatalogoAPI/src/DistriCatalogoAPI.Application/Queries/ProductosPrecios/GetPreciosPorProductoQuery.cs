using MediatR;

namespace DistriCatalogoAPI.Application.Queries.ProductosPrecios
{
    public class GetPreciosPorProductoQuery : IRequest<GetPreciosPorProductoQueryResult>
    {
        public int ProductoId { get; set; }
    }

    public class GetPreciosPorProductoQueryResult
    {
        public int ProductoId { get; set; }
        public List<PrecioListaDto> Precios { get; set; } = new();
    }

    public class PrecioListaDto
    {
        public int ListaPrecioId { get; set; }
        public string ListaPrecioCodigo { get; set; } = "";
        public string ListaPrecioNombre { get; set; } = "";
        public decimal Precio { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
    }
}