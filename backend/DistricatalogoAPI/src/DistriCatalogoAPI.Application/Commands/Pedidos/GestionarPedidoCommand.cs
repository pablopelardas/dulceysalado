using MediatR;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Commands.Pedidos
{
    public class GestionarPedidoCommand : IRequest<bool>
    {
        public int PedidoId { get; set; }
        public PedidoEstado NuevoEstado { get; set; }
        public int UsuarioId { get; set; }
        public string? Motivo { get; set; }
        public string? UpdatedBy { get; set; }
    }
}