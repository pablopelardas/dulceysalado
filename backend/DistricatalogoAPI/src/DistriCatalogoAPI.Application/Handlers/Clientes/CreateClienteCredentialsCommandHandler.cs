using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Clientes;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Clientes
{
    public class CreateClienteCredentialsCommandHandler : IRequestHandler<CreateClienteCredentialsCommand, bool>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IClienteAuthService _clienteAuthService;
        private readonly ILogger<CreateClienteCredentialsCommandHandler> _logger;

        public CreateClienteCredentialsCommandHandler(
            IClienteRepository clienteRepository,
            IClienteAuthService clienteAuthService,
            ILogger<CreateClienteCredentialsCommandHandler> logger)
        {
            _clienteRepository = clienteRepository;
            _clienteAuthService = clienteAuthService;
            _logger = logger;
        }

        public async Task<bool> Handle(CreateClienteCredentialsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating credentials for cliente {ClienteId} with username {Username}", 
                request.ClienteId, request.Username);

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

                // Verificar que el username no esté en uso por otro cliente de la misma empresa
                var existingCliente = await _clienteRepository.GetByUsernameAsync(request.EmpresaId, request.Username);
                if (existingCliente != null && existingCliente.Id != request.ClienteId)
                {
                    _logger.LogWarning("Username {Username} already exists for empresa {EmpresaId}", 
                        request.Username, request.EmpresaId);
                    throw new InvalidOperationException($"El nombre de usuario '{request.Username}' ya está en uso.");
                }

                // Encriptar contraseña
                var passwordHash = await _clienteAuthService.HashPasswordAsync(request.Password);

                // Actualizar cliente con credenciales
                cliente.Username = request.Username;
                cliente.SetPassword(passwordHash);
                cliente.IsActive = request.IsActive;
                cliente.UpdatedBy = request.CreatedBy;

                // Si es la primera vez que se crean credenciales, activar el cliente
                if (!cliente.IsActive && request.IsActive)
                {
                    cliente.Activate();
                }

                await _clienteRepository.UpdateAsync(cliente);

                _logger.LogInformation("Credentials created successfully for cliente {ClienteId}", request.ClienteId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating credentials for cliente {ClienteId}", request.ClienteId);
                throw;
            }
        }
    }
}