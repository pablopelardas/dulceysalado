using System;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class ClienteRefreshToken
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navegación
        public virtual Cliente? Cliente { get; set; }
        
        // Métodos de dominio
        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpiresAt;
        }
        
        public bool IsValid()
        {
            return !IsExpired() && !string.IsNullOrEmpty(Token);
        }
    }
}