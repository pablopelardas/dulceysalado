using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Novedades;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Novedades
{
    public class SetNovedadesForEmpresaCommandHandler : IRequestHandler<SetNovedadesForEmpresaCommand, bool>
    {
        private readonly IEmpresaNovedadRepository _empresaNovedadRepository;
        private readonly ILogger<SetNovedadesForEmpresaCommandHandler> _logger;

        public SetNovedadesForEmpresaCommandHandler(
            IEmpresaNovedadRepository empresaNovedadRepository,
            ILogger<SetNovedadesForEmpresaCommandHandler> logger)
        {
            _empresaNovedadRepository = empresaNovedadRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(SetNovedadesForEmpresaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _empresaNovedadRepository.SetNovedadesForEmpresaAsync(request.EmpresaId, request.AgrupacionIds);

                _logger.LogInformation("Novedades actualizadas exitosamente para EmpresaId {EmpresaId}. Total: {Count}",
                    request.EmpresaId, request.AgrupacionIds.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar novedades para EmpresaId {EmpresaId}", request.EmpresaId);
                throw;
            }
        }
    }
}