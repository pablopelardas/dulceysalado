using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosPrecios;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosPrecios
{
    public class UpsertPrecioProductoCommandHandler : IRequestHandler<UpsertPrecioProductoCommand, UpsertPrecioProductoCommandResult>
    {
        private readonly IProductBasePrecioRepository _precioRepository;
        private readonly IProductBaseRepository _productRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ILogger<UpsertPrecioProductoCommandHandler> _logger;

        public UpsertPrecioProductoCommandHandler(
            IProductBasePrecioRepository precioRepository,
            IProductBaseRepository productRepository,
            IListaPrecioRepository listaPrecioRepository,
            ILogger<UpsertPrecioProductoCommandHandler> logger)
        {
            _precioRepository = precioRepository;
            _productRepository = productRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _logger = logger;
        }

        public async Task<UpsertPrecioProductoCommandResult> Handle(UpsertPrecioProductoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Upsert precio: producto {ProductoId}, lista {ListaId}, precio {Precio}", 
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

            // Verificar si ya existe un precio
            var preciosExistentes = await _precioRepository.GetPreciosPorProductoYListaAsync(request.ProductoId, request.ListaPrecioId);
            var wasCreated = !preciosExistentes.Any();

            // Realizar upsert
            var success = await _precioRepository.UpsertPrecioAsync(request.ProductoId, request.ListaPrecioId, request.Precio);

            if (!success)
            {
                throw new InvalidOperationException("Error al actualizar el precio");
            }

            var message = wasCreated ? "Precio creado exitosamente" : "Precio actualizado exitosamente";

            return new UpsertPrecioProductoCommandResult
            {
                ProductoId = request.ProductoId,
                ListaPrecioId = request.ListaPrecioId,
                Precio = request.Precio,
                WasCreated = wasCreated,
                Message = message
            };
        }
    }
}