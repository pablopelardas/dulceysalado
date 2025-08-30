using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Pedidos
{
    public class RechazarCorreccionCommand : IRequest<bool>
    {
        public int PedidoId { get; set; }
        public int ClienteId { get; set; }
        public string? Comentario { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }
}