using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Pedidos
{
    public class GetPedidosEstadisticasQuery : IRequest<PedidoEstadisticasDto>
    {
        public int EmpresaId { get; set; }
    }
}