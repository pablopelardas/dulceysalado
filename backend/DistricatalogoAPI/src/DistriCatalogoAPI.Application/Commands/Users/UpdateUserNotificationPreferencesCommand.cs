using DistriCatalogoAPI.Application.DTOs;
using MediatR;
using System.Text.Json.Serialization;

namespace DistriCatalogoAPI.Application.Commands.Users
{
    public class UpdateUserNotificationPreferencesCommand : IRequest<UserNotificationPreferencesDto>
    {
        public int UserId { get; set; }
        
        [JsonPropertyName("notificacion_nuevos_pedidos")]
        public bool NotificacionNuevosPedidos { get; set; }
        
        [JsonPropertyName("notificacion_correcciones_aprobadas")]
        public bool NotificacionCorreccionesAprobadas { get; set; }
        
        [JsonPropertyName("notificacion_correcciones_rechazadas")]
        public bool NotificacionCorreccionesRechazadas { get; set; }
        
        [JsonPropertyName("notificacion_pedidos_cancelados")]
        public bool NotificacionPedidosCancelados { get; set; }
    }
}