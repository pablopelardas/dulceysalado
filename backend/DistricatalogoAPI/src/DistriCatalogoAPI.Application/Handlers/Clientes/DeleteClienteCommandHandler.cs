using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Clientes;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Clientes
{
    public class DeleteClienteCommandHandler : IRequestHandler<DeleteClienteCommand, bool>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ILogger<DeleteClienteCommandHandler> _logger;

        public DeleteClienteCommandHandler(
            IClienteRepository clienteRepository,
            ILogger<DeleteClienteCommandHandler> logger)
        {
            _clienteRepository = clienteRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteClienteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting cliente with ID {ClienteId} for empresa {EmpresaId}", 
                request.Id, request.EmpresaId);

            try
            {
                // Buscar cliente existente
                var cliente = await _clienteRepository.GetByIdAsync(request.Id);
                if (cliente == null || cliente.EmpresaId != request.EmpresaId)
                {
                    _logger.LogWarning("Cliente with ID {ClienteId} not found for empresa {EmpresaId}", 
                        request.Id, request.EmpresaId);
                    return false;
                }

                // Verificar si ya está eliminado
                if (!cliente.Activo)
                {
                    _logger.LogInformation("Cliente with ID {ClienteId} is already deleted", request.Id);
                    return true;
                }

                // Actualizar metadatos de auditoría antes del soft delete
                cliente.UpdatedBy = request.UpdatedBy;

                // Realizar soft delete
                await _clienteRepository.DeleteAsync(cliente);

                _logger.LogInformation("Cliente with ID {ClienteId} deleted successfully (soft delete)", request.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cliente with ID {ClienteId} for empresa {EmpresaId}", 
                    request.Id, request.EmpresaId);
                throw;
            }
        }
    }
}