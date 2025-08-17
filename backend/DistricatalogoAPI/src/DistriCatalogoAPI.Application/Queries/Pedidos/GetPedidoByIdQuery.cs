using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Pedidos
{
    public class GetPedidoByIdQuery : IRequest<PedidoDto?>
    {
        public int PedidoId { get; set; }
        public int EmpresaId { get; set; }
        public bool IncludeItems { get; set; } = true;
    }
}