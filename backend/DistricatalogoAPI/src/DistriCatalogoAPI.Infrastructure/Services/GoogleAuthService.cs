using System.Text;
using System.Text.Json;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace DistriCatalogoAPI.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleAuthService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IClienteRepository _clienteRepository;
        private readonly IClienteAuthService _clienteAuthService;
        private readonly DistricatalogoContext _context;
        
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;

        public GoogleAuthService(
            IConfiguration configuration,
            ILogger<GoogleAuthService> logger,
            HttpClient httpClient,
            IClienteRepository clienteRepository,
            IClienteAuthService clienteAuthService,
            DistricatalogoContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _clienteRepository = clienteRepository;
            _clienteAuthService = clienteAuthService;
            _context = context;
            
            _clientId = configuration["GoogleAuth:ClientId"] ?? throw new ArgumentNullException("GoogleAuth:ClientId");
            _clientSecret = configuration["GoogleAuth:ClientSecret"] ?? throw new ArgumentNullException("GoogleAuth:ClientSecret");
            _redirectUri = configuration["GoogleAuth:RedirectUri"] ?? throw new ArgumentNullException("GoogleAuth:RedirectUri");
        }

        public string GenerateAuthorizationUrl(string state)
        {
            var authUrl = "https://accounts.google.com/o/oauth2/v2/auth";
            var scope = Uri.EscapeDataString("openid email profile");
            
            return $"{authUrl}?" +
                   $"client_id={_clientId}&" +
                   $"redirect_uri={Uri.EscapeDataString(_redirectUri)}&" +
                   $"response_type=code&" +
                   $"scope={scope}&" +
                   $"state={state}&" +
                   $"access_type=offline&" +
                   $"prompt=consent";
        }

        public async Task<GoogleTokenResponse?> ExchangeCodeForTokensAsync(string code)
        {
            try
            {
                var tokenUrl = "https://oauth2.googleapis.com/token";
                
                _logger.LogInformation("Intercambiando código por tokens. ClientId: {ClientId}, RedirectUri: {RedirectUri}", 
                    _clientId, _redirectUri);
                
                var requestData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret),
                    new KeyValuePair<string, string>("redirect_uri", _redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                });

                var response = await _httpClient.PostAsync(tokenUrl, requestData);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error intercambiando código por tokens. Status: {StatusCode}, Content: {ErrorContent}", 
                        response.StatusCode, errorContent);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Tokens obtenidos exitosamente. Response: {Response}", content);
                
                var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ExchangeCodeForTokensAsync");
                return null;
            }
        }

        public async Task<GoogleUserInfo?> GetUserInfoAsync(string accessToken)
        {
            try
            {
                var userInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
                
                _logger.LogInformation("Obteniendo información del usuario. AccessToken length: {TokenLength}", accessToken?.Length ?? 0);
                
                var request = new HttpRequestMessage(HttpMethod.Get, userInfoUrl);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error obteniendo información del usuario. Status: {StatusCode}, Content: {ErrorContent}", 
                        response.StatusCode, errorContent);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Información del usuario obtenida exitosamente");
                
                var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetUserInfoAsync");
                return null;
            }
        }

        public async Task<GooglePayload?> ValidateIdTokenAsync(string idToken)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _clientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                
                return new GooglePayload
                {
                    Subject = payload.Subject,
                    Email = payload.Email,
                    EmailVerified = payload.EmailVerified,
                    Name = payload.Name,
                    GivenName = payload.GivenName,
                    FamilyName = payload.FamilyName,
                    Picture = payload.Picture
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando ID token");
                return null;
            }
        }

        public async Task<(Cliente? cliente, bool esNuevo)> CreateOrUpdateClienteFromGoogleAsync(GoogleUserInfo userInfo, int empresaId)
        {
            try
            {
                var existingCliente = await _clienteRepository.GetByEmailAsync(userInfo.Email, empresaId);
                
                if (existingCliente != null)
                {
                    existingCliente.GoogleId = userInfo.Id;
                    existingCliente.FotoUrl = userInfo.Picture;
                    existingCliente.EmailVerificado = userInfo.VerifiedEmail;
                    existingCliente.ProveedorAuth = "google";
                    existingCliente.UpdateLastLogin();
                    
                    await _clienteRepository.UpdateAsync(existingCliente);
                    return (existingCliente, false); // Cliente existente
                }
                
                var nuevoCliente = new Cliente
                {
                    Codigo = await GenerateUniqueCodeAsync(empresaId),
                    Nombre = userInfo.Name,
                    Email = userInfo.Email,
                    Username = userInfo.Email,
                    EmpresaId = empresaId,
                    ListaPrecioId = await GetDefaultPriceListAsync(empresaId),
                    GoogleId = userInfo.Id,
                    FotoUrl = userInfo.Picture,
                    EmailVerificado = userInfo.VerifiedEmail,
                    ProveedorAuth = "google",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _clienteRepository.AddAsync(nuevoCliente);
                return (nuevoCliente, true); // Cliente nuevo
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando o actualizando cliente desde Google");
                return (null, false);
            }
        }

        private async Task<string> GenerateUniqueCodeAsync(int empresaId)
        {
            var random = new Random();
            string code;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                code = $"G{DateTime.Now:yyMMdd}{random.Next(1000, 9999)}";
                var exists = await _clienteRepository.ExistsByCodeAsync(code, empresaId);
                
                if (!exists) return code;
                
                attempts++;
            } while (attempts < maxAttempts);

            throw new InvalidOperationException("No se pudo generar un código único para el cliente");
        }

        private async Task<int?> GetDefaultPriceListAsync(int empresaId)
        {
            try
            {
                // Buscar la primera lista de precios disponible
                var listaPrecio = await _context.ListasPrecios
                    .Where(lp => lp.Activa)
                    .OrderBy(lp => lp.Id)
                    .FirstOrDefaultAsync();
                
                if (listaPrecio != null)
                {
                    _logger.LogInformation("Asignando lista de precios {ListaPrecioId} a nuevo cliente Google", listaPrecio.Id);
                    return listaPrecio.Id;
                }
                
                _logger.LogWarning("No se encontró ninguna lista de precios activa para empresa {EmpresaId}", empresaId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo obtener lista de precios por defecto para empresa {EmpresaId}", empresaId);
                return null;
            }
        }
    }

}