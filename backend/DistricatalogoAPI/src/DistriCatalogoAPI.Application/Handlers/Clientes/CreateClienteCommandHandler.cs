using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Clientes;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Clientes
{
    public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, ClienteDto>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateClienteCommandHandler> _logger;

        public CreateClienteCommandHandler(
            IClienteRepository clienteRepository,
            IMapper mapper,
            ILogger<CreateClienteCommandHandler> logger)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ClienteDto> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating cliente with codigo {Codigo} for empresa {EmpresaId}", 
                request.Codigo, request.EmpresaId);

            try
            {
                // Verificar si ya existe un cliente con el mismo código
                var existingCliente = await _clienteRepository.GetByCodigoAsync(request.EmpresaId, request.Codigo);
                if (existingCliente != null)
                {
                    _logger.LogWarning("Cliente with codigo {Codigo} already exists for empresa {EmpresaId}", 
                        request.Codigo, request.EmpresaId);
                    throw new InvalidOperationException($"Ya existe un cliente con el código '{request.Codigo}' en esta empresa.");
                }

                // Crear entidad cliente
                var cliente = new Cliente
                {
                    EmpresaId = request.EmpresaId,
                    Codigo = request.Codigo,
                    Nombre = request.Nombre,
                    Direccion = request.Direccion,
                    Localidad = request.Localidad,
                    Telefono = request.Telefono,
                    Cuit = request.Cuit,
                    Altura = request.Altura,
                    Provincia = request.Provincia,
                    Email = request.Email,
                    TipoIva = request.TipoIva,
                    ListaPrecioId = request.ListaPrecioId,
                    CreatedBy = request.CreatedBy,
                    Activo = true
                };

                // Guardar en la base de datos
                var savedCliente = await _clienteRepository.AddAsync(cliente);

                _logger.LogInformation("Cliente created successfully with ID {ClienteId}", savedCliente.Id);

                // Mapear a DTO y retornar
                return _mapper.Map<ClienteDto>(savedCliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cliente with codigo {Codigo} for empresa {EmpresaId}", 
                    request.Codigo, request.EmpresaId);
                throw;
            }
        }
    }
}