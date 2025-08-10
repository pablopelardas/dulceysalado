using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ListasPrecios
{
    public class CreateListaPrecioCommand : IRequest<CreateListaPrecioCommandResult>
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? Orden { get; set; }
        public int EmpresaId { get; set; }
    }

    public class CreateListaPrecioCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ListaId { get; set; }
    }
}