using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductosEmpresaPrecios
{
    public class UpsertProductoEmpresaPrecioCommand : IRequest<UpsertProductoEmpresaPrecioCommandResult>
    {
        public int ProductoEmpresaId { get; set; }
        public int ListaPrecioId { get; set; }
        public decimal Precio { get; set; }
    }

    public class UpsertProductoEmpresaPrecioCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}