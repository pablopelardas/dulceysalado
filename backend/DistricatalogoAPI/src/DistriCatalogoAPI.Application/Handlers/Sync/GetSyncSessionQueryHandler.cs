using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.Sync;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Sync
{
    public class GetSyncSessionQueryHandler : IRequestHandler<GetSyncSessionQuery, GetSyncSessionResult>
    {
        private readonly ISyncSessionRepository _syncSessionRepository;
        private readonly ILogger<GetSyncSessionQueryHandler> _logger;

        public GetSyncSessionQueryHandler(
            ISyncSessionRepository syncSessionRepository,
            ILogger<GetSyncSessionQueryHandler> logger)
        {
            _syncSessionRepository = syncSessionRepository;
            _logger = logger;
        }

        public async Task<GetSyncSessionResult> Handle(GetSyncSessionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var session = await _syncSessionRepository.GetByIdWithCompanyAsync(request.SessionId);
                
                if (session == null)
                {
                    throw new InvalidOperationException("Sesión de sincronización no encontrada");
                }

                // Verificar que la sesión pertenece a la empresa
                if (session.EmpresaPrincipalId != request.EmpresaPrincipalId)
                {
                    throw new InvalidOperationException("La sesión no pertenece a la empresa actual");
                }

                var progreso = session.GetProgreso();

                var productosPerSecond = session.TiempoTotalMs.HasValue && session.TiempoTotalMs > 0
                    ? session.ProductosTotales / (session.TiempoTotalMs.Value / 1000.0)
                    : 0;

                return new GetSyncSessionResult
                {
                    Id = session.Id,
                    Estado = session.Estado.ToString(),
                    Empresa = session.EmpresaPrincipal?.Nombre ?? "N/A",
                    FechaInicio = session.FechaInicio,
                    FechaFin = session.FechaFin,
                    UsuarioProceso = session.UsuarioProceso,
                    Progreso = new SessionProgress
                    {
                        Porcentaje = progreso.Porcentaje,
                        LotesProcesados = progreso.LotesProcesados,
                        TotalLotesEsperados = progreso.TotalLotesEsperados,
                        ProductosProcesados = progreso.ProductosProcesados,
                        Estado = progreso.Estado
                    },
                    Metricas = new SessionMetrics
                    {
                        ProductosTotales = session.ProductosTotales,
                        ProductosActualizados = session.ProductosActualizados,
                        ProductosNuevos = session.ProductosNuevos,
                        ProductosErrores = session.ProductosErrores,
                        TiempoTotalMs = session.TiempoTotalMs ?? 0,
                        ProductosPorSegundo = Math.Round(productosPerSecond, 2),
                        TiempoPromedioMs = session.Metricas.TiempoPromedioMs
                    },
                    ErroresDetalle = new SessionErrors
                    {
                        TotalErrores = session.ProductosErrores,
                        DetallesErrores = session.HasErrors() ? session.ErroresDetalle : null
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de sesión {SessionId}", request.SessionId);
                throw;
            }
        }
    }
}