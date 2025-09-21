using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Interfaces
{
    public interface INotificationService
    {
        Task NotificarCambioEstadoPedidoAsync(Pedido pedido, string estadoAnterior);
        Task NotificarCorreccionPedidoAsync(Pedido pedido, string token, string? motivoCorreccion = null);
        Task<bool> EnviarEmailAsync(string destinatario, string asunto, string mensaje, bool esHtml = false);
        
        // Notificaciones a usuarios de la empresa
        Task NotificarNuevoPedidoAsync(Pedido pedido);
        Task NotificarRespuestaCorreccionAsync(Pedido pedido, bool aprobada, string? comentarioCliente = null);
        Task NotificarCancelacionPedidoAsync(Pedido pedido, string? motivoCancelacion = null);
        
        // Notificaciones de solicitudes de reventa
        Task NotificarNuevaSolicitudReventaAsync(SolicitudReventa solicitud, Cliente cliente);
        Task NotificarRespuestaSolicitudReventaAsync(SolicitudReventa solicitud, Cliente cliente);
    }
}