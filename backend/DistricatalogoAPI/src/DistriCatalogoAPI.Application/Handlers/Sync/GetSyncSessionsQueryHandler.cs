using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.Sync;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Sync
{
    public class GetSyncSessionsQueryHandler : IRequestHandler<GetSyncSessionsQuery, GetSyncSessionsResult>
    {
        private readonly ISyncSessionRepository _syncSessionRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<GetSyncSessionsQueryHandler> _logger;

        public GetSyncSessionsQueryHandler(
            ISyncSessionRepository syncSessionRepository,
            ICompanyRepository companyRepository,
            ILogger<GetSyncSessionsQueryHandler> logger)
        {
            _syncSessionRepository = syncSessionRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<GetSyncSessionsResult> Handle(GetSyncSessionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar que la empresa existe
                var empresa = await _companyRepository.GetByIdAsync(request.EmpresaPrincipalId);
                if (empresa == null)
                {
                    throw new InvalidOperationException("Empresa no encontrada");
                }

                // Obtener sesiones con paginación
                var sessions = await _syncSessionRepository.GetByCompanyAsync(
                    request.EmpresaPrincipalId, 
                    request.Page, 
                    request.Limit, 
                    request.Estado);

                var totalCount = await _syncSessionRepository.GetCountByCompanyAsync(
                    request.EmpresaPrincipalId, 
                    request.Estado);

                var sessionItems = sessions.Select(session =>
                {
                    var progreso = session.GetProgreso();
                    
                    return new SyncSessionItem
                    {
                        Id = session.Id,
                        Estado = session.Estado.ToString(),
                        FechaInicio = session.FechaInicio,
                        FechaFin = session.FechaFin,
                        UsuarioProceso = session.UsuarioProceso,
                        Empresa = empresa.Nombre,
                        ProductosTotales = session.ProductosTotales,
                        ProductosErrores = session.ProductosErrores,
                        TiempoTotalMs = session.TiempoTotalMs ?? 0,
                        Progreso = new SessionProgressList
                        {
                            Porcentaje = progreso.Porcentaje,
                            LotesProcesados = progreso.LotesProcesados,
                            TotalLotesEsperados = progreso.TotalLotesEsperados,
                            Estado = progreso.Estado
                        }
                    };
                }).ToList();

                return new GetSyncSessionsResult
                {
                    Sessions = sessionItems,
                    Pagination = new PaginationInfo
                    {
                        Total = totalCount,
                        Page = request.Page,
                        Limit = request.Limit,
                        TotalPages = (int)Math.Ceiling((double)totalCount / request.Limit)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar sesiones para empresa {EmpresaId}", request.EmpresaPrincipalId);
                throw;
            }
        }
    }
}