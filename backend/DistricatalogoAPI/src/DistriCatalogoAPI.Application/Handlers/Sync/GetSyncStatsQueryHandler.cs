using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.Sync;
using DistriCatalogoAPI.Domain.Interfaces;
using System.Linq;

namespace DistriCatalogoAPI.Application.Handlers.Sync
{
    public class GetSyncStatsQueryHandler : IRequestHandler<GetSyncStatsQuery, GetSyncStatsResult>
    {
        private readonly ISyncLogRepository _syncLogRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<GetSyncStatsQueryHandler> _logger;

        public GetSyncStatsQueryHandler(
            ISyncLogRepository syncLogRepository,
            ICompanyRepository companyRepository,
            ILogger<GetSyncStatsQueryHandler> logger)
        {
            _syncLogRepository = syncLogRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<GetSyncStatsResult> Handle(GetSyncStatsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar que la empresa existe
                var empresa = await _companyRepository.GetByIdAsync(request.EmpresaPrincipalId);
                if (empresa == null)
                {
                    throw new InvalidOperationException("Empresa no encontrada");
                }

                // Obtener estadísticas del repositorio
                var stats = await _syncLogRepository.GetStatsAsync(request.EmpresaPrincipalId, request.Days);
                
                if (stats == null)
                {
                    return CreateEmptyStats(empresa.Nombre, request.Days);
                }

                // Obtener logs recientes para estadísticas diarias
                var fechaInicio = DateTime.UtcNow.AddDays(-request.Days);
                var logsRecientes = await _syncLogRepository.GetByCompanyAsync(
                    request.EmpresaPrincipalId, 
                    fechaInicio, 
                    DateTime.UtcNow);

                // Agrupar por día para estadísticas diarias
                var estadisticasDiarias = logsRecientes
                    .GroupBy(log => log.FechaProcesamiento.Date)
                    .Select(group => new DailyStats
                    {
                        Fecha = group.Key,
                        Syncs = group.Count(),
                        Productos = group.Sum(log => log.ProductosActualizados + log.ProductosNuevos),
                        Errores = group.Sum(log => log.Errores),
                        TiempoPromedioMs = group.Any() ? group.Average(log => log.TiempoProcesamientoMs) : 0
                    })
                    .OrderBy(s => s.Fecha)
                    .ToList();

                // Calcular métricas de performance
                var tiemposMs = logsRecientes
                    .Where(log => log.TiempoProcesamientoMs > 0)
                    .Select(log => log.TiempoProcesamientoMs)
                    .ToList();

                var performance = new PerformanceMetrics();
                if (tiemposMs.Any())
                {
                    performance.TiempoMinimoMs = tiemposMs.Min();
                    performance.TiempoMaximoMs = tiemposMs.Max();
                    performance.SessionesConAdvertencias = tiemposMs.Count(t => t > 10000); // > 10 segundos
                    
                    var productividad = logsRecientes
                        .Where(log => log.TiempoProcesamientoMs > 0)
                        .Select(log => (log.ProductosActualizados + log.ProductosNuevos) / (log.TiempoProcesamientoMs / 1000.0))
                        .ToList();
                    
                    performance.ProductivividadPromedio = productividad.Any() ? productividad.Average() : 0;
                }

                // Calcular tasa de éxito promedio
                var tasaExitoPromedio = logsRecientes.Any()
                    ? logsRecientes.Average(log => log.GetTasaExito())
                    : 0;

                // Calcular productos por segundo promedio
                var productosPerSegundoPromedio = logsRecientes
                    .Where(log => log.TiempoProcesamientoMs > 0)
                    .Select(log => log.GetProductosPorSegundo())
                    .Where(pps => pps > 0)
                    .DefaultIfEmpty(0)
                    .Average();

                return new GetSyncStatsResult
                {
                    Empresa = empresa.Nombre,
                    PeriodoDias = request.Days,
                    Timestamp = DateTime.UtcNow,
                    Estadisticas = new Queries.Sync.SyncStatistics
                    {
                        TotalSyncs = stats.TotalSyncs,
                        ProductosTotales = stats.ProductosTotales,
                        ProductosActualizados = stats.ProductosActualizados,
                        ProductosNuevos = stats.ProductosNuevos,
                        ErroresTotales = stats.ErroresTotales,
                        TiempoPromedioMs = stats.TiempoPromedioMs,
                        SyncsExitosos = stats.SyncsExitosos,
                        SyncsConErrores = stats.SyncsConErrores,
                        SyncsFallidos = stats.SyncsFallidos,
                        UltimaSincronizacion = stats.UltimaSincronizacion,
                        TasaExitoPromedio = Math.Round(tasaExitoPromedio, 2),
                        ProductosPorSegundoPromedio = Math.Round(productosPerSegundoPromedio, 2),
                        Performance = performance,
                        EstadisticasDiarias = estadisticasDiarias
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas para empresa {EmpresaId}", request.EmpresaPrincipalId);
                throw;
            }
        }

        private GetSyncStatsResult CreateEmptyStats(string empresaNombre, int dias)
        {
            return new GetSyncStatsResult
            {
                Empresa = empresaNombre,
                PeriodoDias = dias,
                Timestamp = DateTime.UtcNow,
                Estadisticas = new Queries.Sync.SyncStatistics
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
                    TasaExitoPromedio = 0,
                    ProductosPorSegundoPromedio = 0,
                    Performance = new PerformanceMetrics(),
                    EstadisticasDiarias = new()
                }
            };
        }
    }
}