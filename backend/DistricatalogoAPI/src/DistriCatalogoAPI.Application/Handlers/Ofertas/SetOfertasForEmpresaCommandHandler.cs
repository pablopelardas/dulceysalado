using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Ofertas;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Ofertas
{
    public class SetOfertasForEmpresaCommandHandler : IRequestHandler<SetOfertasForEmpresaCommand, bool>
    {
        private readonly IEmpresaOfertaRepository _empresaOfertaRepository;
        private readonly ILogger<SetOfertasForEmpresaCommandHandler> _logger;

        public SetOfertasForEmpresaCommandHandler(
            IEmpresaOfertaRepository empresaOfertaRepository,
            ILogger<SetOfertasForEmpresaCommandHandler> logger)
        {
            _empresaOfertaRepository = empresaOfertaRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(SetOfertasForEmpresaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _empresaOfertaRepository.SetOfertasForEmpresaAsync(request.EmpresaId, request.AgrupacionIds);

                _logger.LogInformation("Ofertas actualizadas exitosamente para EmpresaId {EmpresaId}. Total: {Count}",
                    request.EmpresaId, request.AgrupacionIds.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar ofertas para EmpresaId {EmpresaId}", request.EmpresaId);
                throw;
            }
        }
    }
}