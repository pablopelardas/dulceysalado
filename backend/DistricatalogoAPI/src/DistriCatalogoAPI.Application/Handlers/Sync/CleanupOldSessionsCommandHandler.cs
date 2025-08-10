using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Sync;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Sync
{
    public class CleanupOldSessionsCommandHandler : IRequestHandler<CleanupOldSessionsCommand, CleanupOldSessionsResult>
    {
        private readonly ISyncSessionRepository _syncSessionRepository;
        private readonly ILogger<CleanupOldSessionsCommandHandler> _logger;

        public CleanupOldSessionsCommandHandler(
            ISyncSessionRepository syncSessionRepository,
            ILogger<CleanupOldSessionsCommandHandler> logger)
        {
            _syncSessionRepository = syncSessionRepository;
            _logger = logger;
        }

        public async Task<CleanupOldSessionsResult> Handle(CleanupOldSessionsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando limpieza de sesiones antiguas: {Dias} días para empresa {EmpresaId}", 
                request.DiasAntiguedad, request.EmpresaPrincipalId);

            try
            {
                var sesionesEliminadas = await _syncSessionRepository.CleanupOldSessionsAsync(request.DiasAntiguedad);

                _logger.LogInformation("Limpieza completada: {SesionesEliminadas} sesiones eliminadas", sesionesEliminadas);

                return new CleanupOldSessionsResult
                {
                    SesionesEliminadas = sesionesEliminadas,
                    DiasAntiguedad = request.DiasAntiguedad,
                    Mensaje = $"Limpieza de sesiones completada: {sesionesEliminadas} sesiones eliminadas"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante limpieza de sesiones para empresa {EmpresaId}", request.EmpresaPrincipalId);
                throw;
            }
        }
    }
}