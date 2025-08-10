using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Sync;
using DistriCatalogoAPI.Application.Queries.Sync;
using System.Text.Json.Serialization;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/sync")]
    [Authorize]
    public class SyncController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SyncController> _logger;

        public SyncController(IMediator mediator, ILogger<SyncController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/sync/session/start
        /// Inicia una nueva sesión de sincronización
        /// </summary>
        [HttpPost("session/start")]
        public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                var ipOrigen = GetClientIpAddress();

                var command = new StartSyncSessionCommand
                {
                    TotalLotesEsperados = request.TotalLotesEsperados,
                    UsuarioProceso = request.UsuarioProceso,
                    IpOrigen = ipOrigen,
                    EmpresaPrincipalId = empresaId,
                    ListaCodigo = request.ListaCodigo,
                    MultiLista = request.MultiLista
                };

                var result = await _mediator.Send(command);

                _logger.LogInformation("Sesión de sync iniciada: {SessionId} para empresa {EmpresaId}", 
                    result.SessionId, empresaId);

                return Ok(new
                {
                    success = true,
                    message = "Sesión de sincronización iniciada",
                    session_id = result.SessionId,
                    empresa_principal = result.EmpresaPrincipal,
                    fecha_inicio = result.FechaInicio,
                    total_lotes_esperados = result.TotalLotesEsperados,
                    lista_codigo = result.ListaCodigo,
                    lista_nombre = result.ListaNombre
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión de sincronización");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// POST /api/sync/products/bulk
        /// Procesa un lote de productos
        /// </summary>
        [HttpPost("products/bulk")]
        public async Task<IActionResult> ProcessBulkProducts([FromBody] ProcessBulkRequest request)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                
                // DEBUG: Log para verificar que lleguen los stocks por empresa
                _logger.LogInformation("🔍 DEBUG - Productos recibidos: {Count}", request.Productos.Length);
                foreach (var producto in request.Productos.Take(3)) // Solo los primeros 3 para no saturar logs
                {
                    _logger.LogInformation("🔍 DEBUG - Producto {Codigo}: StocksPorEmpresa Count = {Count}", 
                        producto.Codigo, producto.StocksPorEmpresa?.Count ?? 0);
                    
                    if (producto.StocksPorEmpresa?.Any() == true)
                    {
                        foreach (var stock in producto.StocksPorEmpresa)
                        {
                            _logger.LogInformation("🔍 DEBUG - Stock EmpresaId: {EmpresaId}, Stock: {Stock}", 
                                stock.EmpresaId, stock.Stock);
                        }
                    }
                }

                var command = new ProcessBulkProductsCommand
                {
                    SessionId = request.SessionId,
                    LoteNumero = request.LoteNumero,
                    StockOnlyMode = request.StockOnlyMode,
                    Productos = request.Productos.Select(p => new BulkProductDto
                    {
                        Codigo = p.Codigo,
                        Descripcion = p.Descripcion ?? "", // Descripción vacía si es null
                        CategoriaId = p.CategoriaId,
                        Precio = p.Precio,
                        ListasPrecios = p.ListasPrecios?.Select(lp => new ProductPriceDto
                        {
                            ListaId = lp.ListaId,
                            Precio = lp.Precio,
                            Fecha = lp.Fecha
                        }).ToList(),
                        Stock = p.Stock,
                        StocksPorEmpresa = p.StocksPorEmpresa?.Select(se => new StockPorEmpresaDto
                        {
                            EmpresaId = se.EmpresaId,
                            Stock = se.Stock
                        }).ToList() ?? new List<StockPorEmpresaDto>(),
                        Grupo1 = p.Grupo1,
                        Grupo2 = p.Grupo2,
                        Grupo3 = p.Grupo3,
                        FechaAlta = p.FechaAlta,
                        FechaModi = p.FechaModi,
                        Imputable = p.Imputable,
                        Disponible = p.Disponible,
                        CodigoUbicacion = p.CodigoUbicacion
                    }).ToList(),
                    EmpresaPrincipalId = empresaId
                };

                // DEBUG: Log para verificar el mapeo al Command
                _logger.LogInformation("🔍 DEBUG - Command creado con {ProductCount} productos, StockOnlyMode: {StockOnlyMode}", 
                    command.Productos.Count, command.StockOnlyMode);
                foreach (var producto in command.Productos.Take(3))
                {
                    _logger.LogInformation("🔍 DEBUG - Producto mapeado {Codigo}: StocksPorEmpresa Count = {Count}", 
                        producto.Codigo, producto.StocksPorEmpresa?.Count ?? 0);
                        
                    if (producto.StocksPorEmpresa?.Any() == true)
                    {
                        foreach (var stock in producto.StocksPorEmpresa)
                        {
                            _logger.LogInformation("🔍 DEBUG - Stock mapeado EmpresaId: {EmpresaId}, Stock: {Stock}", 
                                stock.EmpresaId, stock.Stock);
                        }
                    }
                }

                var result = await _mediator.Send(command);

                return Ok(new
                {
                    success = true,
                    session_id = result.SessionId,
                    lote_numero = result.LoteNumero,
                    total_lotes = result.TotalLotes,
                    estadisticas = new
                    {
                        productos_procesados = result.Estadisticas.ProductosProcesados,
                        productos_actualizados = result.Estadisticas.ProductosActualizados,
                        productos_nuevos = result.Estadisticas.ProductosNuevos,
                        errores = result.Estadisticas.Errores,
                        errores_detalle = result.Estadisticas.ErroresDetalle
                    },
                    tiempo_procesamiento_ms = result.TiempoProcesamientoMs,
                    progreso = new
                    {
                        porcentaje = result.Progreso.Porcentaje,
                        lotes_procesados = result.Progreso.LotesProcesados,
                        total_lotes_esperados = result.Progreso.TotalLotesEsperados,
                        estado = result.Progreso.Estado
                    },
                    categorias_info = new
                    {
                        categorias_existentes_inicialmente = result.CategoriasInfo.CategoriasExistentesInicialmente,
                        categorias_creadas_automaticamente = result.CategoriasInfo.CategoriasCreatesAutomaticamente,
                        categorias_creadas_lista = result.CategoriasInfo.CategoriasCreatesLista,
                        total_categorias_procesadas = result.CategoriasInfo.TotalCategoriasProcesadas
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando lote de productos");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET /api/sync/logs
        /// Obtiene los logs de sincronización más recientes para debugging
        /// </summary>
        [HttpGet("logs")]
        public async Task<IActionResult> GetRecentSyncLogs([FromQuery] int take = 10)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();
                
                var query = new GetSyncLogsQuery
                {
                    EmpresaPrincipalId = empresaId,
                    Page = 1,
                    Limit = take
                };

                var result = await _mediator.Send(query);
                
                return Ok(new
                {
                    success = true,
                    logs = result.Logs.Select(log => new
                    {
                        id = log.Id,
                        archivo_nombre = log.ArchivoNombre,
                        fecha_procesamiento = log.FechaProcesamiento,
                        productos_actualizados = log.ProductosActualizados,
                        productos_nuevos = log.ProductosNuevos,
                        errores = log.Errores,
                        tiempo_procesamiento_ms = log.TiempoProcesamientoMs,
                        estado = log.Estado.ToString(),
                        usuario_proceso = log.UsuarioProceso
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo logs de sync");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// POST /api/sync/session/{sessionId}/finish
        /// Finaliza una sesión de sincronización
        /// </summary>
        [HttpPost("session/{sessionId}/finish")]
        public async Task<IActionResult> FinishSession(Guid sessionId, [FromBody] FinishSessionRequest request)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();

                var command = new FinishSyncSessionCommand
                {
                    SessionId = sessionId,
                    Estado = request.Estado ?? "completada",
                    EmpresaPrincipalId = empresaId
                };

                var result = await _mediator.Send(command);

                _logger.LogInformation("Sesión de sync finalizada: {SessionId} con estado {Estado}", 
                    sessionId, result.EstadoFinal);

                return Ok(new
                {
                    success = true,
                    message = "Sesión de sincronización finalizada",
                    session_id = result.SessionId,
                    estado_final = result.EstadoFinal,
                    resumen = new
                    {
                        productos_totales = result.Resumen.ProductosTotales,
                        productos_actualizados = result.Resumen.ProductosActualizados,
                        productos_nuevos = result.Resumen.ProductosNuevos,
                        productos_errores = result.Resumen.ProductosErrores,
                        lotes_procesados = result.Resumen.LotesProcesados,
                        tiempo_total_ms = result.Resumen.TiempoTotalMs,
                        productos_por_segundo = result.Resumen.ProductosPorSegundo,
                        tasa_exito = result.Resumen.TasaExito
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar sesión {SessionId}", sessionId);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET /api/sync/session/{sessionId}/status
        /// Obtiene el estado actual de una sesión de sincronización
        /// </summary>
        [HttpGet("session/{sessionId}/status")]
        public async Task<IActionResult> GetSessionStatus(Guid sessionId)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();

                var query = new GetSyncSessionQuery
                {
                    SessionId = sessionId,
                    EmpresaPrincipalId = empresaId
                };

                var result = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    session = new
                    {
                        id = result.Id,
                        estado = result.Estado,
                        empresa = result.Empresa,
                        fecha_inicio = result.FechaInicio,
                        fecha_fin = result.FechaFin,
                        usuario_proceso = result.UsuarioProceso,
                        progreso = new
                        {
                            porcentaje = result.Progreso.Porcentaje,
                            lotes_procesados = result.Progreso.LotesProcesados,
                            total_lotes_esperados = result.Progreso.TotalLotesEsperados,
                            productos_procesados = result.Progreso.ProductosProcesados,
                            estado = result.Progreso.Estado
                        },
                        metricas = new
                        {
                            productos_totales = result.Metricas.ProductosTotales,
                            productos_actualizados = result.Metricas.ProductosActualizados,
                            productos_nuevos = result.Metricas.ProductosNuevos,
                            productos_errores = result.Metricas.ProductosErrores,
                            tiempo_total_ms = result.Metricas.TiempoTotalMs,
                            productos_por_segundo = result.Metricas.ProductosPorSegundo,
                            tiempo_promedio_ms = result.Metricas.TiempoPromedioMs
                        },
                        errores_detalle = result.ErroresDetalle.DetallesErrores
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de sesión {SessionId}", sessionId);
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET /api/sync/sessions
        /// Lista las sesiones de sincronización con paginación
        /// </summary>
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions(
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 20, 
            [FromQuery] string estado = null)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();

                var query = new GetSyncSessionsQuery
                {
                    Page = page,
                    Limit = limit,
                    Estado = estado,
                    EmpresaPrincipalId = empresaId
                };

                var result = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    sessions = result.Sessions.Select(s => new
                    {
                        id = s.Id,
                        estado = s.Estado,
                        fecha_inicio = s.FechaInicio,
                        fecha_fin = s.FechaFin,
                        usuario_proceso = s.UsuarioProceso,
                        empresa = s.Empresa,
                        productos_totales = s.ProductosTotales,
                        productos_errores = s.ProductosErrores,
                        tiempo_total_ms = s.TiempoTotalMs,
                        // Calcular métricas dinámicamente desde los datos que SÍ funcionan
                        productos_por_segundo = s.TiempoTotalMs > 0 
                            ? Math.Round(s.ProductosTotales / (s.TiempoTotalMs / 1000.0), 2) 
                            : 0,
                        tasa_exito = s.ProductosTotales > 0 
                            ? Math.Round(((s.ProductosTotales - s.ProductosErrores) * 100.0) / s.ProductosTotales, 2)
                            : 100,
                        tiempo_promedio_por_lote_ms = s.Progreso.LotesProcesados > 0 
                            ? Math.Round((double)s.TiempoTotalMs / s.Progreso.LotesProcesados, 0)
                            : 0,
                        progreso = new
                        {
                            porcentaje = s.Progreso.Porcentaje,
                            lotes_procesados = s.Progreso.LotesProcesados,
                            total_lotes_esperados = s.Progreso.TotalLotesEsperados,
                            estado = s.Progreso.Estado
                        }
                    }).ToList(),
                    pagination = new
                    {
                        total = result.Pagination.Total,
                        page = result.Pagination.Page,
                        limit = result.Pagination.Limit,
                        total_pages = result.Pagination.TotalPages
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar sesiones de sincronización");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// GET /api/sync/stats
        /// Obtiene estadísticas de sincronización de la empresa
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetSyncStats([FromQuery] int days = 30)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();

                var query = new GetSyncStatsQuery
                {
                    Days = days,
                    EmpresaPrincipalId = empresaId
                };

                var result = await _mediator.Send(query);

                return Ok(new
                {
                    success = true,
                    empresa = result.Empresa,
                    periodo_dias = result.PeriodoDias,
                    estadisticas = new
                    {
                        total_syncs = result.Estadisticas.TotalSyncs,
                        productos_totales = result.Estadisticas.ProductosTotales,
                        productos_actualizados = result.Estadisticas.ProductosActualizados,
                        productos_nuevos = result.Estadisticas.ProductosNuevos,
                        errores_totales = result.Estadisticas.ErroresTotales,
                        tiempo_promedio_ms = result.Estadisticas.TiempoPromedioMs,
                        syncs_exitosos = result.Estadisticas.SyncsExitosos,
                        syncs_con_errores = result.Estadisticas.SyncsConErrores,
                        syncs_fallidos = result.Estadisticas.SyncsFallidos,
                        ultima_sincronizacion = result.Estadisticas.UltimaSincronizacion,
                        tasa_exito_promedio = result.Estadisticas.TasaExitoPromedio,
                        productos_por_segundo_promedio = result.Estadisticas.ProductosPorSegundoPromedio,
                        performance = new
                        {
                            tiempo_minimo_ms = result.Estadisticas.Performance.TiempoMinimoMs,
                            tiempo_maximo_ms = result.Estadisticas.Performance.TiempoMaximoMs,
                            sesiones_con_advertencias = result.Estadisticas.Performance.SessionesConAdvertencias,
                            productividad_promedio = result.Estadisticas.Performance.ProductivividadPromedio
                        },
                        estadisticas_diarias = result.Estadisticas.EstadisticasDiarias.Select(d => new
                        {
                            fecha = d.Fecha,
                            syncs = d.Syncs,
                            productos = d.Productos,
                            errores = d.Errores,
                            tiempo_promedio_ms = d.TiempoPromedioMs
                        }).ToList()
                    },
                    timestamp = result.Timestamp
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de sincronización");
                return HandleError(ex);
            }
        }

        /// <summary>
        /// DELETE /api/sync/sessions/cleanup
        /// Limpia sesiones antiguas completadas
        /// </summary>
        [HttpDelete("sessions/cleanup")]
        public async Task<IActionResult> CleanupOldSessions([FromQuery] int dias = 7)
        {
            try
            {
                var empresaId = GetEmpresaIdFromToken();

                var command = new CleanupOldSessionsCommand
                {
                    DiasAntiguedad = dias,
                    EmpresaPrincipalId = empresaId
                };

                var result = await _mediator.Send(command);

                return Ok(new
                {
                    success = true,
                    message = result.Mensaje,
                    sesiones_eliminadas = result.SesionesEliminadas,
                    dias_antiguedad = result.DiasAntiguedad
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en limpieza de sesiones");
                return HandleError(ex);
            }
        }

        #region Private Methods

        private int GetEmpresaIdFromToken()
        {
            // Verificar que es empresa principal
            var tipoEmpresaClaim = User.FindFirst("tipo_empresa")?.Value;
            if (tipoEmpresaClaim != "principal")
            {
                throw new UnauthorizedAccessException("Solo las empresas principales pueden realizar sincronización");
            }

            // Obtener el ID de la empresa principal (no el empresaId del usuario)
            var empresaPrincipalIdClaim = User.FindFirst("empresa_principal_id")?.Value;
            if (string.IsNullOrEmpty(empresaPrincipalIdClaim) || !int.TryParse(empresaPrincipalIdClaim, out var empresaPrincipalId))
            {
                throw new UnauthorizedAccessException("Token no contiene información válida de empresa principal");
            }

            return empresaPrincipalId;
        }

        private string GetClientIpAddress()
        {
            var xForwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xForwardedFor))
            {
                return xForwardedFor.Split(',')[0].Trim();
            }

            var xRealIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(xRealIp))
            {
                return xRealIp;
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private IActionResult HandleError(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                }),
                InvalidOperationException => BadRequest(new
                {
                    success = false,
                    message = ex.Message
                }),
                ArgumentException => BadRequest(new
                {
                    success = false,
                    message = ex.Message
                }),
                _ => StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    error = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                        ? ex.Message 
                        : null
                })
            };
        }

        #endregion

        #region DTOs

        public class StartSessionRequest
        {
            public int TotalLotesEsperados { get; set; }
            public string UsuarioProceso { get; set; }
            public string? ListaCodigo { get; set; } // Opcional: código de lista de precios
            public bool MultiLista { get; set; } = false; // NUEVO: Flag para modo multi-lista
        }

        public class ProcessBulkRequest
        {
            public Guid SessionId { get; set; }
            public int LoteNumero { get; set; }
            public ProductRequest[] Productos { get; set; }
            
            [JsonPropertyName("stock_only_mode")]
            public bool StockOnlyMode { get; set; } = false; // Flag para actualizar solo stock
        }

        public class ProductRequest
        {
            public string Codigo { get; set; }
            public string? Descripcion { get; set; } // Nullable para modo stock_only_mode
            public int? CategoriaId { get; set; }
            public decimal? Precio { get; set; } // Opcional para compatibilidad hacia atrás
            
            [JsonPropertyName("listas_precios")]
            public List<ProductPriceRequest>? ListasPrecios { get; set; } // NUEVO: Array de precios para múltiples listas
            
            public decimal Stock { get; set; } // Mantenido para compatibilidad
            
            [JsonPropertyName("stocks_por_empresa")]
            public List<StockPorEmpresaRequest>? StocksPorEmpresa { get; set; } // NUEVO: Stock diferenciado por empresa
            
            public int? Grupo1 { get; set; }
            public int? Grupo2 { get; set; }
            public int? Grupo3 { get; set; }
            public DateTime? FechaAlta { get; set; }
            public DateTime? FechaModi { get; set; }
            public string? Imputable { get; set; }
            public string? Disponible { get; set; }
            public string? CodigoUbicacion { get; set; }
        }

        public class ProductPriceRequest
        {
            [JsonPropertyName("listaId")]
            public int ListaId { get; set; }
            
            [JsonPropertyName("precio")]
            public decimal Precio { get; set; }
            
            [JsonPropertyName("fecha")]
            public DateTime? Fecha { get; set; }
        }

        public class StockPorEmpresaRequest
        {
            [JsonPropertyName("empresa_id")]
            public int EmpresaId { get; set; }
            
            [JsonPropertyName("stock")]
            public decimal Stock { get; set; }
        }

        public class FinishSessionRequest
        {
            public string Estado { get; set; }
        }

        #endregion
    }
}