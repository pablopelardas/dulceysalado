using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Ofertas;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Ofertas
{
    public class DeleteEmpresaOfertaCommandHandler : IRequestHandler<DeleteEmpresaOfertaCommand, bool>
    {
        private readonly IEmpresaOfertaRepository _empresaOfertaRepository;
        private readonly ILogger<DeleteEmpresaOfertaCommandHandler> _logger;

        public DeleteEmpresaOfertaCommandHandler(
            IEmpresaOfertaRepository empresaOfertaRepository,
            ILogger<DeleteEmpresaOfertaCommandHandler> logger)
        {
            _empresaOfertaRepository = empresaOfertaRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteEmpresaOfertaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _empresaOfertaRepository.DeleteAsync(request.Id);

                _logger.LogInformation("Oferta eliminada exitosamente: ID {Id}", request.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar oferta con ID {Id}", request.Id);
                throw;
            }
        }
    }
}