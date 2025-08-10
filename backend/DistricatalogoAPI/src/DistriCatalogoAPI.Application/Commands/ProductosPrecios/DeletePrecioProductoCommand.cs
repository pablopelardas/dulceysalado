using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductosPrecios
{
    public class DeletePrecioProductoCommand : IRequest<bool>
    {
        public int ProductoId { get; set; }
        public int ListaPrecioId { get; set; }
    }
}