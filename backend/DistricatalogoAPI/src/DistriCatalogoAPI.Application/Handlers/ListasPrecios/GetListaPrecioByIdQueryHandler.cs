using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.ListasPrecios;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ListasPrecios
{
    public class GetListaPrecioByIdQueryHandler : IRequestHandler<GetListaPrecioByIdQuery, GetListaPrecioByIdQueryResult?>
    {
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ILogger<GetListaPrecioByIdQueryHandler> _logger;

        public GetListaPrecioByIdQueryHandler(
            IListaPrecioRepository listaPrecioRepository,
            ILogger<GetListaPrecioByIdQueryHandler> logger)
        {
            _listaPrecioRepository = listaPrecioRepository;
            _logger = logger;
        }

        public async Task<GetListaPrecioByIdQueryResult?> Handle(GetListaPrecioByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo lista de precio por ID: {Id}", request.Id);

            var lista = await _listaPrecioRepository.GetCodigoAndNombreByIdAsync(request.Id);
            
            if (lista == null)
            {
                _logger.LogWarning("Lista de precio no encontrada: {Id}", request.Id);
                return null;
            }

            return new GetListaPrecioByIdQueryResult
            {
                Id = request.Id,
                Codigo = lista.Value.codigo,
                Nombre = lista.Value.nombre
            };
        }
    }
}