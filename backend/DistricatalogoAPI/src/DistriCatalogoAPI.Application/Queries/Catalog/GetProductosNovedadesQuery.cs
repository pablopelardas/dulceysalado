using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Catalog
{
    public class GetProductosNovedadesQuery : IRequest<GetProductosNovedadesQueryResult>
    {
        public int EmpresaId { get; set; }
        public string? ListaPrecioCodigo { get; set; }
        public string? OrdenarPor { get; set; }
    }

    public class GetProductosNovedadesQueryResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<ProductoCatalogoDto> Productos { get; set; } = new();
        public int TotalProductos { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;
    }
}