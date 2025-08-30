namespace DistriCatalogoAPI.Application.DTOs
{
    public class UserNotificationPreferencesWithUserDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        // Tipos de notificaciones de pedidos
        public bool NotificacionNuevosPedidos { get; set; }
        public bool NotificacionCorreccionesAprobadas { get; set; }
        public bool NotificacionCorreccionesRechazadas { get; set; }
        public bool NotificacionPedidosCancelados { get; set; }
        
        // Información del usuario necesaria para enviar emails
        public UserInfoDto User { get; set; } = new();
    }

    public class UserInfoDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int EmpresaId { get; set; }
    }
}