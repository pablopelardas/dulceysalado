using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ListasPrecios;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ListasPrecios
{
    public class UpdateListaPrecioCommandHandler : IRequestHandler<UpdateListaPrecioCommand, UpdateListaPrecioCommandResult>
    {
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ILogger<UpdateListaPrecioCommandHandler> _logger;

        public UpdateListaPrecioCommandHandler(
            IListaPrecioRepository listaPrecioRepository,
            ILogger<UpdateListaPrecioCommandHandler> logger)
        {
            _listaPrecioRepository = listaPrecioRepository;
            _logger = logger;
        }

        public async Task<UpdateListaPrecioCommandResult> Handle(UpdateListaPrecioCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar que la lista existe
                var lista = await _listaPrecioRepository.GetByIdAsync(request.Id);
                if (lista == null)
                {
                    return new UpdateListaPrecioCommandResult
                    {
                        Success = false,
                        Message = "Lista de precios no encontrada"
                    };
                }

                // Verificar que no sea una lista predeterminada si se intenta desactivar
                if (request.Activa == false)
                {
                    var isDefault = await _listaPrecioRepository.IsDefaultListAsync(request.Id);
                    if (isDefault)
                    {
                        return new UpdateListaPrecioCommandResult
                        {
                            Success = false,
                            Message = "No se puede desactivar la lista de precios predeterminada"
                        };
                    }
                }

                // Validar que el nombre no esté vacío si se está actualizando
                if (!string.IsNullOrEmpty(request.Nombre) && string.IsNullOrWhiteSpace(request.Nombre))
                {
                    return new UpdateListaPrecioCommandResult
                    {
                        Success = false,
                        Message = "El nombre de la lista de precios no puede estar vacío"
                    };
                }

                // Actualizar la lista de precios
                var updated = await _listaPrecioRepository.UpdateAsync(
                    request.Id,
                    request.Codigo,
                    request.Nombre,
                    request.Descripcion,
                    request.Activa,
                    request.Orden);

                if (!updated)
                {
                    return new UpdateListaPrecioCommandResult
                    {
                        Success = false,
                        Message = "No se pudo actualizar la lista de precios"
                    };
                }

                _logger.LogInformation("Lista de precios {ListaId} actualizada exitosamente por empresa {EmpresaId}", 
                    request.Id, request.EmpresaId);

                return new UpdateListaPrecioCommandResult
                {
                    Success = true,
                    Message = "Lista de precios actualizada exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar lista de precios {ListaId}", request.Id);
                return new UpdateListaPrecioCommandResult
                {
                    Success = false,
                    Message = "Error interno al actualizar la lista de precios"
                };
            }
        }
    }
}