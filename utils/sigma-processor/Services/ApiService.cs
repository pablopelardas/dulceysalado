using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SigmaProcessor.Config;
using SigmaProcessor.Models;

namespace SigmaProcessor.Services;

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
            // TEMPORAL: Deshabilitar autenticación para pruebas
            var skipAuth = Environment.GetEnvironmentVariable("SKIP_AUTH")?.ToLower() == "true" ||
                          _apiConfig.Username == "SKIP_AUTH";

            if (skipAuth)
            {
                _logger.LogInformation("⚠️ AUTENTICACIÓN DESHABILITADA PARA PRUEBAS");
                return true;
            }

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
            var response = await _httpClient.PostAsync("/api/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error en autenticación: {StatusCode} - {Content}",
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);

            if (authResponse?.access_token == null)
            {
                _logger.LogError("Respuesta de autenticación inválida");
                return false;
            }

            _jwtToken = authResponse.access_token;
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

    public async Task<ApiResponse<SyncSession>> StartSyncSessionAsync(string fileName, string? listaCodigo = null, bool multiLista = false)
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
                usuario_proceso = "SISTEMA_GECOM",
                lista_codigo = listaCodigo, // Especificar lista de precios (null para modo multi-lista)
                multi_lista = multiLista // NUEVO: Flag para modo multi-lista
            };

            var json = JsonSerializer.Serialize(sessionRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Iniciando sesión de sincronización para archivo: {FileName}", fileName);

            var response = await _httpClient.PostAsync("/api/sync/session/start", content);
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

    public async Task<ApiResponse<BatchProcessResult>> ProcessBatchWithPricesAsync(List<ProductWithPrices> batch, int batchNumber, string sessionId)
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
                productos = batch.Select(p => 
                {
                    _logger.LogInformation("API MAPPING - Producto {Codigo}: {Count} listas", p.Codigo, p.ListasPrecios.Count);
                    _logger.LogInformation("API MAPPING - Producto {Codigo} Grupos: G1={Grupo1}, G2={Grupo2}, G3={Grupo3}", 
                        p.Codigo, p.Grupo1, p.Grupo2, p.Grupo3);
                    
                    // Log detallado para producto 92
                    if (p.Codigo == "92")
                    {
                        _logger.LogInformation("API MAPPING DETALLE - Producto 92 antes de mapear:");
                        for (int i = 0; i < p.ListasPrecios.Count; i++)
                        {
                            _logger.LogInformation("  Lista[{Index}]: ListaId={Id}, Precio={Precio}", 
                                i, p.ListasPrecios[i].ListaId, p.ListasPrecios[i].Precio);
                        }
                    }
                    
                    var productoMapeado = new
                    {
                        codigo = p.Codigo.ToString(),
                        nombre = p.Descripcion,
                        descripcion = p.Descripcion,
                        categoria_id = p.CodigoRubro,
                        precio = p.ListasPrecios.Any() ? (decimal?)null : p.PrecioDefault, // null si tiene listas_precios
                        stock = (int)p.Existencia,
                        activo = p.Disponible == "S",
                        visible_catalogo = p.Imputable == "S",
                        // Campos de grupos para filtros
                        grupo1 = p.Grupo1,
                        grupo2 = p.Grupo2,
                        grupo3 = p.Grupo3,
                        // Campos adicionales de fechas y ubicación
                        fecha_alta = p.FechaAlta?.ToString("yyyy-MM-dd"),
                        fecha_modi = p.FechaModi?.ToString("yyyy-MM-dd"),
                        codigo_ubicacion = p.CodigoUbicacion,
                        // NUEVO: Listas de precios (formato ProductPriceDto)
                        listas_precios = p.ListasPrecios.Select((lp, idx) => {
                            var lista = new
                            {
                                ListaId = lp.ListaId,
                                Precio = lp.Precio,
                                Fecha = lp.Fecha?.ToString("yyyy-MM-dd")
                            };
                            
                            if (p.Codigo == "92")
                            {
                                _logger.LogInformation("API MAPPING - Lista[{Index}] mapeada: ListaId={Id}, Precio={Precio}", 
                                    idx, lista.ListaId, lista.Precio);
                            }
                            
                            return lista;
                        }).ToArray(),
                        porcentaje = p.Porcentaje,
                        // NUEVO: Stocks por empresa
                        stocks_por_empresa = p.StocksPorEmpresa?.Select(s => new
                        {
                            empresa_id = s.EmpresaId,
                            stock = s.Stock
                        }).ToArray() ?? new object[0]
                    };
                    
                    if (p.Codigo == "92")
                    {
                        _logger.LogInformation("API MAPPING - Producto 92 después de mapear: {Count} listas", 
                            productoMapeado.listas_precios.Length);
                    }
                    
                    return productoMapeado;
                }).ToArray()
            };

            var json = JsonSerializer.Serialize(batchRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            // Debug: Log del JSON para el primer producto si es el 92
            if (batch.Any(p => p.Codigo == "92"))
            {
                try
                {
                    using var jsonDoc = JsonDocument.Parse(json);
                    var productos = jsonDoc.RootElement.GetProperty("productos");
                    foreach (var prod in productos.EnumerateArray())
                    {
                        if (prod.GetProperty("codigo").GetString() == "92")
                        {
                            _logger.LogInformation("=== JSON FINAL PRODUCTO 92 ===");
                            _logger.LogInformation(JsonSerializer.Serialize(prod, new JsonSerializerOptions { WriteIndented = true }));
                            
                            // Verificar listas_precios en el JSON
                            if (prod.TryGetProperty("listas_precios", out var listasPreciosElement))
                            {
                                _logger.LogInformation("JSON - Listas de precios count: {Count}", listasPreciosElement.GetArrayLength());
                                int idx = 0;
                                foreach (var lista in listasPreciosElement.EnumerateArray())
                                {
                                    var listaId = lista.TryGetProperty("listaId", out var listaIdEl) ? listaIdEl.GetInt32() : -1;
                                    var precio = lista.TryGetProperty("precio", out var precioEl) ? precioEl.GetDecimal() : 0m;
                                    _logger.LogInformation("  JSON Lista[{Index}]: listaId={Id}, precio={Precio}", idx++, listaId, precio);
                                }
                            }
                            _logger.LogInformation("=== FIN JSON PRODUCTO 92 ===");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error en debug JSON: {Error}", ex.Message);
                }
            }

            // Log de información del lote incluyendo campos de grupos y precios
            var productosConGrupos = batch.Where(p => p.Grupo1.HasValue || p.Grupo2.HasValue || p.Grupo3.HasValue).Count();
            var productosConMultiplesPrecios = batch.Where(p => p.ListasPrecios.Count > 1).Count();
            var totalListasPrecios = batch.Sum(p => p.ListasPrecios.Count);
            
            _logger.LogInformation("Enviando lote {BatchNumber} con {Count} productos (Sesión: {SessionId}) - {ProductosConGrupos} con grupos, {ProductosConMultiplesPrecios} con múltiples precios, {TotalListas} listas total", 
                batchNumber, batch.Count, sessionId, productosConGrupos, productosConMultiplesPrecios, totalListasPrecios);
                
            // Log de muestra de los primeros productos con múltiples precios (para debugging)
            var primerosConPrecios = batch.Where(p => p.ListasPrecios.Any()).Take(3);
            foreach (var producto in primerosConPrecios)
            {
                var listas = string.Join(", ", producto.ListasPrecios.Select(lp => $"L{lp.ListaId}:{lp.Precio:F2}"));
                _logger.LogInformation("DEBUG - Producto {Codigo}: {Count} listas = {Listas}", 
                    producto.Codigo, producto.ListasPrecios.Count, listas);
                    
                // Debug detallado del primer producto
                if (producto.Codigo == "92")
                {
                    _logger.LogInformation("DEBUG DETALLADO - Producto 92:");
                    for (int i = 0; i < producto.ListasPrecios.Count; i++)
                    {
                        var lista = producto.ListasPrecios[i];
                        _logger.LogInformation("  Lista[{Index}]: ID={ListaId}, Precio={Precio}, Fecha={Fecha}", 
                            i, lista.ListaId, lista.Precio, lista.Fecha);
                    }
                }
            }
                
            var response = await _httpClient.PostAsync("/api/sync/products/bulk", content);
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

    /// <summary>
    /// Procesa un lote de productos actualizando SOLO sus stocks
    /// </summary>
    public async Task<ApiResponse<BatchProcessResult>> ProcessStockOnlyBatchAsync(List<ProductStockOnly> batch, int batchNumber, string sessionId)
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
                stock_only_mode = true, // Flag para indicar que solo se actualiza stock
                productos = batch.Select(p => new
                {
                    codigo = p.Codigo.ToString(),
                    stocks_por_empresa = p.StocksPorEmpresa?.Select(s => new
                    {
                        empresa_id = s.EmpresaId,
                        stock = s.Stock
                    }).ToArray() ?? new object[0]
                }).ToArray()
            };

            var json = JsonSerializer.Serialize(batchRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _logger.LogInformation("Enviando lote {BatchNumber} (solo stocks) con {Count} productos", 
                batchNumber, batch.Count);

            var response = await _httpClient.PostAsync("/api/sync/products/bulk", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error en lote {BatchNumber}: {StatusCode} - {Content}", 
                    batchNumber, response.StatusCode, responseContent);
                return new ApiResponse<BatchProcessResult>
                {
                    Success = false,
                    ErrorMessage = $"Error {response.StatusCode}: {responseContent}"
                };
            }

            var result = new BatchProcessResult();
            
            // Parsear respuesta
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;
            
            if (root.TryGetProperty("productos_nuevos", out var nuevos))
                result.ProductosNuevos = nuevos.GetInt32();
            
            if (root.TryGetProperty("productos_actualizados", out var actualizados))
                result.ProductosActualizados = actualizados.GetInt32();
            
            if (root.TryGetProperty("errores", out var errores))
                result.Errores = errores.GetInt32();
            
            _logger.LogInformation("Lote {BatchNumber} (solo stocks) procesado: {Actualizados} actualizados, {Errores} errores", 
                batchNumber, result.ProductosActualizados, result.Errores);

            return new ApiResponse<BatchProcessResult>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando lote {BatchNumber} (solo stocks)", batchNumber);
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

            var response = await _httpClient.GetAsync($"/api/sync/session/{sessionId}/status");
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

            var response = await _httpClient.PostAsync($"/api/sync/session/{sessionId}/finish", content);
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

    public async Task<ApiResponse<List<ListaPrecio>>> GetListasPreciosAsync()
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                return new ApiResponse<List<ListaPrecio>>
                {
                    Success = false,
                    ErrorMessage = "Error de autenticación"
                };
            }

            _logger.LogInformation("Obteniendo listas de precios disponibles...");

            var response = await _httpClient.GetAsync("/api/listas-precios");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error obteniendo listas de precios: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);
                return new ApiResponse<List<ListaPrecio>>
                {
                    Success = false,
                    ErrorMessage = $"Error HTTP: {response.StatusCode}"
                };
            }

            var apiResponse = JsonSerializer.Deserialize<ApiListasPreciosResponse>(responseContent);

            return new ApiResponse<List<ListaPrecio>>
            {
                Success = apiResponse?.success ?? false,
                Data = apiResponse?.listas?.ToList() ?? new List<ListaPrecio>(),
                ErrorMessage = apiResponse?.success == false ? "Error en respuesta API" : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo listas de precios");
            return new ApiResponse<List<ListaPrecio>>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    // NUEVO: Método para importación masiva usando la nueva API de sincronización
    public async Task<ApiResponse<BulkImportResult>> BulkImportProductsAsync(List<ProductWithPrices> products, string fileName)
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                return new ApiResponse<BulkImportResult>
                {
                    Success = false,
                    ErrorMessage = "Error de autenticación"
                };
            }

            _logger.LogInformation("Iniciando importación masiva de {Count} productos usando nueva API", products.Count);

            // Mapear productos a formato de la nueva API
            var bulkRequest = new BulkImportRequest
            {
                ExternalSystemId = "GECOM_PROCESSOR",
                Products = products.Select(p => new BulkProductDto
                {
                    ExternalId = p.Codigo.ToString(),
                    Code = p.Codigo.ToString(),
                    Description = p.Descripcion,
                    Category = p.CodigoRubro?.ToString(),
                    Brand = null,
                    Supplier = null,
                    Prices = p.ListasPrecios.Select(lp => new BulkPriceDto
                    {
                        ListCode = $"LISTA_{lp.ListaId}",
                        ListName = $"Lista {lp.ListaId}",
                        Price = lp.Precio,
                        Currency = "ARS",
                        ValidFrom = lp.Fecha
                    }).ToList(),
                    Stock = new BulkStockDto
                    {
                        Quantity = p.Existencia,
                        Unit = "UN",
                        Location = p.CodigoUbicacion
                    },
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["grupo1"] = p.Grupo1 ?? 0,
                        ["grupo2"] = p.Grupo2 ?? 0,
                        ["grupo3"] = p.Grupo3 ?? 0,
                        ["fechaAlta"] = p.FechaAlta?.ToString("yyyy-MM-dd") ?? "",
                        ["fechaModi"] = p.FechaModi?.ToString("yyyy-MM-dd") ?? "",
                        ["imputable"] = p.Imputable,
                        ["disponible"] = p.Disponible,
                        ["porcentaje"] = p.Porcentaje
                    }
                }).ToList(),
                ProcessImmediately = true,
                BatchSize = 50
            };

            var json = JsonSerializer.Serialize(bulkRequest, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Enviando {Count} productos a nueva API de sincronización", products.Count);

            var response = await _httpClient.PostAsync("/synchronization/products/bulk-import", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error en importación masiva: {StatusCode} - {Content}",
                    response.StatusCode, responseContent);

                return new ApiResponse<BulkImportResult>
                {
                    Success = false,
                    ErrorMessage = $"HTTP {response.StatusCode}: {responseContent}"
                };
            }

            var bulkResponse = JsonSerializer.Deserialize<BulkImportResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (bulkResponse == null)
            {
                return new ApiResponse<BulkImportResult>
                {
                    Success = false,
                    ErrorMessage = "Respuesta inválida de la API"
                };
            }

            var result = new BulkImportResult
            {
                OperationId = bulkResponse.OperationId,
                TotalProducts = bulkResponse.TotalProducts,
                RecordsCreated = bulkResponse.RecordsCreated,
                Status = bulkResponse.Status,
                Message = bulkResponse.Message,
                Errors = bulkResponse.Errors
            };

            _logger.LogInformation("Importación masiva iniciada: OperationId={OperationId}, Status={Status}",
                result.OperationId, result.Status);

            return new ApiResponse<BulkImportResult>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en importación masiva");
            return new ApiResponse<BulkImportResult>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    // NUEVO: Método para consultar estado de operación masiva
    public async Task<ApiResponse<BulkOperationStatus>> GetBulkOperationStatusAsync(Guid operationId)
    {
        try
        {
            if (!await EnsureAuthenticatedAsync())
            {
                return new ApiResponse<BulkOperationStatus>
                {
                    Success = false,
                    ErrorMessage = "Error de autenticación"
                };
            }

            _logger.LogDebug("Consultando estado de operación {OperationId}", operationId);

            var response = await _httpClient.GetAsync($"/synchronization/operations/{operationId}/status");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error consultando estado de operación {OperationId}: {StatusCode} - {Content}",
                    operationId, response.StatusCode, responseContent);

                return new ApiResponse<BulkOperationStatus>
                {
                    Success = false,
                    ErrorMessage = $"HTTP {response.StatusCode}: {responseContent}"
                };
            }

            var statusResponse = JsonSerializer.Deserialize<BulkOperationStatus>(responseContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return new ApiResponse<BulkOperationStatus>
            {
                Success = true,
                Data = statusResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando estado de operación {OperationId}", operationId);
            return new ApiResponse<BulkOperationStatus>
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    // NUEVO: Método principal para procesar productos usando la nueva API
    public async Task<ApiResponse<BulkProcessResult>> ProcessProductsBulkAsync(List<ProductWithPrices> products, string fileName)
    {
        try
        {
            _logger.LogInformation("=== INICIANDO PROCESAMIENTO BULK ===");
            _logger.LogInformation("Archivo: {FileName}", fileName);
            _logger.LogInformation("Total productos: {Count}", products.Count);

            // 1. Enviar productos a la nueva API
            var importResult = await BulkImportProductsAsync(products, fileName);
            if (!importResult.Success || importResult.Data == null)
            {
                return new ApiResponse<BulkProcessResult>
                {
                    Success = false,
                    ErrorMessage = importResult.ErrorMessage,
                    Data = new BulkProcessResult
                    {
                        OperationId = Guid.Empty,
                        TotalProducts = products.Count,
                        Status = "Failed",
                        Message = importResult.ErrorMessage ?? "Error desconocido"
                    }
                };
            }

            var operationId = importResult.Data.OperationId;
            _logger.LogInformation("Operación iniciada: {OperationId}", operationId);

            BulkOperationStatus? finalStatus = null;

            // 2. Verificar si necesita monitoreo o si ya está completado
            if (operationId == Guid.Empty)
            {
                // Endpoint simplificado - resultado inmediato, no necesita monitoreo
                _logger.LogInformation("Endpoint simplificado detectado - resultado inmediato sin monitoreo");

                // Crear estado final basado en la respuesta del import
                finalStatus = new BulkOperationStatus
                {
                    OperationId = Guid.Empty,
                    Status = "Completed",
                    TotalProducts = importResult.Data.TotalProducts,
                    ProcessedProducts = importResult.Data.TotalProducts,
                    SuccessCount = importResult.Data.TotalProducts,
                    ErrorCount = 0,
                    ProgressPercentage = 100,
                    StartedAt = DateTime.UtcNow,
                    CompletedAt = DateTime.UtcNow,
                    Errors = new List<BulkProductError>()
                };
            }
            else
            {
                // Endpoint tradicional - necesita monitoreo
                _logger.LogInformation("Endpoint tradicional detectado - iniciando monitoreo");
                finalStatus = await MonitorBulkOperationAsync(operationId);
            }

            var result = new BulkProcessResult
            {
                OperationId = operationId,
                TotalProducts = finalStatus?.TotalProducts ?? products.Count,
                ProcessedProducts = finalStatus?.ProcessedProducts ?? 0,
                SuccessCount = finalStatus?.SuccessCount ?? 0,
                ErrorCount = finalStatus?.ErrorCount ?? 0,
                Status = finalStatus?.Status ?? "Unknown",
                Message = $"Procesamiento completado. {finalStatus?.SuccessCount ?? 0} exitosos, {finalStatus?.ErrorCount ?? 0} errores",
                ProgressPercentage = finalStatus?.ProgressPercentage ?? 0,
                StartedAt = finalStatus?.StartedAt ?? DateTime.UtcNow,
                CompletedAt = finalStatus?.CompletedAt,
                Errors = finalStatus?.Errors?.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
            };

            _logger.LogInformation("=== PROCESAMIENTO BULK COMPLETADO ===");
            _logger.LogInformation("Estado final: {Status}", result.Status);
            _logger.LogInformation("Productos procesados: {Processed}/{Total}", result.ProcessedProducts, result.TotalProducts);
            _logger.LogInformation("Exitosos: {Success}, Errores: {Errors}", result.SuccessCount, result.ErrorCount);

            return new ApiResponse<BulkProcessResult>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en procesamiento bulk");
            return new ApiResponse<BulkProcessResult>
            {
                Success = false,
                ErrorMessage = ex.Message,
                Data = new BulkProcessResult
                {
                    OperationId = Guid.Empty,
                    TotalProducts = products.Count,
                    Status = "Failed",
                    Message = $"Error en procesamiento: {ex.Message}"
                }
            };
        }
    }

    private async Task<BulkOperationStatus?> MonitorBulkOperationAsync(Guid operationId)
    {
        const int maxWaitMinutes = 30; // Máximo 30 minutos de espera
        const int pollIntervalSeconds = 10; // Consultar cada 10 segundos

        var startTime = DateTime.UtcNow;
        var maxWaitTime = startTime.AddMinutes(maxWaitMinutes);

        _logger.LogInformation("Iniciando monitoreo de operación {OperationId} (máximo {MaxMinutes} minutos)",
            operationId, maxWaitMinutes);

        while (DateTime.UtcNow < maxWaitTime)
        {
            try
            {
                var statusResult = await GetBulkOperationStatusAsync(operationId);
                if (!statusResult.Success || statusResult.Data == null)
                {
                    _logger.LogWarning("Error consultando estado: {Error}", statusResult.ErrorMessage);
                    await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds));
                    continue;
                }

                var status = statusResult.Data;
                _logger.LogInformation("Estado: {Status} - Progreso: {Progress}% ({Processed}/{Total})",
                    status.Status, status.ProgressPercentage, status.ProcessedProducts, status.TotalProducts);

                // Verificar si la operación está completa
                if (IsOperationComplete(status.Status))
                {
                    _logger.LogInformation("Operación completada con estado: {Status}", status.Status);
                    return status;
                }

                // Esperar antes de la siguiente consulta
                await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante monitoreo de operación {OperationId}", operationId);
                await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds));
            }
        }

        _logger.LogWarning("Timeout esperando completar operación {OperationId} después de {Minutes} minutos",
            operationId, maxWaitMinutes);

        // Intentar obtener estado final
        try
        {
            var finalStatusResult = await GetBulkOperationStatusAsync(operationId);
            return finalStatusResult.Success ? finalStatusResult.Data : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsOperationComplete(string status)
    {
        return status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Failed", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Partial", StringComparison.OrdinalIgnoreCase);
    }
}

