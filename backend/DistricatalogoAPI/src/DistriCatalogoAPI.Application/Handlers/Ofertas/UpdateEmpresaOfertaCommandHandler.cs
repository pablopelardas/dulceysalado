using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Ofertas;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Ofertas
{
    public class UpdateEmpresaOfertaCommandHandler : IRequestHandler<UpdateEmpresaOfertaCommand, bool>
    {
        private readonly IEmpresaOfertaRepository _empresaOfertaRepository;
        private readonly ILogger<UpdateEmpresaOfertaCommandHandler> _logger;

        public UpdateEmpresaOfertaCommandHandler(
            IEmpresaOfertaRepository empresaOfertaRepository,
            ILogger<UpdateEmpresaOfertaCommandHandler> logger)
        {
            _empresaOfertaRepository = empresaOfertaRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateEmpresaOfertaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var oferta = await _empresaOfertaRepository.GetByIdAsync(request.Id);
                if (oferta == null)
                {
                    throw new InvalidOperationException($"Oferta con ID {request.Id} no encontrada");
                }

                oferta.SetVisible(request.Visible);
                await _empresaOfertaRepository.UpdateAsync(oferta);

                _logger.LogInformation("Oferta actualizada exitosamente: ID {Id}, Visible {Visible}",
                    request.Id, request.Visible);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar oferta con ID {Id}", request.Id);
                throw;
            }
        }
    }
}