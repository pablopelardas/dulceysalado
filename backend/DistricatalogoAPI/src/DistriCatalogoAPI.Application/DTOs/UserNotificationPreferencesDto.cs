namespace DistriCatalogoAPI.Application.DTOs
{
    public class UserNotificationPreferencesDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        // Tipos de notificaciones de pedidos
        public bool NotificacionNuevosPedidos { get; set; }
        public bool NotificacionCorreccionesAprobadas { get; set; }
        public bool NotificacionCorreccionesRechazadas { get; set; }
        public bool NotificacionPedidosCancelados { get; set; }
        
        // Campos de auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}