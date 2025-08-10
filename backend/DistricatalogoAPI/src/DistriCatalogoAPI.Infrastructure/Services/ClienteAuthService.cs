using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using BCrypt.Net;

namespace DistriCatalogoAPI.Infrastructure.Services
{
    public class ClienteAuthService : IClienteAuthService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IConfiguration _configuration;
        private readonly string _jwtSecret;
        private readonly int _jwtExpirationMinutes;
        private readonly int _refreshTokenExpirationDays;

        public ClienteAuthService(
            IClienteRepository clienteRepository, 
            IConfiguration configuration)
        {
            _clienteRepository = clienteRepository;
            _configuration = configuration;
            _jwtSecret = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
            _jwtExpirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");
            _refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "30");
        }

        public async Task<(bool Success, Cliente? Cliente, string? Token, string? RefreshToken)> AuthenticateAsync(
            int empresaId, 
            string username, 
            string password)
        {
            try
            {
                // Buscar cliente activo
                var cliente = await _clienteRepository.GetActiveByUsernameAsync(empresaId, username);
                if (cliente == null || !cliente.CanAuthenticate())
                {
                    // Registrar intento fallido
                    await RegisterLoginAttemptAsync(null, false, "Usuario no encontrado o inactivo");
                    return (false, null, null, null);
                }

                // Verificar contraseña
                if (!VerifyPasswordAsync(password, cliente.PasswordHash!).Result)
                {
                    // Registrar intento fallido
                    await RegisterLoginAttemptAsync(cliente.Id, false, "Contraseña incorrecta");
                    return (false, null, null, null);
                }

                // Generar tokens
                var accessToken = await GenerateAccessTokenAsync(cliente);
                var refreshToken = await GenerateRefreshTokenAsync(cliente);

                // Actualizar último login
                await _clienteRepository.UpdateLastLoginAsync(cliente.Id);

                // Registrar login exitoso
                await RegisterLoginAttemptAsync(cliente.Id, true, "Login exitoso");

                return (true, cliente, accessToken, refreshToken);
            }
            catch (Exception ex)
            {
                // Log error (en un escenario real usarías ILogger)
                await RegisterLoginAttemptAsync(null, false, $"Error interno: {ex.Message}");
                return (false, null, null, null);
            }
        }

        public async Task<bool> VerifyPasswordAsync(string password, string passwordHash)
        {
            return await Task.FromResult(BCrypt.Net.BCrypt.Verify(password, passwordHash));
        }

        public async Task<string> HashPasswordAsync(string password)
        {
            return await Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt()));
        }

        public async Task<bool> ChangePasswordAsync(int clienteId, string currentPassword, string newPassword)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null || string.IsNullOrEmpty(cliente.PasswordHash))
                return false;

            // Verificar contraseña actual
            if (!await VerifyPasswordAsync(currentPassword, cliente.PasswordHash))
                return false;

            // Actualizar con nueva contraseña
            var newHash = await HashPasswordAsync(newPassword);
            cliente.SetPassword(newHash);
            
            await _clienteRepository.UpdateAsync(cliente);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int clienteId, string newPassword)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                return false;

            var newHash = await HashPasswordAsync(newPassword);
            cliente.SetPassword(newHash);
            
            await _clienteRepository.UpdateAsync(cliente);
            return true;
        }

        public async Task<string> GenerateAccessTokenAsync(Cliente cliente)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, cliente.Id.ToString()),
                new Claim(ClaimTypes.Name, cliente.Username ?? string.Empty),
                new Claim("codigo", cliente.Codigo),
                new Claim("empresa_id", cliente.EmpresaId.ToString()),
                new Claim("lista_precio_id", cliente.ListaPrecioId?.ToString() ?? string.Empty),
                new Claim("tipo", "cliente") // Para distinguir de usuarios backoffice
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return await Task.FromResult(tokenHandler.WriteToken(token));
        }

        public async Task<string> GenerateRefreshTokenAsync(Cliente cliente)
        {
            // Generar token único
            var tokenValue = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
            
            var now = DateTime.UtcNow;
            var refreshToken = new ClienteRefreshToken
            {
                ClienteId = cliente.Id,
                Token = tokenValue,
                ExpiresAt = now.AddDays(_refreshTokenExpirationDays),
                CreatedAt = now
            };

            await _clienteRepository.AddRefreshTokenAsync(refreshToken);
            return tokenValue;
        }

        public async Task<(bool Success, string? NewAccessToken)> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _clienteRepository.GetRefreshTokenAsync(refreshToken);
            
            if (tokenEntity == null)
            {
                // Log: Token not found
                return (false, null);
            }
                
            if (tokenEntity == null || !tokenEntity.IsValid())
                return (false, null);

            var cliente = tokenEntity.Cliente;
            if (cliente == null || !cliente.CanAuthenticate())
                return (false, null);

            // Generar nuevo access token
            var newAccessToken = await GenerateAccessTokenAsync(cliente);

            // Actualizar último login
            await _clienteRepository.UpdateLastLoginAsync(cliente.Id);

            return (true, newAccessToken);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _clienteRepository.GetRefreshTokenAsync(refreshToken);
            if (tokenEntity == null)
                return false;

            await _clienteRepository.DeleteRefreshTokenAsync(tokenEntity);
            return true;
        }

        public async Task<bool> ValidateAccessTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSecret);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<Cliente?> GetClienteFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);

                var clienteIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (clienteIdClaim == null || !int.TryParse(clienteIdClaim.Value, out int clienteId))
                    return null;

                return await _clienteRepository.GetByIdAsync(clienteId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            return await RevokeRefreshTokenAsync(refreshToken);
        }

        public async Task CleanupExpiredTokensAsync()
        {
            await _clienteRepository.DeleteExpiredRefreshTokensAsync();
        }

        private async Task RegisterLoginAttemptAsync(int? clienteId, bool success, string message)
        {
            if (!clienteId.HasValue) return;

            var history = new ClienteLoginHistory
            {
                ClienteId = clienteId.Value,
                LoginAt = DateTime.UtcNow,
                Success = success,
                IpAddress = null, // Se podría obtener del HttpContext si estuviera disponible
                UserAgent = null
            };

            await _clienteRepository.AddLoginHistoryAsync(history);
        }
    }
}