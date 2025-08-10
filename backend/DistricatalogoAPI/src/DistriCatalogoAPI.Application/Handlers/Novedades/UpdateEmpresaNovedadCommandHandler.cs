using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Novedades;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Novedades
{
    public class UpdateEmpresaNovedadCommandHandler : IRequestHandler<UpdateEmpresaNovedadCommand, bool>
    {
        private readonly IEmpresaNovedadRepository _empresaNovedadRepository;
        private readonly ILogger<UpdateEmpresaNovedadCommandHandler> _logger;

        public UpdateEmpresaNovedadCommandHandler(
            IEmpresaNovedadRepository empresaNovedadRepository,
            ILogger<UpdateEmpresaNovedadCommandHandler> logger)
        {
            _empresaNovedadRepository = empresaNovedadRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateEmpresaNovedadCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var novedad = await _empresaNovedadRepository.GetByIdAsync(request.Id);
                if (novedad == null)
                {
                    throw new InvalidOperationException($"Novedad con ID {request.Id} no encontrada");
                }

                novedad.SetVisible(request.Visible);
                await _empresaNovedadRepository.UpdateAsync(novedad);

                _logger.LogInformation("Novedad actualizada exitosamente: ID {Id}, Visible {Visible}",
                    request.Id, request.Visible);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar novedad con ID {Id}", request.Id);
                throw;
            }
        }
    }
}