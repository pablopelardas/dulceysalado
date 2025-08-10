using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;
using AutoMapper;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class SyncLogRepository : ISyncLogRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SyncLogRepository> _logger;

        public SyncLogRepository(
            DistricatalogoContext context,
            IMapper mapper,
            ILogger<SyncLogRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Domain.Entities.SyncLog> CreateAsync(Domain.Entities.SyncLog log)
        {
            try
            {
                var syncLogModel = MapToInfrastructure(log);
                
                _context.SyncLogs.Add(syncLogModel);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Log de sync creado: {Id} para empresa {EmpresaId}", 
                    syncLogModel.Id, syncLogModel.EmpresaPrincipalId);
                
                return MapToDomain(syncLogModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear log de sync");
                throw;
            }
        }

        public async Task<List<Domain.Entities.SyncLog>> GetByCompanyAsync(int empresaId, DateTime desde, DateTime hasta)
        {
            try
            {
                var syncLogModels = await _context.SyncLogs
                    .Where(log => log.EmpresaPrincipalId == empresaId &&
                                 log.FechaProcesamiento >= desde &&
                                 log.FechaProcesamiento <= hasta)
                    .OrderByDescending(log => log.FechaProcesamiento)
                    .ToListAsync();

                return syncLogModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs por empresa {EmpresaId} entre {Desde} y {Hasta}", 
                    empresaId, desde, hasta);
                throw;
            }
        }

        public async Task<SyncStats> GetStatsAsync(int empresaId, int dias)
        {
            try
            {
                var fechaInicio = DateTime.UtcNow.AddDays(-dias);

                var logs = await _context.SyncLogs
                    .Where(log => log.EmpresaPrincipalId == empresaId &&
                                 log.FechaProcesamiento >= fechaInicio)
                    .ToListAsync();

                if (!logs.Any())
                {
                    return new SyncStats
                    {
                        TotalSyncs = 0,
                        ProductosTotales = 0,
                        ProductosActualizados = 0,
                        ProductosNuevos = 0,
                        ErroresTotales = 0,
                        TiempoPromedioMs = 0,
                        SyncsExitosos = 0,
                        SyncsConErrores = 0,
                        SyncsFallidos = 0,
                        UltimaSincronizacion = null,
                        TasaExitoPromedio = 0
                    };
                }

                var stats = new SyncStats
                {
                    TotalSyncs = logs.Count,
                    ProductosTotales = logs.Sum(log => (log.ProductosActualizados ?? 0) + (log.ProductosNuevos ?? 0)),
                    ProductosActualizados = logs.Sum(log => log.ProductosActualizados ?? 0),
                    ProductosNuevos = logs.Sum(log => log.ProductosNuevos ?? 0),
                    ErroresTotales = logs.Sum(log => log.Errores ?? 0),
                    TiempoPromedioMs = logs.Any() 
                        ? logs.Average(log => log.TiempoProcesamientoMs ?? 0) 
                        : 0,
                    SyncsExitosos = logs.Count(log => log.Estado == "exitoso"),
                    SyncsConErrores = logs.Count(log => log.Estado == "con_errores"),
                    SyncsFallidos = logs.Count(log => log.Estado == "fallido"),
                    UltimaSincronizacion = logs.Max(log => log.FechaProcesamiento),
                    TasaExitoPromedio = CalcularTasaExitoPromedio(logs)
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<Domain.Entities.SyncLog>> GetRecentLogsAsync(int empresaId, int cantidad = 10)
        {
            try
            {
                var syncLogModels = await _context.SyncLogs
                    .Where(log => log.EmpresaPrincipalId == empresaId)
                    .OrderByDescending(log => log.FechaProcesamiento)
                    .Take(cantidad)
                    .ToListAsync();

                return syncLogModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs recientes para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<Domain.Entities.SyncLog>> GetByCompanyAsync(int empresaId, int page, int limit)
        {
            try
            {
                var skip = (page - 1) * limit;
                
                var syncLogModels = await _context.SyncLogs
                    .Where(log => log.EmpresaPrincipalId == empresaId)
                    .OrderByDescending(log => log.FechaProcesamiento)
                    .Skip(skip)
                    .Take(limit)
                    .ToListAsync();

                return syncLogModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs paginados para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<int> GetCountByCompanyAsync(int empresaId)
        {
            try
            {
                return await _context.SyncLogs
                    .Where(log => log.EmpresaPrincipalId == empresaId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar logs para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<int> GetErrorCountAsync(int empresaId, DateTime desde)
        {
            try
            {
                return await _context.SyncLogs
                    .Where(log => log.EmpresaPrincipalId == empresaId &&
                                 log.FechaProcesamiento >= desde &&
                                 log.Errores > 0)
                    .SumAsync(log => log.Errores ?? 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar errores para empresa {EmpresaId} desde {Desde}", 
                    empresaId, desde);
                throw;
            }
        }

        private double CalcularTasaExitoPromedio(List<Infrastructure.Models.SyncLog> logs)
        {
            if (!logs.Any()) return 0;

            var tasasExito = logs.Select(log =>
            {
                var total = (log.ProductosActualizados ?? 0) + (log.ProductosNuevos ?? 0) + (log.Errores ?? 0);
                if (total == 0) return 100.0;
                
                var exitosos = (log.ProductosActualizados ?? 0) + (log.ProductosNuevos ?? 0);
                return (exitosos * 100.0) / total;
            }).ToList();

            return tasasExito.Average();
        }

        private Domain.Entities.SyncLog MapToDomain(Infrastructure.Models.SyncLog model)
        {
            // Usar reflection para crear instancia y establecer propiedades privadas usando backing fields
            var syncLog = Activator.CreateInstance(typeof(Domain.Entities.SyncLog), true) as Domain.Entities.SyncLog;
            var syncLogType = typeof(Domain.Entities.SyncLog);

            _logger.LogDebug("Mapeando SyncLog desde DB: Id={Id}, Archivo={Archivo}, Fecha={Fecha}", 
                model.Id, model.ArchivoNombre, model.FechaProcesamiento);

            void SetProperty(string propertyName, object value)
            {
                _logger.LogDebug("Intentando establecer propiedad {PropertyName} con valor {Value}", propertyName, value);
                var prop = syncLogType.GetProperty(propertyName);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(syncLog, value);
                    _logger.LogDebug("Propiedad {PropertyName} establecida directamente", propertyName);
                }
                else
                {
                    var field = syncLogType.GetField($"<{propertyName}>k__BackingField", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(syncLog, value);
                        _logger.LogDebug("Propiedad {PropertyName} establecida via backing field", propertyName);
                    }
                    else
                    {
                        _logger.LogWarning("No se pudo establecer propiedad {PropertyName}", propertyName);
                    }
                }
            }

            SetProperty("Id", model.Id);
            SetProperty("EmpresaPrincipalId", model.EmpresaPrincipalId);
            SetProperty("ArchivoNombre", model.ArchivoNombre ?? "");
            SetProperty("FechaProcesamiento", model.FechaProcesamiento ?? DateTime.UtcNow);
            SetProperty("ProductosActualizados", model.ProductosActualizados ?? 0);
            SetProperty("ProductosNuevos", model.ProductosNuevos ?? 0);
            SetProperty("Errores", model.Errores ?? 0);
            SetProperty("TiempoProcesamientoMs", model.TiempoProcesamientoMs ?? 0);
            SetProperty("Estado", model.Estado);
            SetProperty("DetallesErrores", DeserializeErrorDetails(model.DetallesErrores));
            SetProperty("UsuarioProceso", model.UsuarioProceso ?? "SISTEMA");

            // Establecer propiedades base
            SetProperty("CreatedAt", model.FechaProcesamiento ?? DateTime.UtcNow);
            SetProperty("UpdatedAt", model.FechaProcesamiento ?? DateTime.UtcNow);

            _logger.LogDebug("SyncLog mapeado exitosamente: Id={Id}", syncLog?.Id);

            return syncLog;
        }

        private Infrastructure.Models.SyncLog MapToInfrastructure(Domain.Entities.SyncLog domain)
        {
            return new Infrastructure.Models.SyncLog
            {
                EmpresaPrincipalId = domain.EmpresaPrincipalId,
                ArchivoNombre = domain.ArchivoNombre,
                FechaProcesamiento = domain.FechaProcesamiento,
                ProductosActualizados = domain.ProductosActualizados,
                ProductosNuevos = domain.ProductosNuevos,
                Errores = domain.Errores,
                TiempoProcesamientoMs = domain.TiempoProcesamientoMs,
                Estado = domain.Estado.ToString().ToLower(),
                DetallesErrores = SerializeErrorDetails(domain.DetallesErrores),
                UsuarioProceso = domain.UsuarioProceso
                // No mapeamos CreatedAt/UpdatedAt porque la tabla sync_logs no las tiene
            };
        }


        private string SerializeErrorDetails(List<Domain.Entities.SyncLog.ErrorDetail> errorDetails)
        {
            if (errorDetails?.Any() != true) return null;

            try
            {
                return System.Text.Json.JsonSerializer.Serialize(errorDetails);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error serializando detalles de errores de SyncLog");
                return null;
            }
        }

        private List<Domain.Entities.SyncLog.ErrorDetail> DeserializeErrorDetails(string json)
        {
            if (string.IsNullOrEmpty(json)) return new List<Domain.Entities.SyncLog.ErrorDetail>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<Domain.Entities.SyncLog.ErrorDetail>>(json)
                       ?? new List<Domain.Entities.SyncLog.ErrorDetail>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error deserializando detalles de errores de SyncLog");
                return new List<Domain.Entities.SyncLog.ErrorDetail>();
            }
        }
    }
}