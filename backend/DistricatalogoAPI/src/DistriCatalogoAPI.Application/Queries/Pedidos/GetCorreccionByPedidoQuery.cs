using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Pedidos
{
    public class GetCorreccionByPedidoQuery : IRequest<CorreccionDto?>
    {
        public int PedidoId { get; set; }
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
    }
}