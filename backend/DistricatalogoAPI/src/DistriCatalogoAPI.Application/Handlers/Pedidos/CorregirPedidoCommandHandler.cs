using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DistriCatalogoAPI.Application.Commands.Pedidos;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Application.Configuration;
using System.Text.Json;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class CorregirPedidoCommandHandler : IRequestHandler<CorregirPedidoCommand, CorregirPedidoResult>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ICorreccionTokenRepository _tokenRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CorregirPedidoCommandHandler> _logger;
        private readonly ApplicationOptions _applicationOptions;

        public CorregirPedidoCommandHandler(
            IPedidoRepository pedidoRepository,
            ICorreccionTokenRepository tokenRepository,
            INotificationService notificationService,
            ILogger<CorregirPedidoCommandHandler> logger,
            IOptions<ApplicationOptions> applicationOptions)
        {
            _pedidoRepository = pedidoRepository;
            _tokenRepository = tokenRepository;
            _notificationService = notificationService;
            _logger = logger;
            _applicationOptions = applicationOptions.Value;
        }

        public async Task<CorregirPedidoResult> Handle(CorregirPedidoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var pedido = await _pedidoRepository.GetByIdWithItemsAsync(request.PedidoId);
                if (pedido == null)
                {
                    _logger.LogWarning("Pedido {PedidoId} no encontrado", request.PedidoId);
                    return new CorregirPedidoResult { Success = false };
                }

                // Crear snapshot del pedido original antes de las correcciones
                var pedidoOriginal = new PedidoOriginalData
                {
                    MontoTotal = pedido.MontoTotal,
                    Items = pedido.Items.Select(i => new ItemOriginalData
                    {
                        CodigoProducto = i.CodigoProducto,
                        NombreProducto = i.NombreProducto,
                        Cantidad = i.Cantidad,
                        PrecioUnitario = i.PrecioUnitario,
                        Subtotal = i.Subtotal,
                        Observaciones = i.Observaciones
                    }).ToList()
                };
                
                var pedidoOriginalJson = JsonSerializer.Serialize(pedidoOriginal);

                // Iniciar corrección
                pedido.IniciarCorreccion(request.UsuarioId);

                // Aplicar correcciones a los items
                foreach (var itemCorreccion in request.ItemsCorregidos)
                {
                    pedido.ModificarItem(itemCorreccion.CodigoProducto, itemCorreccion.NuevaCantidad, itemCorreccion.Motivo);
                }

                string? tokenGenerado = null;
                
                // Si se debe enviar al cliente, cambiar estado y crear token
                if (request.EnviarAlCliente)
                {
                    pedido.EnviarCorreccionAlCliente(request.UsuarioId);
                    
                    // Crear token de corrección con el snapshot del pedido original
                    var token = CorreccionToken.Crear(pedido.Id, pedidoOriginalJson, request.MotivoCorreccion);
                    await _tokenRepository.AddAsync(token);
                    tokenGenerado = token.Token;
                    
                    _logger.LogInformation("Token de corrección creado: {Token} para pedido {PedidoId}", 
                        token.Token, pedido.Id);
                        
                    // Enviar notificación por email
                    try
                    {
                        await _notificationService.NotificarCorreccionPedidoAsync(pedido, token.Token, request.MotivoCorreccion);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error enviando notificación de corrección para pedido {PedidoId}", pedido.Id);
                        // No fallar el proceso por error de email
                    }
                }

                await _pedidoRepository.UpdateAsync(pedido);

                _logger.LogInformation("Pedido {PedidoId} corregido por usuario {UsuarioId}", 
                    request.PedidoId, request.UsuarioId);

                var correctionUrl = tokenGenerado != null ? 
                    $"{_applicationOptions.CatalogUrl}/correccion/{tokenGenerado}" : 
                    null;

                return new CorregirPedidoResult 
                { 
                    Success = true, 
                    Token = tokenGenerado,
                    CorrectionUrl = correctionUrl,
                    EnviadoAlCliente = request.EnviarAlCliente
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error corrigiendo pedido {PedidoId}", request.PedidoId);
                return new CorregirPedidoResult { Success = false };
            }
        }
    }
}