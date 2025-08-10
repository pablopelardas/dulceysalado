using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IClienteAuthService
    {
        // Autenticación
        Task<(bool Success, Cliente? Cliente, string? Token, string? RefreshToken)> AuthenticateAsync(
            int empresaId, 
            string username, 
            string password);
        
        // Gestión de contraseñas
        Task<bool> VerifyPasswordAsync(string password, string passwordHash);
        Task<string> HashPasswordAsync(string password);
        Task<bool> ChangePasswordAsync(int clienteId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(int clienteId, string newPassword);
        
        // Tokens
        Task<string> GenerateAccessTokenAsync(Cliente cliente);
        Task<string> GenerateRefreshTokenAsync(Cliente cliente);
        Task<(bool Success, string? NewAccessToken)> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
        
        // Validación
        Task<bool> ValidateAccessTokenAsync(string token);
        Task<Cliente?> GetClienteFromTokenAsync(string token);
        
        // Gestión de sesiones
        Task<bool> LogoutAsync(string refreshToken);
        Task CleanupExpiredTokensAsync();
    }
}