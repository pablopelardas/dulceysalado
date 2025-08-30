using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Pedidos
{
    public class ResponderCorreccionCommand : IRequest<bool>
    {
        public string Token { get; set; } = string.Empty;
        public bool Aprobado { get; set; }
        public string? ComentarioCliente { get; set; }
    }
}