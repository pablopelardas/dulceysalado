using System;

namespace DistriCatalogoAPI.Application.DTOs
{
    public class ClienteDto
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
        public bool TieneAcceso { get; set; }
        public string? Username { get; set; }
        public ListaPrecioDto? ListaPrecio { get; set; }
        public bool Activo { get; set; }
        public DateTime? LastLogin { get; set; }
    }
    
    public class CreateClienteDto
    {
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
        public int? ListaPrecioId { get; set; }
    }
    
    public class UpdateClienteDto
    {
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Telefono { get; set; }
        public string? Cuit { get; set; }
        public string? Altura { get; set; }
        public string? Provincia { get; set; }
        public string? Email { get; set; }
        public string? TipoIva { get; set; }
        public int? ListaPrecioId { get; set; }
        public bool? Activo { get; set; }
    }
    
    public class CreateClienteCredentialsDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
    
    public class UpdateClientePasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
    
    public class ClienteLoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int? EmpresaId { get; set; }
    }
    
    public class ClienteTokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public ClienteAuthDto Cliente { get; set; } = new();
        public int ExpiresIn { get; set; }
    }
    
    public class ClienteAuthDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public int? ListaPrecioId { get; set; }
    }
    
    public class ClienteRefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
    
    public class ClienteChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}