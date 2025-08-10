using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ListasPrecios
{
    public class DeleteListaPrecioCommand : IRequest<DeleteListaPrecioCommandResult>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
    }

    public class DeleteListaPrecioCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}