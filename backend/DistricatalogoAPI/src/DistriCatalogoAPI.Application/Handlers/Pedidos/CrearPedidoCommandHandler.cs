using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Pedidos;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class CrearPedidoCommandHandler : IRequestHandler<CrearPedidoCommand, PedidoDto>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CrearPedidoCommandHandler> _logger;

        public CrearPedidoCommandHandler(
            IPedidoRepository pedidoRepository,
            IClienteRepository clienteRepository,
            IMapper mapper,
            ILogger<CrearPedidoCommandHandler> logger)
        {
            _pedidoRepository = pedidoRepository;
            _clienteRepository = clienteRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PedidoDto> Handle(CrearPedidoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validar que el cliente existe y está activo
                var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
                if (cliente == null || !cliente.IsActive || cliente.EmpresaId != request.EmpresaId)
                {
                    throw new InvalidOperationException("Cliente no encontrado o inactivo");
                }

                // Validar que hay items en el pedido
                if (!request.Items.Any())
                {
                    throw new ArgumentException("El pedido debe tener al menos un item");
                }

                // Crear el pedido
                var pedido = new Pedido
                {
                    ClienteId = request.ClienteId,
                    EmpresaId = request.EmpresaId,
                    Numero = await _pedidoRepository.GenerateUniqueNumeroAsync(),
                    FechaPedido = DateTime.UtcNow,
                    FechaEntrega = request.FechaEntrega,
                    HorarioEntrega = request.HorarioEntrega,
                    DireccionEntrega = request.DireccionEntrega,
                    Observaciones = request.Observaciones,
                    Estado = PedidoEstado.Pendiente,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };

                // Crear los items del pedido
                foreach (var itemDto in request.Items)
                {
                    if (string.IsNullOrWhiteSpace(itemDto.CodigoProducto))
                    {
                        throw new ArgumentException("Código de producto es requerido");
                    }

                    if (itemDto.Cantidad <= 0)
                    {
                        throw new ArgumentException("La cantidad debe ser mayor a cero");
                    }

                    if (itemDto.PrecioUnitario < 0)
                    {
                        throw new ArgumentException("El precio unitario no puede ser negativo");
                    }

                    var item = new PedidoItem
                    {
                        CodigoProducto = itemDto.CodigoProducto,
                        NombreProducto = itemDto.NombreProducto ?? itemDto.CodigoProducto,
                        Cantidad = itemDto.Cantidad,
                        PrecioUnitario = itemDto.PrecioUnitario,
                        Observaciones = itemDto.Observaciones,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    pedido.Items.Add(item);
                }

                // Calcular el monto total
                pedido.CalcularMontoTotal();

                // Guardar el pedido
                var pedidoCreado = await _pedidoRepository.AddAsync(pedido);

                _logger.LogInformation("Pedido {PedidoNumero} creado exitosamente para cliente {ClienteId}", 
                    pedidoCreado.Numero, request.ClienteId);

                // Obtener el pedido completo con navegación
                var pedidoCompleto = await _pedidoRepository.GetByIdAsync(pedidoCreado.Id, true);
                
                return _mapper.Map<PedidoDto>(pedidoCompleto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando pedido para cliente {ClienteId}", request.ClienteId);
                throw;
            }
        }
    }
}