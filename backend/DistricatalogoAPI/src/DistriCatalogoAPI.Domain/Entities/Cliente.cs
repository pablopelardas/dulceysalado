using System;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Telefono { get; set; }
        public string? Cuit { get; set; }
        public string? Altura { get; set; }
        public string? Provincia { get; set; }
        public string? Email { get; set; }
        public string? TipoIva { get; set; }
        
        // Campos de autenticación
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime? LastLogin { get; set; }
        
        // Campos para autenticación social
        public string? GoogleId { get; set; }
        public string? FotoUrl { get; set; }
        public bool EmailVerificado { get; set; } = false;
        public string? ProveedorAuth { get; set; }
        
        // Lista de precios
        public int? ListaPrecioId { get; set; }
        
        // Auditoría
        public bool Activo { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Navegación - Se configurará el mapping en la infraestructura  
        // Nota: La referencia apunta a Infrastructure.Models.ListasPrecio
        // Se configura en ClienteConfiguration
        
        // Métodos de dominio
        public void SetPassword(string passwordHash)
        {
            PasswordHash = passwordHash;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void RecordLogin()
        {
            LastLogin = DateTime.UtcNow;
        }
        
        public void UpdateLastLogin()
        {
            LastLogin = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void SoftDelete()
        {
            Activo = false;
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public bool CanAuthenticate()
        {
            return Activo && IsActive && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(PasswordHash);
        }
    }
}