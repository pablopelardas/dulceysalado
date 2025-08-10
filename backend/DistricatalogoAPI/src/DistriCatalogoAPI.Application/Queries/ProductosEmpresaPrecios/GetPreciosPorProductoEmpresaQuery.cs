using MediatR;

namespace DistriCatalogoAPI.Application.Queries.ProductosEmpresaPrecios
{
    public class GetPreciosPorProductoEmpresaQuery : IRequest<GetPreciosPorProductoEmpresaQueryResult>
    {
        public int ProductoEmpresaId { get; set; }
    }

    public class GetPreciosPorProductoEmpresaQueryResult
    {
        public int ProductoEmpresaId { get; set; }
        public List<PrecioEmpresaListaDto> Precios { get; set; } = new();
    }

    public class PrecioEmpresaListaDto
    {
        public int ListaPrecioId { get; set; }
        public string ListaPrecioCodigo { get; set; } = "";
        public string ListaPrecioNombre { get; set; } = "";
        public decimal? Precio { get; set; }
        public DateTime? UltimaActualizacion { get; set; }
    }
}