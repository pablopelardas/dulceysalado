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
    public class GetClienteByIdQueryHandler : IRequestHandler<GetClienteByIdQuery, ClienteDto?>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetClienteByIdQueryHandler> _logger;

        public GetClienteByIdQueryHandler(
            IClienteRepository clienteRepository,
            IListaPrecioRepository listaPrecioRepository,
            IMapper mapper,
            ILogger<GetClienteByIdQueryHandler> logger)
        {
            _clienteRepository = clienteRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ClienteDto?> Handle(GetClienteByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting cliente {ClienteId} for empresa {EmpresaId}", 
                request.ClienteId, request.EmpresaId);

            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, request.IncludeDeleted);

                if (cliente == null || cliente.EmpresaId != request.EmpresaId)
                {
                    _logger.LogWarning("Cliente {ClienteId} not found for empresa {EmpresaId}", 
                        request.ClienteId, request.EmpresaId);
                    return null;
                }

                // Verificar si está eliminado y si se debe incluir
                if (!cliente.Activo && !request.IncludeDeleted)
                {
                    _logger.LogWarning("Cliente {ClienteId} is deleted and IncludeDeleted is false", request.ClienteId);
                    return null;
                }

                var clienteDto = _mapper.Map<ClienteDto>(cliente);
                
                // Cargar lista de precios si existe
                if (cliente.ListaPrecioId.HasValue)
                {
                    var listaPrecioDomain = await _listaPrecioRepository.GetByIdAsync(cliente.ListaPrecioId.Value);
                    if (listaPrecioDomain != null)
                    {
                        // Mapeo manual del DTO del dominio al DTO de aplicación
                        clienteDto.ListaPrecio = new Application.DTOs.ListaPrecioDto
                        {
                            Id = listaPrecioDomain.Id,
                            Codigo = listaPrecioDomain.Codigo,
                            Nombre = listaPrecioDomain.Nombre,
                            Descripcion = null, // El DTO del dominio no tiene descripción
                            Activo = listaPrecioDomain.Activa
                        };
                    }
                }

                _logger.LogInformation("Retrieved cliente {ClienteId} - {Codigo} for empresa {EmpresaId}", 
                    cliente.Id, cliente.Codigo, request.EmpresaId);

                return clienteDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cliente {ClienteId} for empresa {EmpresaId}", 
                    request.ClienteId, request.EmpresaId);
                throw;
            }
        }
    }
}