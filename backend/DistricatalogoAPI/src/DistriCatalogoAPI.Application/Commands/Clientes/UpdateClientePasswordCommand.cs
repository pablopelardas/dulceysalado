using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Clientes
{
    public class UpdateClientePasswordCommand : IRequest<bool>
    {
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
    }
}