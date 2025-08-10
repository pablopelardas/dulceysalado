using MediatR;

namespace DistriCatalogoAPI.Application.Commands.Clientes
{
    public class DeleteClienteCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string? UpdatedBy { get; set; }
    }
}