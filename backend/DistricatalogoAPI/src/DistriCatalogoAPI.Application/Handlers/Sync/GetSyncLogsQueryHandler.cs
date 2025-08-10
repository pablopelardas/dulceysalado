using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.Sync;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Sync
{
    public class GetSyncLogsQueryHandler : IRequestHandler<GetSyncLogsQuery, GetSyncLogsResult>
    {
        private readonly ISyncLogRepository _syncLogRepository;
        private readonly ILogger<GetSyncLogsQueryHandler> _logger;

        public GetSyncLogsQueryHandler(
            ISyncLogRepository syncLogRepository,
            ILogger<GetSyncLogsQueryHandler> logger)
        {
            _syncLogRepository = syncLogRepository;
            _logger = logger;
        }

        public async Task<GetSyncLogsResult> Handle(GetSyncLogsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Obteniendo logs de sync para empresa {EmpresaId}, página {Page}, límite {Limit}", 
                request.EmpresaPrincipalId, request.Page, request.Limit);

            try
            {
                var logs = await _syncLogRepository.GetByCompanyAsync(request.EmpresaPrincipalId, request.Page, request.Limit);
                var totalCount = await _syncLogRepository.GetCountByCompanyAsync(request.EmpresaPrincipalId);

                var result = new GetSyncLogsResult
                {
                    Logs = logs.Select(log => new SyncLogDto
                    {
                        Id = log.Id,
                        ArchivoNombre = log.ArchivoNombre,
                        FechaProcesamiento = log.FechaProcesamiento,
                        ProductosActualizados = log.ProductosActualizados,
                        ProductosNuevos = log.ProductosNuevos,
                        Errores = log.Errores,
                        TiempoProcesamientoMs = log.TiempoProcesamientoMs,
                        Estado = log.Estado.ToString(),
                        UsuarioProceso = log.UsuarioProceso
                    }).ToList(),
                    TotalCount = totalCount,
                    Page = request.Page,
                    Limit = request.Limit
                };

                _logger.LogDebug("Encontrados {LogCount} logs de {TotalCount} total", result.Logs.Count, totalCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo logs de sync para empresa {EmpresaId}", request.EmpresaPrincipalId);
                throw;
            }
        }
    }
}