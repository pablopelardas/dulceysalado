using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Clientes;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Clientes
{
    public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, ClienteDto>
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateClienteCommandHandler> _logger;

        public UpdateClienteCommandHandler(
            IClienteRepository clienteRepository,
            IMapper mapper,
            ILogger<UpdateClienteCommandHandler> logger)
        {
            _clienteRepository = clienteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ClienteDto> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating cliente with ID {ClienteId} for empresa {EmpresaId}", 
                request.Id, request.EmpresaId);

            try
            {
                // Buscar cliente existente (incluir eliminados para poder reactivarlos)
                var cliente = await _clienteRepository.GetByIdAsync(request.Id, includeDeleted: true);
                if (cliente == null || cliente.EmpresaId != request.EmpresaId)
                {
                    _logger.LogWarning("Cliente with ID {ClienteId} not found for empresa {EmpresaId}", 
                        request.Id, request.EmpresaId);
                    throw new InvalidOperationException($"Cliente con ID '{request.Id}' no encontrado.");
                }

                // Actualizar solo los campos proporcionados
                if (request.Nombre != null)
                    cliente.Nombre = request.Nombre;
                
                if (request.Direccion != null)
                    cliente.Direccion = request.Direccion;
                
                if (request.Localidad != null)
                    cliente.Localidad = request.Localidad;
                
                if (request.Telefono != null)
                    cliente.Telefono = request.Telefono;
                
                if (request.Cuit != null)
                    cliente.Cuit = request.Cuit;
                
                if (request.Altura != null)
                    cliente.Altura = request.Altura;
                
                if (request.Provincia != null)
                    cliente.Provincia = request.Provincia;
                
                if (request.Email != null)
                    cliente.Email = request.Email;
                
                if (request.TipoIva != null)
                    cliente.TipoIva = request.TipoIva;
                
                if (request.ListaPrecioId.HasValue)
                    cliente.ListaPrecioId = request.ListaPrecioId;
                
                if (request.Activo.HasValue)
                    cliente.Activo = request.Activo.Value;

                // Actualizar metadatos de auditoría
                cliente.UpdatedBy = request.UpdatedBy;

                // Guardar cambios
                await _clienteRepository.UpdateAsync(cliente);

                _logger.LogInformation("Cliente with ID {ClienteId} updated successfully", request.Id);

                // Mapear a DTO y retornar
                return _mapper.Map<ClienteDto>(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cliente with ID {ClienteId} for empresa {EmpresaId}", 
                    request.Id, request.EmpresaId);
                throw;
            }
        }
    }
}