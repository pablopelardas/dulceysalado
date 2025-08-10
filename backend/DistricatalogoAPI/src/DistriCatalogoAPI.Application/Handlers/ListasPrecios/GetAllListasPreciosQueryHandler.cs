using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.ListasPrecios;
using DistriCatalogoAPI.Domain.Interfaces;
using ListaPrecioQueryDto = DistriCatalogoAPI.Application.Queries.ListasPrecios.ListaPrecioDto;

namespace DistriCatalogoAPI.Application.Handlers.ListasPrecios
{
    public class GetAllListasPreciosQueryHandler : IRequestHandler<GetAllListasPreciosQuery, GetAllListasPreciosQueryResult>
    {
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ILogger<GetAllListasPreciosQueryHandler> _logger;

        public GetAllListasPreciosQueryHandler(
            IListaPrecioRepository listaPrecioRepository,
            ILogger<GetAllListasPreciosQueryHandler> logger)
        {
            _listaPrecioRepository = listaPrecioRepository;
            _logger = logger;
        }

        public async Task<GetAllListasPreciosQueryResult> Handle(GetAllListasPreciosQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo todas las listas de precios");

            var listas = await _listaPrecioRepository.GetAllActiveAsync();

            var listasDto = listas.Select(l => new ListaPrecioQueryDto
            {
                Id = l.Id,
                Codigo = l.Codigo,
                Nombre = l.Nombre,
                EsPredeterminada = l.EsPredeterminada
            }).ToList();

            return new GetAllListasPreciosQueryResult
            {
                Listas = listasDto
            };
        }
    }
}