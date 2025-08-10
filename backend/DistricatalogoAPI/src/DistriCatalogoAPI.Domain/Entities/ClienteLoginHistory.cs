using System;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class ClienteLoginHistory
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public DateTime LoginAt { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool Success { get; set; } = true;
        
        // Navegación
        public virtual Cliente? Cliente { get; set; }
    }
}