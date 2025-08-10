using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ListasPrecios;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ListasPrecios
{
    public class DeleteListaPrecioCommandHandler : IRequestHandler<DeleteListaPrecioCommand, DeleteListaPrecioCommandResult>
    {
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ILogger<DeleteListaPrecioCommandHandler> _logger;

        public DeleteListaPrecioCommandHandler(
            IListaPrecioRepository listaPrecioRepository,
            ILogger<DeleteListaPrecioCommandHandler> logger)
        {
            _listaPrecioRepository = listaPrecioRepository;
            _logger = logger;
        }

        public async Task<DeleteListaPrecioCommandResult> Handle(DeleteListaPrecioCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar que la lista existe
                var lista = await _listaPrecioRepository.GetByIdAsync(request.Id);
                if (lista == null)
                {
                    return new DeleteListaPrecioCommandResult
                    {
                        Success = false,
                        Message = "Lista de precios no encontrada"
                    };
                }

                // Verificar que no sea una lista predeterminada
                if (lista.EsPredeterminada)
                {
                    return new DeleteListaPrecioCommandResult
                    {
                        Success = false,
                        Message = "No se puede eliminar la lista de precios predeterminada"
                    };
                }

                // Verificar que no tenga precios asociados
                var tienePrecios = await _listaPrecioRepository.TienePreciosAsociadosAsync(request.Id);
                if (tienePrecios)
                {
                    return new DeleteListaPrecioCommandResult
                    {
                        Success = false,
                        Message = "No se puede eliminar la lista porque tiene precios asociados. Elimine los precios primero."
                    };
                }

                // Eliminar la lista
                await _listaPrecioRepository.DeleteAsync(request.Id);

                _logger.LogInformation("Lista de precios {ListaId} eliminada exitosamente por empresa {EmpresaId}", 
                    request.Id, request.EmpresaId);

                return new DeleteListaPrecioCommandResult
                {
                    Success = true,
                    Message = "Lista de precios eliminada exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar lista de precios {ListaId}", request.Id);
                return new DeleteListaPrecioCommandResult
                {
                    Success = false,
                    Message = "Error interno al eliminar la lista de precios"
                };
            }
        }
    }
}