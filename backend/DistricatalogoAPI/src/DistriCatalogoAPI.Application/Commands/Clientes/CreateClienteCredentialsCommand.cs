using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Clientes
{
    public class CreateClienteCredentialsCommand : IRequest<bool>
    {
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
    }
}