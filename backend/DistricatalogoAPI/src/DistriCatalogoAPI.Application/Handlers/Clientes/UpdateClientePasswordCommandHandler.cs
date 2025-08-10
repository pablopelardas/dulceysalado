using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Clientes;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Clientes
{
    public class UpdateClientePasswordCommandHandler : IRequestHandler<UpdateClientePasswordCommand, bool>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IClienteAuthService _clienteAuthService;
        private readonly ILogger<UpdateClientePasswordCommandHandler> _logger;

        public UpdateClientePasswordCommandHandler(
            IClienteRepository clienteRepository,
            IClienteAuthService clienteAuthService,
            ILogger<UpdateClientePasswordCommandHandler> logger)
        {
            _clienteRepository = clienteRepository;
            _clienteAuthService = clienteAuthService;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateClientePasswordCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating password for cliente {ClienteId}", request.ClienteId);

            try
            {
                // Verificar que el cliente existe y pertenece a la empresa
                var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
                if (cliente == null || cliente.EmpresaId != request.EmpresaId)
                {
                    _logger.LogWarning("Cliente with ID {ClienteId} not found for empresa {EmpresaId}", 
                        request.ClienteId, request.EmpresaId);
                    throw new InvalidOperationException($"Cliente con ID '{request.ClienteId}' no encontrado.");
                }

                // Verificar que el cliente tiene credenciales configuradas
                if (string.IsNullOrEmpty(cliente.Username))
                {
                    _logger.LogWarning("Cliente {ClienteId} does not have credentials configured", request.ClienteId);
                    throw new InvalidOperationException("El cliente no tiene credenciales configuradas. Configure primero el nombre de usuario.");
                }

                // Usar el servicio de autenticación para resetear la contraseña
                var success = await _clienteAuthService.ResetPasswordAsync(request.ClienteId, request.NewPassword);
                
                if (!success)
                {
                    _logger.LogError("Failed to reset password for cliente {ClienteId}", request.ClienteId);
                    throw new InvalidOperationException("Error al actualizar la contraseña.");
                }

                // Actualizar metadatos de auditoría
                cliente.UpdatedBy = request.UpdatedBy;
                await _clienteRepository.UpdateAsync(cliente);

                _logger.LogInformation("Password updated successfully for cliente {ClienteId}", request.ClienteId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password for cliente {ClienteId}", request.ClienteId);
                throw;
            }
        }
    }
}