using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.ValueObjects;
using DistriCatalogoAPI.Infrastructure.Models;
using AutoMapper;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class SyncSessionRepository : ISyncSessionRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SyncSessionRepository> _logger;

        public SyncSessionRepository(
            DistricatalogoContext context,
            IMapper mapper,
            ILogger<SyncSessionRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Domain.Entities.SyncSession?> GetByIdAsync(Guid id)
        {
            try
            {
                var syncSessionModel = await _context.SyncSessions
                    .FirstOrDefaultAsync(s => s.Id == id);

                return syncSessionModel != null ? MapToDomain(syncSessionModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión de sync por ID {Id}", id);
                throw;
            }
        }

        public async Task<Domain.Entities.SyncSession?> GetByIdWithCompanyAsync(Guid id)
        {
            try
            {
                var syncSessionModel = await _context.SyncSessions
                    .Include(s => s.EmpresaPrincipal)
                    .FirstOrDefaultAsync(s => s.Id == id);

                return syncSessionModel != null ? MapToDomainWithCompany(syncSessionModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión de sync con empresa por ID {Id}", id);
                throw;
            }
        }

        public async Task<List<Domain.Entities.SyncSession>> GetByCompanyAsync(int empresaId, int page, int pageSize, string? estado = null)
        {
            try
            {
                var query = _context.SyncSessions
                    .Where(s => s.EmpresaPrincipalId == empresaId);

                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(s => s.Estado == estado);
                }

                var syncSessionModels = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return syncSessionModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones por empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<int> GetCountByCompanyAsync(int empresaId, string? estado = null)
        {
            try
            {
                var query = _context.SyncSessions
                    .Where(s => s.EmpresaPrincipalId == empresaId);

                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(s => s.Estado == estado);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar sesiones por empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<Domain.Entities.SyncSession> CreateAsync(Domain.Entities.SyncSession session)
        {
            try
            {
                _logger.LogDebug("Iniciando CreateAsync para sesión {SessionId}", session.Id);
                
                var syncSessionModel = MapToInfrastructure(session);
                _logger.LogDebug("MapToInfrastructure completado para sesión {SessionId}", session.Id);
                
                _context.SyncSessions.Add(syncSessionModel);
                _logger.LogDebug("Sesión agregada al contexto, guardando cambios...");
                
                await _context.SaveChangesAsync();
                _logger.LogDebug("SaveChangesAsync completado para sesión {SessionId}", syncSessionModel.Id);

                _logger.LogInformation("Sesión de sync creada: {Id}", syncSessionModel.Id);
                
                _logger.LogDebug("Iniciando MapToDomain para sesión {SessionId}", syncSessionModel.Id);
                var domainSession = MapToDomain(syncSessionModel);
                _logger.LogDebug("MapToDomain completado para sesión {SessionId}", syncSessionModel.Id);
                
                return domainSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear sesión de sync");
                throw;
            }
        }

        public async Task UpdateAsync(Domain.Entities.SyncSession session)
        {
            try
            {
                var existingModel = await _context.SyncSessions
                    .FirstOrDefaultAsync(s => s.Id == session.Id);

                if (existingModel == null)
                {
                    throw new InvalidOperationException($"Sesión de sync {session.Id} no encontrada para actualizar");
                }

                // Actualizar solo los campos que pueden cambiar
                existingModel.Estado = session.Estado.ToString();
                existingModel.LotesProcesados = session.LotesProcesados;
                existingModel.ProductosTotales = session.ProductosTotales;
                existingModel.ProductosActualizados = session.ProductosActualizados;
                existingModel.ProductosNuevos = session.ProductosNuevos;
                existingModel.ProductosErrores = session.ProductosErrores;
                existingModel.ErroresDetalle = SerializeErrorDetails(session.ErroresDetalle);
                existingModel.Metricas = SerializeMetrics(session.Metricas);
                existingModel.FechaFin = session.FechaFin;
                existingModel.TiempoTotalMs = session.TiempoTotalMs;
                existingModel.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogDebug("Sesión de sync actualizada: {Id}", session.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar sesión de sync {Id}", session.Id);
                throw;
            }
        }

        public async Task<int> CleanupOldSessionsAsync(int diasAntiguedad)
        {
            try
            {
                var fechaCorte = DateTime.UtcNow.AddDays(-diasAntiguedad);
                
                var sessionesToDelete = await _context.SyncSessions
                    .Where(s => s.CreatedAt < fechaCorte && 
                               (s.Estado == "completada" || s.Estado == "error" || s.Estado == "cancelada"))
                    .ToListAsync();

                if (sessionesToDelete.Any())
                {
                    _context.SyncSessions.RemoveRange(sessionesToDelete);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Eliminadas {Count} sesiones antiguas (>{Dias} días)", 
                        sessionesToDelete.Count, diasAntiguedad);
                }

                return sessionesToDelete.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante limpieza de sesiones antiguas");
                throw;
            }
        }

        public async Task<List<Domain.Entities.SyncSession>> GetActiveSessionsByCompanyAsync(int empresaId)
        {
            try
            {
                var activeStates = new[] { "iniciada", "procesando" };
                
                var syncSessionModels = await _context.SyncSessions
                    .Where(s => s.EmpresaPrincipalId == empresaId && 
                               activeStates.Contains(s.Estado))
                    .ToListAsync();

                return syncSessionModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones activas por empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<bool> ExistsActiveSessionAsync(int empresaId)
        {
            try
            {
                var activeStates = new[] { "iniciada", "procesando" };
                
                return await _context.SyncSessions
                    .AnyAsync(s => s.EmpresaPrincipalId == empresaId && 
                                  activeStates.Contains(s.Estado));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar sesiones activas por empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        private Domain.Entities.SyncSession MapToDomain(Infrastructure.Models.SyncSession model)
        {
            try
            {
                _logger.LogDebug("Iniciando MapToDomain para modelo con ID {Id}", model.Id);
                
                // Crear sesión usando constructor privado con reflection para mapear desde DB
                var session = Activator.CreateInstance(typeof(Domain.Entities.SyncSession), true) as Domain.Entities.SyncSession;
                
                if (session == null)
                {
                    throw new InvalidOperationException("No se pudo crear instancia de SyncSession usando reflection");
                }
                
                // Usar reflection para establecer propiedades con setter privado
                var sessionType = typeof(Domain.Entities.SyncSession);
                
                // Función helper para establecer propiedades con setter privado
                void SetProperty(string propertyName, object value)
                {
                    _logger.LogDebug("Intentando establecer propiedad {PropertyName} con valor {Value}", propertyName, value);
                    
                    var prop = sessionType.GetProperty(propertyName);
                    if (prop != null)
                    {
                        _logger.LogDebug("Propiedad {PropertyName} encontrada. CanWrite: {CanWrite}", propertyName, prop.CanWrite);
                        
                        if (prop.CanWrite)
                        {
                            prop.SetValue(session, value);
                            _logger.LogDebug("Propiedad {PropertyName} establecida directamente", propertyName);
                        }
                        else
                        {
                            // Si no se puede escribir directamente, intentar con backing field
                            var field = sessionType.GetField($"<{propertyName}>k__BackingField", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            
                            if (field != null)
                            {
                                field.SetValue(session, value);
                                _logger.LogDebug("Propiedad {PropertyName} establecida usando backing field", propertyName);
                            }
                            else
                            {
                                _logger.LogWarning("No se pudo encontrar backing field para propiedad {PropertyName}", propertyName);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Propiedad {PropertyName} no encontrada en tipo {TypeName}", propertyName, sessionType.Name);
                    }
                }
                
                SetProperty("Id", model.Id);
                SetProperty("EmpresaPrincipalId", model.EmpresaPrincipalId);
                SetProperty("ListaPrecioId", model.ListaPrecioId);
                
                // Crear el estado con validación extra
                try
                {
                    var estadoString = model.Estado ?? "iniciada";
                    _logger.LogDebug("Creando SessionState para valor: {Estado}", estadoString);
                    var estado = SessionState.From(estadoString);
                    SetProperty("Estado", estado);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creando SessionState para valor: {Estado}", model.Estado);
                    throw;
                }
                
                SetProperty("TotalLotesEsperados", model.TotalLotesEsperados ?? 0);
                SetProperty("LotesProcesados", model.LotesProcesados ?? 0);
                SetProperty("ProductosTotales", model.ProductosTotales ?? 0);
                SetProperty("ProductosActualizados", model.ProductosActualizados ?? 0);
                SetProperty("ProductosNuevos", model.ProductosNuevos ?? 0);
                SetProperty("ProductosErrores", model.ProductosErrores ?? 0);
                SetProperty("ErroresDetalle", DeserializeErrorDetails(model.ErroresDetalle));
                SetProperty("Metricas", DeserializeMetrics(model.Metricas));
                SetProperty("FechaInicio", model.FechaInicio ?? DateTime.UtcNow);
                SetProperty("FechaFin", model.FechaFin);
                SetProperty("TiempoTotalMs", model.TiempoTotalMs);
                SetProperty("UsuarioProceso", model.UsuarioProceso ?? "SISTEMA");
                SetProperty("IpOrigen", model.IpOrigen ?? "Unknown");

                // Establecer propiedades base
                SetProperty("CreatedAt", model.CreatedAt ?? DateTime.UtcNow);
                SetProperty("UpdatedAt", model.UpdatedAt ?? DateTime.UtcNow);

                // Validar que las propiedades críticas se establecieron correctamente
                _logger.LogDebug("Validando propiedades después del mapeo:");
                _logger.LogDebug("session.Id: {Id}", session.Id);
                _logger.LogDebug("session.EmpresaPrincipalId: {EmpresaId}", session.EmpresaPrincipalId);
                _logger.LogDebug("session.Estado: {Estado}", session.Estado?.ToString() ?? "NULL");
                _logger.LogDebug("session.TotalLotesEsperados: {TotalLotes}", session.TotalLotesEsperados);

                if (session.Id == Guid.Empty)
                {
                    _logger.LogError("El ID de la sesión sigue siendo Guid.Empty después del mapeo");
                    throw new InvalidOperationException("Error en el mapeo: el ID de la sesión no se estableció correctamente");
                }

                if (session.Estado == null)
                {
                    _logger.LogError("El Estado de la sesión sigue siendo null después del mapeo");
                    throw new InvalidOperationException("Error en el mapeo: el Estado de la sesión no se estableció correctamente");
                }

                _logger.LogDebug("MapToDomain completado exitosamente para sesión {Id}", session.Id);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en MapToDomain para modelo con ID {Id}", model.Id);
                throw;
            }
        }

        private Domain.Entities.SyncSession MapToDomainWithCompany(Infrastructure.Models.SyncSession model)
        {
            var session = MapToDomain(model);
            
            if (model.EmpresaPrincipal != null)
            {
                // Mapear la empresa si está incluida
                var companyProps = typeof(Domain.Entities.SyncSession).GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var empresaProperty = companyProps.FirstOrDefault(p => p.Name == "EmpresaPrincipal");
                
                if (empresaProperty != null)
                {
                    var company = _mapper.Map<Company>(model.EmpresaPrincipal);
                    empresaProperty.SetValue(session, company);
                }
            }

            return session;
        }

        private Infrastructure.Models.SyncSession MapToInfrastructure(Domain.Entities.SyncSession domain)
        {
            return new Infrastructure.Models.SyncSession
            {
                Id = domain.Id,
                EmpresaPrincipalId = domain.EmpresaPrincipalId,
                ListaPrecioId = domain.ListaPrecioId,
                Estado = domain.Estado.ToString(),
                TotalLotesEsperados = domain.TotalLotesEsperados,
                LotesProcesados = domain.LotesProcesados,
                ProductosTotales = domain.ProductosTotales,
                ProductosActualizados = domain.ProductosActualizados,
                ProductosNuevos = domain.ProductosNuevos,
                ProductosErrores = domain.ProductosErrores,
                ErroresDetalle = SerializeErrorDetails(domain.ErroresDetalle),
                Metricas = SerializeMetrics(domain.Metricas),
                FechaInicio = domain.FechaInicio,
                FechaFin = domain.FechaFin,
                TiempoTotalMs = domain.TiempoTotalMs,
                UsuarioProceso = domain.UsuarioProceso,
                IpOrigen = domain.IpOrigen,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }

        private string SerializeErrorDetails(List<Domain.Entities.SyncSession.ProductError> errors)
        {
            if (errors?.Any() != true) return null;
            
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(errors);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error serializando detalles de errores");
                return null;
            }
        }

        private List<Domain.Entities.SyncSession.ProductError> DeserializeErrorDetails(string json)
        {
            if (string.IsNullOrEmpty(json)) return new List<Domain.Entities.SyncSession.ProductError>();
            
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<Domain.Entities.SyncSession.ProductError>>(json) 
                       ?? new List<Domain.Entities.SyncSession.ProductError>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deserializando detalles de errores");
                return new List<Domain.Entities.SyncSession.ProductError>();
            }
        }

        private string SerializeMetrics(SyncMetrics metrics)
        {
            if (metrics == null) return null;
            
            try
            {
                return System.Text.Json.JsonSerializer.Serialize(metrics.ToJson());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error serializando métricas");
                return null;
            }
        }

        private SyncMetrics DeserializeMetrics(string json)
        {
            if (string.IsNullOrEmpty(json)) return new SyncMetrics();
            
            try
            {
                var metricsData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                var metrics = new SyncMetrics();
                
                if (metricsData?.ContainsKey("total_productos_procesados") == true)
                {
                    metrics.TotalProductosProcesados = Convert.ToInt32(metricsData["total_productos_procesados"]);
                }
                
                // Reconstruir tiempos de procesamiento si están disponibles
                if (metricsData?.ContainsKey("tiempo_total_ms") == true && metricsData.ContainsKey("cantidad_lotes"))
                {
                    var tiempoTotal = Convert.ToInt32(metricsData["tiempo_total_ms"]);
                    var cantidadLotes = Convert.ToInt32(metricsData["cantidad_lotes"]);
                    
                    // Si tenemos lotes lentos, podemos reconstruir mejor
                    if (metricsData.ContainsKey("lotes_lentos") && metricsData["lotes_lentos"] is JsonElement lotesLentosElement && lotesLentosElement.ValueKind == JsonValueKind.Array)
                    {
                        // Reconstruir con información de lotes lentos
                        if (cantidadLotes > 0)
                        {
                            // Distribuir el tiempo proporcionalmente entre los lotes
                            var tiempoPorLote = cantidadLotes > 0 ? tiempoTotal / cantidadLotes : tiempoTotal;
                            for (int i = 0; i < cantidadLotes; i++)
                            {
                                metrics.AgregarTiempoProcesamiento(tiempoPorLote);
                            }
                        }
                    }
                    else if (cantidadLotes > 0)
                    {
                        // Reconstruir distribuyendo el tiempo uniformemente
                        var tiempoPorLote = tiempoTotal / cantidadLotes;
                        for (int i = 0; i < cantidadLotes; i++)
                        {
                            metrics.AgregarTiempoProcesamiento(tiempoPorLote);
                        }
                    }
                    else if (tiempoTotal > 0)
                    {
                        // Si no hay información de lotes, agregar el tiempo total como un solo lote
                        metrics.AgregarTiempoProcesamiento(tiempoTotal);
                    }
                }
                
                _logger.LogDebug("Métricas deserializadas: TotalProductos={Total}, TiempoTotal={Tiempo}, Lotes={Lotes}", 
                    metrics.TotalProductosProcesados, metrics.TiempoTotalMs, metrics.CantidadLotes);
                
                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deserializando métricas: {Error}", ex.Message);
                return new SyncMetrics();
            }
        }
    }
}