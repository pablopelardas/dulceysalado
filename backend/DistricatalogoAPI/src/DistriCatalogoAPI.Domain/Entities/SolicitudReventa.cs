using System;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class SolicitudReventa
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int EmpresaId { get; set; }
        
        // Datos de la empresa solicitante
        public string? Cuit { get; set; }
        public string? RazonSocial { get; set; }
        public string? DireccionComercial { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public string? TelefonoComercial { get; set; }
        public string? CategoriaIva { get; set; }
        public string? EmailComercial { get; set; }
        
        // Estado de la solicitud
        public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;
        public string? ComentarioRespuesta { get; set; }
        public DateTime? FechaRespuesta { get; set; }
        public string? RespondidoPor { get; set; }
        
        // Auditoría
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navegación
        public virtual Cliente? Cliente { get; set; }
    }
    
    public enum EstadoSolicitud
    {
        Pendiente = 0,
        Aprobada = 1,
        Rechazada = 2
    }
}