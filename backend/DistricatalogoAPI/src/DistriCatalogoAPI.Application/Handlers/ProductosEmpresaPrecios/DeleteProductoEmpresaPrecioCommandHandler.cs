using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosEmpresaPrecios;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosEmpresaPrecios
{
    public class DeleteProductoEmpresaPrecioCommandHandler : IRequestHandler<DeleteProductoEmpresaPrecioCommand, DeleteProductoEmpresaPrecioCommandResult>
    {
        private readonly IProductoEmpresaPrecioRepository _precioRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteProductoEmpresaPrecioCommandHandler> _logger;

        public DeleteProductoEmpresaPrecioCommandHandler(
            IProductoEmpresaPrecioRepository precioRepository,
            ICurrentUserService currentUserService,
            ILogger<DeleteProductoEmpresaPrecioCommandHandler> logger)
        {
            _precioRepository = precioRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<DeleteProductoEmpresaPrecioCommandResult> Handle(DeleteProductoEmpresaPrecioCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = await _currentUserService.GetCurrentUserAsync();

                _logger.LogInformation("Eliminando precio empresa - ProductoId: {ProductoId}, ListaId: {ListaId}, Usuario: {UserId}",
                    request.ProductoEmpresaId, request.ListaPrecioId, currentUser.Id);

                var success = await _precioRepository.DeletePrecioAsync(
                    request.ProductoEmpresaId, 
                    request.ListaPrecioId);

                if (success)
                {
                    return new DeleteProductoEmpresaPrecioCommandResult
                    {
                        Success = true,
                        Message = "Precio eliminado exitosamente"
                    };
                }
                else
                {
                    return new DeleteProductoEmpresaPrecioCommandResult
                    {
                        Success = false,
                        Message = "Precio no encontrado"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando precio empresa");
                return new DeleteProductoEmpresaPrecioCommandResult
                {
                    Success = false,
                    Message = "Error interno del servidor"
                };
            }
        }
    }
}