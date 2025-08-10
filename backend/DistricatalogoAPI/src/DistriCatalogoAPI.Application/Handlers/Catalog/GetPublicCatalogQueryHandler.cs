using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MediatR;
using DistriCatalogoAPI.Application.Queries.Catalog;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Catalog
{
    public class GetPublicCatalogQueryHandler : IRequestHandler<GetPublicCatalogQuery, GetPublicCatalogQueryResult>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly ICategoryEmpresaRepository _categoryRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<GetPublicCatalogQueryHandler> _logger;

        public GetPublicCatalogQueryHandler(
            ICatalogRepository catalogRepository,
            ICategoryEmpresaRepository categoryRepository,
            ICompanyRepository companyRepository,
            ILogger<GetPublicCatalogQueryHandler> logger)
        {
            _catalogRepository = catalogRepository;
            _categoryRepository = categoryRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<GetPublicCatalogQueryResult> Handle(GetPublicCatalogQuery request, CancellationToken cancellationToken)
        {
            using var activity = _logger.BeginScope(new Dictionary<string, object>
            {
                ["Operation"] = "GetPublicCatalog",
                ["CompanyId"] = request.EmpresaId,
                ["ListaPrecioCodigo"] = request.ListaPrecioCodigo,
                ["Page"] = request.Page,
                ["PageSize"] = request.PageSize
            });

            _logger.LogInformation("Fetching public catalog for company {CompanyId} with filters: {Filters}", 
                request.EmpresaId, new 
                { 
                    request.ListaPrecioCodigo, 
                    request.Busqueda, 
                    request.CodigoRubro, 
                    request.Destacados, 
                    request.OrdenarPor,
                    request.Page,
                    request.PageSize 
                });

            // Validar que la empresa existe
            var empresa = await _companyRepository.GetByIdAsync(request.EmpresaId);
            if (empresa == null)
            {
                _logger.LogWarning("Company {CompanyId} not found for catalog request", request.EmpresaId);
                return new GetPublicCatalogQueryResult
                {
                    Productos = new List<ProductoCatalogoDto>(),
                    TotalCount = 0,
                    Page = 1,
                    PageSize = request.PageSize,
                    TotalPages = 0,
                    Categorias = new List<CategoriaPublicaDto>()
                };
            }

            _logger.LogDebug("Company {CompanyId} ({CompanyName}) found - MostrarPrecios: {MostrarPrecios}", 
                empresa.Id, empresa.Nombre, empresa.MostrarPrecios);
            // Obtener productos del catálogo con paginación correcta
            var (catalogProducts, totalCount) = await _catalogRepository.GetCatalogProductsAsync(
                request.EmpresaId, 
                request.ListaPrecioCodigo,
                request.Destacados,
                request.CodigoRubro,
                request.Busqueda,
                request.OrdenarPor,
                request.Page, 
                request.PageSize);

            // Mapear a ProductoCatalogoDto respetando configuraciones de empresa
            var productos = catalogProducts.Select(product => new ProductoCatalogoDto
            {
                Codigo = product.Codigo.ToString(),
                Nombre = product.Descripcion,
                Descripcion = product.Descripcion,
                DescripcionCorta = product.DescripcionCorta,
                Precio = empresa.MostrarPrecios ? product.Precio : null,
                Destacado = product.Destacado,
                ImagenUrls = !string.IsNullOrWhiteSpace(product.ImagenUrl) 
                    ? new List<string> { product.ImagenUrl } 
                    : new List<string>(),
                Stock = empresa.MostrarStock ? (int?)product.Existencia : null,
                Tags = product.Tags?.ToList() ?? new List<string>(),
                Marca = product.Marca,
                Unidad = product.UnidadMedida,
                CodigoBarras = product.CodigoBarras,
                CodigoRubro = product.CodigoRubro,
                ImagenAlt = product.ImagenAlt,
                TipoProducto = product.TipoProducto,
                
                // Price list information (solo si se muestran precios)
                ListaPrecioId = empresa.MostrarPrecios ? product.ListaPrecioId : null,
                ListaPrecioNombre = empresa.MostrarPrecios ? product.ListaPrecioNombre : null,
                ListaPrecioCodigo = empresa.MostrarPrecios ? product.ListaPrecioCodigo : null
            }).ToList();

            // Si hay búsqueda, obtener TODAS las categorías con conteos de productos que coinciden
            List<CategoriaPublicaDto> categoriasDtos = new List<CategoriaPublicaDto>();
            
            if (!string.IsNullOrEmpty(request.Busqueda))
            {
                var categorias = await _catalogRepository.GetCategoriesFromFilteredProductsAsync(
                    request.EmpresaId,
                    request.ListaPrecioCodigo,
                    request.Destacados,
                    request.CodigoRubro,
                    request.Busqueda
                );
                
                categoriasDtos = categorias.Select(cat => new CategoriaPublicaDto
                {
                    Id = cat.CategoryId,
                    CodigoRubro = cat.CodigoRubro,
                    Nombre = cat.Nombre,
                    Descripcion = cat.Descripcion,
                    Icono = cat.Icono ?? "📦",
                    Color = cat.Color ?? "#6B7280",
                    Orden = cat.Orden,
                    ProductCount = cat.ProductCount // Esto puede ser 0 si no hay productos con la búsqueda en esa categoría
                }).ToList();
            }

            _logger.LogInformation("Public catalog retrieved successfully for company {CompanyId}: {ProductCount} products (total: {TotalCount}), {CategoryCount} categories", 
                request.EmpresaId, productos.Count, totalCount, categoriasDtos.Count);

            if (productos.Count > 0)
            {
                var firstProduct = productos.First();
                _logger.LogDebug("First product in catalog: {ProductCode} - {ProductName}, Price: {Price}", 
                    firstProduct.Codigo, firstProduct.Nombre, firstProduct.Precio);
            }

            return new GetPublicCatalogQueryResult
            {
                Productos = productos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                Categorias = categoriasDtos
            };
        }
    }
}