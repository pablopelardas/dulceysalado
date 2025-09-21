using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DistriCatalogoAPI.Domain.Entities
{
    [Table("user_notification_preferences")]
    public class UserNotificationPreferences
    {
        [Key]
        public int Id { get; set; }

        // Campos de auditoría
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("created_by")]
        [StringLength(255)]
        public string CreatedBy { get; set; } = "SYSTEM";

        [Column("updated_by")]
        [StringLength(255)]
        public string UpdatedBy { get; set; } = "SYSTEM";

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        // Tipos de notificaciones de pedidos
        [Column("notificacion_nuevos_pedidos")]
        public bool NotificacionNuevosPedidos { get; set; } = true;

        [Column("notificacion_correcciones_aprobadas")]
        public bool NotificacionCorreccionesAprobadas { get; set; } = true;

        [Column("notificacion_correcciones_rechazadas")]
        public bool NotificacionCorreccionesRechazadas { get; set; } = true;

        [Column("notificacion_pedidos_cancelados")]
        public bool NotificacionPedidosCancelados { get; set; } = true;

        [Column("notificacion_solicitudes_reventa")]
        public bool NotificacionSolicitudesReventa { get; set; } = true;

        // La navegación se configura en EF Core Configuration, no aquí
        // para evitar dependencias circulares entre Domain e Infrastructure

        // Métodos de utilidad
        public bool DebeRecibirNotificacion(TipoNotificacion tipo)
        {
            return tipo switch
            {
                TipoNotificacion.NuevoPedido => NotificacionNuevosPedidos,
                TipoNotificacion.CorreccionAprobada => NotificacionCorreccionesAprobadas,
                TipoNotificacion.CorreccionRechazada => NotificacionCorreccionesRechazadas,
                TipoNotificacion.PedidoCancelado => NotificacionPedidosCancelados,
                TipoNotificacion.NuevaSolicitudReventa => NotificacionSolicitudesReventa,
                _ => false
            };
        }

        public void ActualizarPreferencia(TipoNotificacion tipo, bool activo)
        {
            switch (tipo)
            {
                case TipoNotificacion.NuevoPedido:
                    NotificacionNuevosPedidos = activo;
                    break;
                case TipoNotificacion.CorreccionAprobada:
                    NotificacionCorreccionesAprobadas = activo;
                    break;
                case TipoNotificacion.CorreccionRechazada:
                    NotificacionCorreccionesRechazadas = activo;
                    break;
                case TipoNotificacion.PedidoCancelado:
                    NotificacionPedidosCancelados = activo;
                    break;
                case TipoNotificacion.NuevaSolicitudReventa:
                    NotificacionSolicitudesReventa = activo;
                    break;
            }
        }
    }

    public enum TipoNotificacion
    {
        NuevoPedido,
        CorreccionAprobada,
        CorreccionRechazada,
        PedidoCancelado,
        NuevaSolicitudReventa
        // Extensible para futuras notificaciones:
        // PedidoCompletado,
        // RecordatorioEntrega,
        // ReporteSemanal
    }
}