using MediatR;

namespace DistriCatalogoAPI.Application.Queries.ProductosPrecios
{
    public class GetPrecioPorProductoYListaQuery : IRequest<GetPrecioPorProductoYListaQueryResult?>
    {
        public int ProductoId { get; set; }
        public int ListaPrecioId { get; set; }
    }

    public class GetPrecioPorProductoYListaQueryResult
    {
        public int ProductoId { get; set; }
        public int ListaPrecioId { get; set; }
        public string ListaPrecioCodigo { get; set; } = "";
        public string ListaPrecioNombre { get; set; } = "";
        public decimal Precio { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
    }
}