public class AuthResponse
{
    public string? access_token { get; set; }
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

public class ListaPrecio
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activa { get; set; }
    public bool EsPredeterminada { get; set; }
}

public class ApiListasPreciosResponse
{
    public bool success { get; set; }
    public ListaPrecio[] listas { get; set; } = Array.Empty<ListaPrecio>();
}

// NUEVOS DTOs para la API de sincronización bulk
public class BulkImportRequest
{
    public string ExternalSystemId { get; set; } = "";
    public List<BulkProductDto> Products { get; set; } = new();
    public bool ProcessImmediately { get; set; } = true;
    public int BatchSize { get; set; } = 50;
}

public class BulkProductDto
{
    public string ExternalId { get; set; } = "";
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Supplier { get; set; }
    public List<BulkPriceDto> Prices { get; set; } = new();
    public BulkStockDto? Stock { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class BulkPriceDto
{
    public string ListCode { get; set; } = "";
    public string ListName { get; set; } = "";
    public decimal Price { get; set; }
    public string Currency { get; set; } = "ARS";
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class BulkStockDto
{
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "UN";
    public decimal? MinStock { get; set; }
    public decimal? MaxStock { get; set; }
    public string? Location { get; set; }
}

public class BulkImportResponse
{
    public Guid OperationId { get; set; }
    public int TotalProducts { get; set; }
    public int RecordsCreated { get; set; }
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
    public List<string>? Errors { get; set; }
}

public class BulkImportResult
{
    public Guid OperationId { get; set; }
    public int TotalProducts { get; set; }
    public int RecordsCreated { get; set; }
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
    public List<string>? Errors { get; set; }
}

public class BulkOperationStatus
{
    public Guid OperationId { get; set; }
    public string Status { get; set; } = "";
    public int TotalProducts { get; set; }
    public int ProcessedProducts { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public decimal ProgressPercentage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<BulkProductError> Errors { get; set; } = new();
}



public class BulkProductError
{
    public string ExternalId { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public string ErrorCode { get; set; } = "";
    public DateTime OccurredAt { get; set; }
}

public class BulkProcessResult
{
    public Guid OperationId { get; set; }
    public int TotalProducts { get; set; }
    public int ProcessedProducts { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
    public decimal ProgressPercentage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<string> Errors { get; set; } = new();
}