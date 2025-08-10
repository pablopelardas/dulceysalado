using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.Catalog;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Catalog
{
    public class GetUnconfiguredProductsQueryHandler : IRequestHandler<GetUnconfiguredProductsQuery, GetUnconfiguredProductsQueryResult>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly ILogger<GetUnconfiguredProductsQueryHandler> _logger;

        public GetUnconfiguredProductsQueryHandler(
            ICatalogRepository catalogRepository,
            ILogger<GetUnconfiguredProductsQueryHandler> logger)
        {
            _catalogRepository = catalogRepository;
            _logger = logger;
        }

        public async Task<GetUnconfiguredProductsQueryResult> Handle(GetUnconfiguredProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var products = await _catalogRepository.GetUnconfiguredProductsAsync(request.EmpresaId, request.Page, request.PageSize);

                var productDtos = products.Select(p => new UnconfiguredProductDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Descripcion = p.Descripcion,
                    TieneImagen = p.TieneImagen,
                    TieneDescripcionCorta = p.TieneDescripcionCorta,
                    TieneDescripcionLarga = p.TieneDescripcionLarga,
                    TieneTags = p.TieneTags,
                    EsVisible = p.EsVisible,
                    EsDestacado = p.EsDestacado,
                    ConfiguracionFaltante = p.ConfiguracionFaltante
                }).ToList();

                // Para una implementación completa, necesitaríamos el total count del repositorio
                var totalCount = productDtos.Count; // Simplificado por ahora
                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                return new GetUnconfiguredProductsQueryResult
                {
                    Products = productDtos,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos sin configuración para empresa {EmpresaId}", request.EmpresaId);
                throw;
            }
        }
    }
}