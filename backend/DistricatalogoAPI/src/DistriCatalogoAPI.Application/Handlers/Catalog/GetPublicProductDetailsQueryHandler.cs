using MediatR;
using DistriCatalogoAPI.Application.Queries.Catalog;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Catalog
{
    public class GetPublicProductDetailsQueryHandler : IRequestHandler<GetPublicProductDetailsQuery, GetPublicProductDetailsQueryResult?>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly ICompanyRepository _companyRepository;

        public GetPublicProductDetailsQueryHandler(
            ICatalogRepository catalogRepository,
            ICompanyRepository companyRepository)
        {
            _catalogRepository = catalogRepository;
            _companyRepository = companyRepository;
        }

        public async Task<GetPublicProductDetailsQueryResult?> Handle(GetPublicProductDetailsQuery request, CancellationToken cancellationToken)
        {
            // Obtener configuración de la empresa
            var empresa = await _companyRepository.GetByIdAsync(request.EmpresaId);
            if (empresa == null)
            {
                throw new KeyNotFoundException($"Empresa {request.EmpresaId} no encontrada");
            }

            var product = await _catalogRepository.GetProductDetailsAsync(
                request.ProductoCodigo, 
                request.EmpresaId, 
                request.ListaPrecioCodigo);
            
            if (product == null)
                return null;

            return new GetPublicProductDetailsQueryResult
            {
                Codigo = product.Codigo.ToString(),
                Nombre = product.Descripcion,
                Descripcion = product.DescripcionCorta ?? product.Descripcion,
                DescripcionLarga = product.Descripcion,
                Precio = empresa.MostrarPrecios ? product.Precio : null,
                Destacado = product.Destacado,
                ImagenUrls = string.IsNullOrEmpty(product.ImagenUrl) ? new List<string>() : new List<string> { product.ImagenUrl },
                Stock = empresa.MostrarStock ? (int?)product.Existencia : null,
                Tags = product.Tags?.ToList() ?? new List<string>(),
                Marca = product.Marca,
                ProductosRelacionados = new List<ProductoRelacionadoDto>() // Se implementaría la lógica de productos relacionados
            };
        }
    }
}