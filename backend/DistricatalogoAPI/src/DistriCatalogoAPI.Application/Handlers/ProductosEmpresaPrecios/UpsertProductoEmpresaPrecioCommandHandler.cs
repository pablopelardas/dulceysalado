using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosEmpresaPrecios;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosEmpresaPrecios
{
    public class UpsertProductoEmpresaPrecioCommandHandler : IRequestHandler<UpsertProductoEmpresaPrecioCommand, UpsertProductoEmpresaPrecioCommandResult>
    {
        private readonly IProductoEmpresaPrecioRepository _precioRepository;
        private readonly IProductoEmpresaRepository _productRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpsertProductoEmpresaPrecioCommandHandler> _logger;

        public UpsertProductoEmpresaPrecioCommandHandler(
            IProductoEmpresaPrecioRepository precioRepository,
            IProductoEmpresaRepository productRepository,
            IListaPrecioRepository listaPrecioRepository,
            ICurrentUserService currentUserService,
            ILogger<UpsertProductoEmpresaPrecioCommandHandler> logger)
        {
            _precioRepository = precioRepository;
            _productRepository = productRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<UpsertProductoEmpresaPrecioCommandResult> Handle(UpsertProductoEmpresaPrecioCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = await _currentUserService.GetCurrentUserAsync();

                _logger.LogInformation("Upsert precio empresa - ProductoId: {ProductoId}, ListaId: {ListaId}, Precio: {Precio}, Usuario: {UserId}",
                    request.ProductoEmpresaId, request.ListaPrecioId, request.Precio, currentUser.Id);

                // Validar que el producto existe
                var producto = await _productRepository.GetByIdAsync(request.ProductoEmpresaId);
                if (producto == null)
                {
                    return new UpsertProductoEmpresaPrecioCommandResult
                    {
                        Success = false,
                        Message = $"Producto empresa {request.ProductoEmpresaId} no encontrado"
                    };
                }

                // Validar que la lista de precios existe
                var listaPrecioInfo = await _listaPrecioRepository.GetCodigoAndNombreByIdAsync(request.ListaPrecioId);
                if (listaPrecioInfo == null)
                {
                    return new UpsertProductoEmpresaPrecioCommandResult
                    {
                        Success = false,
                        Message = $"Lista de precios {request.ListaPrecioId} no encontrada"
                    };
                }

                // Validar precio
                if (request.Precio < 0)
                {
                    return new UpsertProductoEmpresaPrecioCommandResult
                    {
                        Success = false,
                        Message = "El precio no puede ser negativo"
                    };
                }

                // Upsert precio
                var success = await _precioRepository.UpsertPrecioAsync(
                    request.ProductoEmpresaId, 
                    request.ListaPrecioId, 
                    request.Precio);

                if (success)
                {
                    return new UpsertProductoEmpresaPrecioCommandResult
                    {
                        Success = true,
                        Message = "Precio actualizado exitosamente"
                    };
                }
                else
                {
                    return new UpsertProductoEmpresaPrecioCommandResult
                    {
                        Success = false,
                        Message = "Error al actualizar precio"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en upsert precio empresa");
                return new UpsertProductoEmpresaPrecioCommandResult
                {
                    Success = false,
                    Message = "Error interno del servidor"
                };
            }
        }
    }
}