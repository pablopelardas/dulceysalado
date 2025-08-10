using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Sync;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Sync
{
    public class FinishSyncSessionCommandHandler : IRequestHandler<FinishSyncSessionCommand, FinishSyncSessionResult>
    {
        private readonly ISyncSessionRepository _syncSessionRepository;
        private readonly ISyncLogRepository _syncLogRepository;
        private readonly ILogger<FinishSyncSessionCommandHandler> _logger;

        public FinishSyncSessionCommandHandler(
            ISyncSessionRepository syncSessionRepository,
            ISyncLogRepository syncLogRepository,
            ILogger<FinishSyncSessionCommandHandler> logger)
        {
            _syncSessionRepository = syncSessionRepository;
            _syncLogRepository = syncLogRepository;
            _logger = logger;
        }

        public async Task<FinishSyncSessionResult> Handle(FinishSyncSessionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Finalizando sesión de sincronización {SessionId} con estado {Estado}", 
                request.SessionId, request.Estado);

            try
            {
                // Buscar y validar la sesión
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

                // Verificar que la sesión puede ser finalizada
                if (!session.CanFinish())
                {
                    throw new InvalidOperationException($"No se puede finalizar sesión en estado {session.Estado}");
                }

                // Finalizar la sesión
                session.Finalizar(request.Estado);
                await _syncSessionRepository.UpdateAsync(session);

                // Crear log detallado para auditoría
                _logger.LogDebug("Creando SyncLog para sesión {SessionId} con {ProductosTotales} productos, {Errores} errores", 
                    session.Id, session.ProductosTotales, session.ProductosErrores);
                
                var syncLog = SyncLog.CreateFromSession(session);
                await _syncLogRepository.CreateAsync(syncLog);
                
                _logger.LogInformation("SyncLog creado exitosamente para sesión {SessionId} - ID Log: {LogId}", 
                    session.Id, syncLog.Id);

                // Calcular métricas finales
                var tasaExito = session.ProductosTotales > 0 
                    ? ((session.ProductosActualizados + session.ProductosNuevos) * 100.0 / session.ProductosTotales)
                    : 100.0;

                var productosPerSecond = session.TiempoTotalMs.HasValue && session.TiempoTotalMs > 0
                    ? session.ProductosTotales / (session.TiempoTotalMs.Value / 1000.0)
                    : 0;

                _logger.LogInformation("Sesión {SessionId} finalizada: {Estado} - {ProductosTotales} productos, {Errores} errores, {TiempoMs}ms",
                    session.Id, session.Estado, session.ProductosTotales, session.ProductosErrores, session.TiempoTotalMs);

                return new FinishSyncSessionResult
                {
                    SessionId = session.Id,
                    EstadoFinal = session.Estado.ToString(),
                    FechaFin = session.FechaFin.Value,
                    Resumen = new SessionSummary
                    {
                        ProductosTotales = session.ProductosTotales,
                        ProductosActualizados = session.ProductosActualizados,
                        ProductosNuevos = session.ProductosNuevos,
                        ProductosErrores = session.ProductosErrores,
                        LotesProcesados = session.LotesProcesados,
                        TiempoTotalMs = session.TiempoTotalMs ?? 0,
                        ProductosPorSegundo = Math.Round(productosPerSecond, 2),
                        TasaExito = Math.Round(tasaExito, 2)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar sesión {SessionId}", request.SessionId);
                throw;
            }
        }
    }
}