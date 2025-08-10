using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosPrecios;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosPrecios
{
    public class UpdatePrecioProductoCommandHandler : IRequestHandler<UpdatePrecioProductoCommand, UpdatePrecioProductoCommandResult>
    {
        private readonly IProductBasePrecioRepository _precioRepository;
        private readonly IProductBaseRepository _productRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ILogger<UpdatePrecioProductoCommandHandler> _logger;

        public UpdatePrecioProductoCommandHandler(
            IProductBasePrecioRepository precioRepository,
            IProductBaseRepository productRepository,
            IListaPrecioRepository listaPrecioRepository,
            ILogger<UpdatePrecioProductoCommandHandler> logger)
        {
            _precioRepository = precioRepository;
            _productRepository = productRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _logger = logger;
        }

        public async Task<UpdatePrecioProductoCommandResult> Handle(UpdatePrecioProductoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Update precio: producto {ProductoId}, lista {ListaId}, precio {Precio}", 
                request.ProductoId, request.ListaPrecioId, request.Precio);

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

            // Validar precio
            if (request.Precio < 0)
            {
                throw new InvalidOperationException("El precio no puede ser negativo");
            }

            // Verificar que existe el precio a actualizar
            var preciosExistentes = await _precioRepository.GetPreciosPorProductoYListaAsync(request.ProductoId, request.ListaPrecioId);
            if (!preciosExistentes.Any())
            {
                throw new InvalidOperationException($"No existe precio para el producto {request.ProductoId} en la lista {request.ListaPrecioId}");
            }

            // Realizar actualización
            var success = await _precioRepository.UpsertPrecioAsync(request.ProductoId, request.ListaPrecioId, request.Precio);

            if (!success)
            {
                throw new InvalidOperationException("Error al actualizar el precio");
            }

            return new UpdatePrecioProductoCommandResult
            {
                ProductoId = request.ProductoId,
                ListaPrecioId = request.ListaPrecioId,
                Precio = request.Precio,
                Message = "Precio actualizado exitosamente"
            };
        }
    }
}