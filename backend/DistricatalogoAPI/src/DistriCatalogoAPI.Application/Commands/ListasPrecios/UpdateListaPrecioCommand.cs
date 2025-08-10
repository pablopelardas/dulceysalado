using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ListasPrecios
{
    public class UpdateListaPrecioCommand : IRequest<UpdateListaPrecioCommandResult>
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool? Activa { get; set; }
        public int? Orden { get; set; }
        public int EmpresaId { get; set; }
    }

    public class UpdateListaPrecioCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}