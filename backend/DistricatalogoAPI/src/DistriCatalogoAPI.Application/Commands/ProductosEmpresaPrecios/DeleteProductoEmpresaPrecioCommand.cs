using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductosEmpresaPrecios
{
    public class DeleteProductoEmpresaPrecioCommand : IRequest<DeleteProductoEmpresaPrecioCommandResult>
    {
        public int ProductoEmpresaId { get; set; }
        public int ListaPrecioId { get; set; }
    }

    public class DeleteProductoEmpresaPrecioCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}