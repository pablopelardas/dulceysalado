using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Pedidos;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class CrearPedidoCommandHandler : IRequestHandler<CrearPedidoCommand, PedidoDto>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<CrearPedidoCommandHandler> _logger;

        public CrearPedidoCommandHandler(
            IPedidoRepository pedidoRepository,
            IClienteRepository clienteRepository,
            IDeliveryRepository deliveryRepository,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<CrearPedidoCommandHandler> logger)
        {
            _pedidoRepository = pedidoRepository;
            _clienteRepository = clienteRepository;
            _deliveryRepository = deliveryRepository;
            _notificationService = notificationService;
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

                // Reservar slot de delivery si se especificó
                if (!string.IsNullOrWhiteSpace(request.DeliverySlot) && request.FechaEntrega.HasValue)
                {
                    var slotReserved = await ReserveDeliverySlotAsync(request.DeliverySlot, request.FechaEntrega.Value, request.EmpresaId);
                    if (!slotReserved)
                    {
                        throw new InvalidOperationException("La franja horaria seleccionada ya no está disponible. Por favor, selecciona otra franja.");
                    }
                }

                // Guardar el pedido
                var pedidoCreado = await _pedidoRepository.AddAsync(pedido);

                _logger.LogInformation("Pedido {PedidoNumero} creado exitosamente para cliente {ClienteId}", 
                    pedidoCreado.Numero, request.ClienteId);

                // Obtener el pedido completo con navegación
                var pedidoCompleto = await _pedidoRepository.GetByIdAsync(pedidoCreado.Id, true);
                
                // Notificar a usuarios de la empresa sobre el nuevo pedido
                try
                {
                    await _notificationService.NotificarNuevoPedidoAsync(pedidoCompleto);
                }
                catch (Exception notificationEx)
                {
                    _logger.LogError(notificationEx, "Error enviando notificaciones para pedido {PedidoId}", pedidoCompleto.Id);
                    // No lanzamos la excepción para no afectar la creación del pedido
                }
                
                return _mapper.Map<PedidoDto>(pedidoCompleto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando pedido para cliente {ClienteId}", request.ClienteId);
                throw;
            }
        }

        private async Task<bool> ReserveDeliverySlotAsync(string deliverySlot, DateTime fechaEntrega, int empresaId)
        {
            try
            {
                // Parse el delivery slot (formato: "2025-09-01_morning" o "2025-09-01_afternoon")
                var parts = deliverySlot.Split('_');
                if (parts.Length != 2)
                {
                    _logger.LogWarning("Invalid delivery slot format: {DeliverySlot}", deliverySlot);
                    return false;
                }

                var dateStr = parts[0];
                var slotTypeStr = parts[1];

                if (!DateOnly.TryParse(dateStr, out var slotDate))
                {
                    _logger.LogWarning("Invalid date in delivery slot: {DateStr}", dateStr);
                    return false;
                }

                var slotType = slotTypeStr.ToLower() switch
                {
                    "morning" => Domain.Enums.SlotType.Morning,
                    "afternoon" => Domain.Enums.SlotType.Afternoon,
                    _ => throw new ArgumentException($"Invalid slot type: {slotTypeStr}")
                };

                // Reservar el slot
                var reserved = await _deliveryRepository.ReserveSlotAsync(empresaId, slotDate, slotType);
                if (reserved)
                {
                    await _deliveryRepository.SaveChangesAsync();
                    _logger.LogInformation("Delivery slot reserved: {DeliverySlot} for empresa {EmpresaId}", deliverySlot, empresaId);
                }
                else
                {
                    _logger.LogWarning("Failed to reserve delivery slot: {DeliverySlot} for empresa {EmpresaId}", deliverySlot, empresaId);
                }

                return reserved;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving delivery slot: {DeliverySlot} for empresa {EmpresaId}", deliverySlot, empresaId);
                return false;
            }
        }
    }
}