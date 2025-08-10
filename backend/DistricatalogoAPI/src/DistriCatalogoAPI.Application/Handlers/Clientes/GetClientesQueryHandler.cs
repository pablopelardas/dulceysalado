using System.Collections.Generic;
using System.Linq;
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
    public class GetClientesQueryHandler : IRequestHandler<GetClientesQuery, PagedResultDto<ClienteDto>>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetClientesQueryHandler> _logger;

        public GetClientesQueryHandler(
            IClienteRepository clienteRepository,
            IListaPrecioRepository listaPrecioRepository,
            IMapper mapper,
            ILogger<GetClientesQueryHandler> logger)
        {
            _clienteRepository = clienteRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResultDto<ClienteDto>> Handle(GetClientesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting clientes for empresa {EmpresaId} - Page: {Page}, PageSize: {PageSize}", 
                request.EmpresaId, request.Page, request.PageSize);

            try
            {
                var (clientes, totalCount) = await _clienteRepository.GetPaginatedAsync(
                    request.EmpresaId,
                    null, // codigo
                    request.SearchTerm, // nombre
                    null, // cuit
                    null, // localidad
                    request.IsActive, // tieneAcceso
                    request.IncludeDeleted, // includeDeleted
                    request.Page,
                    request.PageSize);

                var clienteDtos = _mapper.Map<List<ClienteDto>>(clientes);
                
                // Cargar listas de precios para todos los clientes que las tengan
                var listaPrecioIds = clientes.Where(c => c.ListaPrecioId.HasValue)
                                            .Select(c => c.ListaPrecioId.Value)
                                            .Distinct()
                                            .ToArray();
                                            
                if (listaPrecioIds.Any())
                {
                    var listasPrecios = new Dictionary<int, DistriCatalogoAPI.Domain.Interfaces.ListaPrecioDto>();
                    foreach (var id in listaPrecioIds)
                    {
                        var listaPrecio = await _listaPrecioRepository.GetByIdAsync(id);
                        if (listaPrecio != null)
                        {
                            listasPrecios[id] = listaPrecio;
                        }
                    }
                    
                    // Mapear las listas de precios a los DTOs
                    foreach (var clienteDto in clienteDtos)
                    {
                        var cliente = clientes.First(c => c.Id == clienteDto.Id);
                        if (cliente.ListaPrecioId.HasValue && listasPrecios.TryGetValue(cliente.ListaPrecioId.Value, out var listaPrecioDomain))
                        {
                            clienteDto.ListaPrecio = new Application.DTOs.ListaPrecioDto
                            {
                                Id = listaPrecioDomain.Id,
                                Codigo = listaPrecioDomain.Codigo,
                                Nombre = listaPrecioDomain.Nombre,
                                Descripcion = null,
                                Activo = listaPrecioDomain.Activa
                            };
                        }
                    }
                }

                var result = new PagedResultDto<ClienteDto>
                {
                    Pagination = new PaginationDto
                    {
                        Page = request.Page,
                        Limit = request.PageSize,
                        Total = totalCount
                    }
                };
                
                result.SetItems(clienteDtos, "clientes");

                _logger.LogInformation("Retrieved {Count} clientes out of {Total} for empresa {EmpresaId}", 
                    clienteDtos.Count, totalCount, request.EmpresaId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clientes for empresa {EmpresaId}", request.EmpresaId);
                throw;
            }
        }
    }
}