using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Novedades;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Novedades
{
    public class DeleteEmpresaNovedadCommandHandler : IRequestHandler<DeleteEmpresaNovedadCommand, bool>
    {
        private readonly IEmpresaNovedadRepository _empresaNovedadRepository;
        private readonly ILogger<DeleteEmpresaNovedadCommandHandler> _logger;

        public DeleteEmpresaNovedadCommandHandler(
            IEmpresaNovedadRepository empresaNovedadRepository,
            ILogger<DeleteEmpresaNovedadCommandHandler> logger)
        {
            _empresaNovedadRepository = empresaNovedadRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteEmpresaNovedadCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _empresaNovedadRepository.DeleteAsync(request.Id);

                _logger.LogInformation("Novedad eliminada exitosamente: ID {Id}", request.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar novedad con ID {Id}", request.Id);
                throw;
            }
        }
    }
}