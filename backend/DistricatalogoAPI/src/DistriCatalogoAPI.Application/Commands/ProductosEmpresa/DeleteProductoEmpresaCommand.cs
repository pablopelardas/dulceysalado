using MediatR;

namespace DistriCatalogoAPI.Application.Commands.ProductosEmpresa
{
    public class DeleteProductoEmpresaCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}