using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosBase;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosBase
{
    public class UpdateProductoBaseCommandHandler : IRequestHandler<UpdateProductoBaseCommand, UpdateProductoBaseCommandResult>
    {
        private readonly IProductBaseRepository _productRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IProductoBaseStockRepository _stockRepository;
        private readonly ILogger<UpdateProductoBaseCommandHandler> _logger;

        public UpdateProductoBaseCommandHandler(
            IProductBaseRepository productRepository,
            ICompanyRepository companyRepository,
            ICurrentUserService currentUserService,
            IProductoBaseStockRepository stockRepository,
            ILogger<UpdateProductoBaseCommandHandler> logger)
        {
            _productRepository = productRepository;
            _companyRepository = companyRepository;
            _currentUserService = currentUserService;
            _stockRepository = stockRepository;
            _logger = logger;
        }

        public async Task<UpdateProductoBaseCommandResult> Handle(UpdateProductoBaseCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            
            // VALIDACIÓN CRÍTICA: Solo empresas principales pueden actualizar productos base
            var company = await _companyRepository.GetByIdAsync(currentUser.CompanyId);
            if (company == null)
            {
                throw new InvalidOperationException("Empresa no encontrada");
            }

            if (!company.IsPrincipal)
            {
                throw new UnauthorizedAccessException("Solo empresas principales pueden actualizar productos base");
            }

            if (!currentUser.CanManageBaseProducts)
            {
                throw new UnauthorizedAccessException("No tiene permisos para gestionar productos base");
            }

            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                throw new InvalidOperationException($"No se encontró el producto con ID {request.Id}");
            }

            _logger.LogInformation("Actualizando producto base - Id: {Id}, Usuario: {UserId}", 
                request.Id, currentUser.Id);

            // Actualizar campos web únicamente
            product.UpdateWebFields(
                request.Visible,
                request.Destacado,
                request.OrdenCategoria,
                request.ImagenUrl,
                request.ImagenAlt,
                request.DescripcionCorta,
                request.DescripcionLarga,
                request.Tags,
                request.CodigoBarras,
                request.Marca,
                request.UnidadMedida);

            // Si se está actualizando descripción, precio o código rubro, usar el método UpdateFromSync
            // La existencia se maneja por separado en la tabla de stock
            if (request.Descripcion != null || request.Precio.HasValue || request.CodigoRubro.HasValue)
            {
                product.UpdateFromSync(
                    request.Descripcion ?? product.Descripcion,
                    request.CodigoRubro ?? product.CodigoRubro,
                    request.Precio ?? product.Precio,
                    product.Existencia, // Mantenemos la existencia original del producto base
                    product.Grupo1,
                    product.Grupo2,
                    product.Grupo3,
                    product.FechaAlta,
                    DateTime.UtcNow,
                    product.Imputable,
                    product.Disponible,
                    product.CodigoUbicacion);
            }

            await _productRepository.UpdateAsync(product);

            // Manejar actualización de stock por empresa
            if (request.Existencia.HasValue)
            {
                // Determinar la empresa objetivo: la especificada o la principal (ID 1)
                var empresaIdObjetivo = request.EmpresaId ?? 1;
                
                _logger.LogInformation("Actualizando stock - Producto: {ProductId}, Empresa: {EmpresaId}, Stock: {Stock}", 
                    product.Id, empresaIdObjetivo, request.Existencia.Value);

                await _stockRepository.UpdateStockAsync(empresaIdObjetivo, product.Id, request.Existencia.Value);
            }

            _logger.LogInformation("Producto base actualizado exitosamente - Id: {ProductId}", product.Id);

            return new UpdateProductoBaseCommandResult
            {
                Id = product.Id,
                Codigo = product.Codigo,
                Descripcion = product.Descripcion,
                CodigoRubro = product.CodigoRubro,
                Precio = product.Precio,
                Existencia = product.Existencia,
                Visible = product.Visible,
                Destacado = product.Destacado,
                OrdenCategoria = product.OrdenCategoria,
                ImagenUrl = product.ImagenUrl,
                ImagenAlt = product.ImagenAlt,
                DescripcionCorta = product.DescripcionCorta,
                DescripcionLarga = product.DescripcionLarga,
                Tags = product.Tags,
                CodigoBarras = product.CodigoBarras,
                Marca = product.Marca,
                UnidadMedida = product.UnidadMedida,
                AdministradoPorEmpresaId = product.AdministradoPorEmpresaId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}