using Microsoft.Extensions.Logging;
using ProcesadorGecomCsv.Config;
using ProcesadorGecomCsv.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ProcesadorGecomCsv.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfig _apiConfig;
    private readonly ILogger<ApiService> _logger;
    private string? _jwtToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public ApiService(HttpClient httpClient, ApiConfig apiConfig, ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _apiConfig = apiConfig;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_apiConfig.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(_apiConfig.TimeoutMinutes);
    }

    public async Task<bool> AuthenticateAsync()
    {
        try
        {
            if (IsTokenValid())
            {
                _logger.LogDebug("Token JWT válido, reutilizando autenticación");
                return true;
            }

            var loginRequest = new
            {
                email = _apiConfig.Username,
                password = _apiConfig.Password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Autenticando con API...");
            var response = await _httpClient.PostAsync("/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error en autenticación: {StatusCode} - {Content}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);

            if (authResponse?.accessToken == null)
            {
                _logger.LogError("Respuesta de autenticación inválida");
                return false;
            }

            _jwtToken = authResponse.accessToken;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(_apiConfig.TimeoutMinutes - 1);
            
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _jwtToken);

            _logger.LogInformation("Autenticación exitosa");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante autenticación");
            return false;
        }
    }

    public async Task<ApiResponse<SyncSession>> StartSyncSessionAsync(string fileName)
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                return new ApiResponse<SyncSession>
                {
                    Success = false,
                    ErrorMessage = "Error de autenticación"
                };
            }

            var sessionRequest = new
            {
                total_lotes_esperados = 1, // Se actualizará cuando sepamos el total real
                usuario_proceso = "SISTEMA_GECOM"
            };

            var json = JsonSerializer.Serialize(sessionRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Iniciando sesión de sincronización para archivo: {FileName}", fileName);

            var response = await _httpClient.PostAsync("/api/admin/sync/session/start", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error iniciando sesión: {StatusCode} - {Content}", 
                    response.StatusCode, responseContent);

                return new ApiResponse<SyncSession>
                {
                    Success = false,
                    ErrorMessage = $"HTTP {response.StatusCode}: {responseContent}"
                };
            }

            using var doc = JsonDocument.Parse(responseContent);
            var session = new SyncSession 
            { 
                SessionId = doc.RootElement.GetProperty("session_id").GetString() ?? string.Empty,
                FechaInicio = DateTime.UtcNow,
                ArchivoNombre = fileName
            };
            
            _logger.LogInformation("Sesión iniciada exitosamente: ID {SessionId}", session.SessionId);

            return new ApiResponse<SyncSession>
            {
                Success = true,
                Data = session
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error iniciando sesión de sincronización");
            return new ApiResponse<SyncSession>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ApiResponse<BatchProcessResult>> ProcessBatchAsync(List<GecomRecord> batch, int batchNumber, string sessionId)
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                return new ApiResponse<BatchProcessResult>
                {
                    Success = false,
                    ErrorMessage = "Error de autenticación"
                };
            }

            var batchRequest = new
            {
                session_id = sessionId,
                lote_numero = batchNumber,
                productos = batch.Select(p => new
                {
                    codigo = p.Codigo.ToString(),
                    nombre = p.Descripcion,
                    descripcion = p.Descripcion,
                    categoria_id = p.CodigoRubro,
                    precio = p.Precio,
                    stock = (int)p.Existencia,
                    activo = p.Disponible == "S",
                    visible_catalogo = p.Imputable == "S"
                }).ToArray()
            };

            var json = JsonSerializer.Serialize(batchRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Enviando lote {BatchNumber} con {Count} productos (Sesión: {SessionId})", 
                batchNumber, batch.Count, sessionId);

            var response = await _httpClient.PostAsync("/api/admin/sync/productos/bulk", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error procesando lote {BatchNumber}: {StatusCode} - {Content}", 
                    batchNumber, response.StatusCode, responseContent);

                return new ApiResponse<BatchProcessResult>
                {
                    Success = false,
                    ErrorMessage = $"HTTP {response.StatusCode}: {responseContent}"
                };
            }

            using var resultDoc = JsonDocument.Parse(responseContent);
            var estadisticas = resultDoc.RootElement.GetProperty("estadisticas");
            var result = new BatchProcessResult
            {
                ProductosNuevos = estadisticas.GetProperty("productos_nuevos").GetInt32(),
                ProductosActualizados = estadisticas.GetProperty("productos_actualizados").GetInt32(),
                Errores = estadisticas.GetProperty("errores").GetInt32()
            };

            // Capturar información de categorías faltantes
            if (resultDoc.RootElement.TryGetProperty("categorias_info", out var categoriasInfo))
            {
                result.CategoriasNoEncontradas = new List<int>();
                if (categoriasInfo.TryGetProperty("categorias_asignadas_null", out var categoriasNull))
                {
                    foreach (var categoria in categoriasNull.EnumerateArray())
                    {
                        result.CategoriasNoEncontradas.Add(categoria.GetInt32());
                    }
                }
                
                if (categoriasInfo.TryGetProperty("categorias_no_encontradas", out var cantidadNoEncontradas))
                {
                    result.CategoriasNoEncontradasCount = cantidadNoEncontradas.GetInt32();
                }

                if (result.CategoriasNoEncontradas.Any())
                {
                    _logger.LogWarning("Lote {BatchNumber}: {Count} categorías no encontradas: {Categorias}", 
                        batchNumber, result.CategoriasNoEncontradas.Count, 
                        string.Join(", ", result.CategoriasNoEncontradas));
                }
            }
            
            _logger.LogInformation("Lote {BatchNumber} procesado: {Nuevos} nuevos, {Actualizados} actualizados, {Errores} errores", 
                batchNumber, result.ProductosNuevos, result.ProductosActualizados, result.Errores);

            return new ApiResponse<BatchProcessResult>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando lote {BatchNumber}", batchNumber);
            return new ApiResponse<BatchProcessResult>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ApiResponse<SyncSessionStatus>> GetSessionStatusAsync(string sessionId)
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                return new ApiResponse<SyncSessionStatus>
                {
                    Success = false,
                    ErrorMessage = "Error de autenticación"
                };
            }

            _logger.LogInformation("Consultando estado de sesión: {SessionId}", sessionId);

            var response = await _httpClient.GetAsync($"/api/admin/sync/session/{sessionId}/status");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error consultando estado de sesión {SessionId}: {StatusCode} - {Content}", 
                    sessionId, response.StatusCode, responseContent);

                return new ApiResponse<SyncSessionStatus>
                {
                    Success = false,
                    ErrorMessage = $"HTTP {response.StatusCode}: {responseContent}"
                };
            }

            var status = JsonSerializer.Deserialize<SyncSessionStatus>(responseContent);

            return new ApiResponse<SyncSessionStatus>
            {
                Success = true,
                Data = status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando estado de sesión {SessionId}", sessionId);
            return new ApiResponse<SyncSessionStatus>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ApiResponse<SyncSessionResult>> FinishSyncSessionAsync(string sessionId)
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                return new ApiResponse<SyncSessionResult>
                {
                    Success = false,
                    ErrorMessage = "Error de autenticación"
                };
            }

            var finishRequest = new
            {
                estado = "completada"
            };

            var json = JsonSerializer.Serialize(finishRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Finalizando sesión de sincronización: {SessionId}", sessionId);

            var response = await _httpClient.PostAsync($"/api/admin/sync/session/{sessionId}/finish", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error finalizando sesión {SessionId}: {StatusCode} - {Content}", 
                    sessionId, response.StatusCode, responseContent);

                return new ApiResponse<SyncSessionResult>
                {
                    Success = false,
                    ErrorMessage = $"HTTP {response.StatusCode}: {responseContent}"
                };
            }

            var result = JsonSerializer.Deserialize<SyncSessionResult>(responseContent);
            
            _logger.LogInformation("Sesión {SessionId} finalizada exitosamente", sessionId);

            return new ApiResponse<SyncSessionResult>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizando sesión {SessionId}", sessionId);
            return new ApiResponse<SyncSessionResult>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<bool> EnsureAuthenticatedAsync()
    {
        if (IsTokenValid())
            return true;

        return await AuthenticateAsync();
    }

    private bool IsTokenValid()
    {
        return !string.IsNullOrEmpty(_jwtToken) && DateTime.UtcNow < _tokenExpiry;
    }
}

public class AuthResponse
{
    public string? accessToken { get; set; }
}

public class SyncSession
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public string ArchivoNombre { get; set; } = string.Empty;
}

public class SyncSessionStatus
{
    public string SessionId { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int LotesProcesados { get; set; }
    public int TotalLotes { get; set; }
    public int ProductosNuevos { get; set; }
    public int ProductosActualizados { get; set; }
    public int Errores { get; set; }
}

public class SyncSessionResult
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime FechaFin { get; set; }
    public int ProductosNuevos { get; set; }
    public int ProductosActualizados { get; set; }
    public int Errores { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class BatchProcessResult
{
    public int ProductosNuevos { get; set; }
    public int ProductosActualizados { get; set; }
    public int Errores { get; set; }
    public string? DetallesErrores { get; set; }
    
    // Información de categorías faltantes
    public List<int> CategoriasNoEncontradas { get; set; } = new();
    public int CategoriasNoEncontradasCount { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
}