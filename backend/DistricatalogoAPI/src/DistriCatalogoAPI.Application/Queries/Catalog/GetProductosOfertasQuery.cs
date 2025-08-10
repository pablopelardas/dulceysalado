using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Catalog
{
    public class GetProductosOfertasQuery : IRequest<GetProductosOfertasQueryResult>
    {
        public int EmpresaId { get; set; }
        public string? ListaPrecioCodigo { get; set; }
        public string? OrdenarPor { get; set; }
    }

    public class GetProductosOfertasQueryResult 
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<ProductoCatalogoDto> Productos { get; set; } = new();
        public int TotalProductos { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;
    }
}