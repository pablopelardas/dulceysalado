using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductosPrecios
{
    public class UpsertPrecioProductoCommand : IRequest<UpsertPrecioProductoCommandResult>
    {
        public int ProductoId { get; set; }
        public int ListaPrecioId { get; set; }
        public decimal Precio { get; set; }
    }

    public class UpsertPrecioProductoCommandResult
    {
        public int ProductoId { get; set; }
        public int ListaPrecioId { get; set; }
        public decimal Precio { get; set; }
        public bool WasCreated { get; set; }
        public string Message { get; set; } = "";
    }
}