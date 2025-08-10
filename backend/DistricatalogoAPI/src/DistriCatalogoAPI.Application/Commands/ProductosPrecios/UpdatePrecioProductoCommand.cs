using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductosPrecios
{
    public class UpdatePrecioProductoCommand : IRequest<UpdatePrecioProductoCommandResult>
    {
        public int ProductoId { get; set; }
        public int ListaPrecioId { get; set; }
        public decimal Precio { get; set; }
    }

    public class UpdatePrecioProductoCommandResult
    {
        public int ProductoId { get; set; }
        public int ListaPrecioId { get; set; }
        public decimal Precio { get; set; }
        public string Message { get; set; } = "";
    }
}