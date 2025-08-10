using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DistriCatalogoAPI.Application.Queries.Clientes;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Clientes
{
    public class GetClienteByUsernameQueryHandler : IRequestHandler<GetClienteByUsernameQuery, ClienteDto?>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetClienteByUsernameQueryHandler> _logger;

        public GetClienteByUsernameQueryHandler(
            IClienteRepository clienteRepository,
            IMapper mapper,
            ILogger<GetClienteByUsernameQueryHandler> logger)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ClienteDto?> Handle(GetClienteByUsernameQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting cliente by username '{Username}' for empresa {EmpresaId}", 
                request.Username, request.EmpresaId);

            try
            {
                var cliente = await _clienteRepository.GetByUsernameAsync(request.EmpresaId, request.Username);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente with username '{Username}' not found for empresa {EmpresaId}", 
                        request.Username, request.EmpresaId);
                    return null;
                }

                // Verificar si está eliminado y si se debe incluir
                if (!cliente.Activo && !request.IncludeDeleted)
                {
                    _logger.LogWarning("Cliente with username '{Username}' is deleted and IncludeDeleted is false", 
                        request.Username);
                    return null;
                }

                var clienteDto = _mapper.Map<ClienteDto>(cliente);

                _logger.LogInformation("Retrieved cliente {ClienteId} - {Codigo} by username '{Username}' for empresa {EmpresaId}", 
                    cliente.Id, cliente.Codigo, request.Username, request.EmpresaId);

                return clienteDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cliente by username '{Username}' for empresa {EmpresaId}", 
                    request.Username, request.EmpresaId);
                throw;
            }
        }
    }
}