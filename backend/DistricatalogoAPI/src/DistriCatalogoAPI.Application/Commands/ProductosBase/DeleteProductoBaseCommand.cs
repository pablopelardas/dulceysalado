using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductosBase
{
    public class DeleteProductoBaseCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}