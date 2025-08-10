using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosPrecios;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosPrecios
{
    public class DeletePrecioProductoCommandHandler : IRequestHandler<DeletePrecioProductoCommand, bool>
    {
        private readonly IProductBasePrecioRepository _precioRepository;
        private readonly IProductBaseRepository _productRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ILogger<DeletePrecioProductoCommandHandler> _logger;

        public DeletePrecioProductoCommandHandler(
            IProductBasePrecioRepository precioRepository,
            IProductBaseRepository productRepository,
            IListaPrecioRepository listaPrecioRepository,
            ILogger<DeletePrecioProductoCommandHandler> logger)
        {
            _precioRepository = precioRepository;
            _productRepository = productRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeletePrecioProductoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Delete precio: producto {ProductoId}, lista {ListaId}", 
                request.ProductoId, request.ListaPrecioId);

            // Validar que el producto existe
            var producto = await _productRepository.GetByIdAsync(request.ProductoId);
            if (producto == null)
            {
                throw new InvalidOperationException($"Producto con ID {request.ProductoId} no encontrado");
            }

            // Validar que la lista de precios existe
            var lista = await _listaPrecioRepository.GetCodigoAndNombreByIdAsync(request.ListaPrecioId);
            if (lista == null)
            {
                throw new InvalidOperationException($"Lista de precios con ID {request.ListaPrecioId} no encontrada");
            }

            // Realizar eliminación
            var success = await _precioRepository.DeletePrecioAsync(request.ProductoId, request.ListaPrecioId);

            if (success)
            {
                _logger.LogInformation("Precio eliminado exitosamente para producto {ProductoId} en lista {ListaId}", 
                    request.ProductoId, request.ListaPrecioId);
            }
            else
            {
                _logger.LogWarning("Precio no encontrado para eliminar: producto {ProductoId}, lista {ListaId}", 
                    request.ProductoId, request.ListaPrecioId);
            }

            return success;
        }
    }
}