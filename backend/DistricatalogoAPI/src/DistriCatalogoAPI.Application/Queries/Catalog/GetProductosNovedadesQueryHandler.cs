using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Catalog
{
    public class GetProductosNovedadesQueryHandler : IRequestHandler<GetProductosNovedadesQuery, GetProductosNovedadesQueryResult>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IEmpresaNovedadRepository _empresaNovedadRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductosNovedadesQueryHandler> _logger;

        public GetProductosNovedadesQueryHandler(
            ICatalogRepository catalogRepository,
            IEmpresaNovedadRepository empresaNovedadRepository,
            ICompanyRepository companyRepository,
            IMapper mapper,
            ILogger<GetProductosNovedadesQueryHandler> logger)
        {
            _catalogRepository = catalogRepository;
            _empresaNovedadRepository = empresaNovedadRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GetProductosNovedadesQueryResult> Handle(GetProductosNovedadesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Obteniendo productos novedades para empresa {EmpresaId}", request.EmpresaId);

                // Validar que la empresa existe
                var empresa = await _companyRepository.GetByIdAsync(request.EmpresaId);
                if (empresa == null)
                {
                    _logger.LogWarning("Empresa {EmpresaId} no encontrada", request.EmpresaId);
                    return new GetProductosNovedadesQueryResult 
                    { 
                        Success = false, 
                        Message = "Empresa no encontrada",
                        Productos = new List<ProductoCatalogoDto>()
                    };
                }

                // Obtener las agrupaciones configuradas como novedades para la empresa
                var novedades = await _empresaNovedadRepository.GetByEmpresaIdAsync(request.EmpresaId, true);
                var agrupacionIds = novedades.Select(n => n.AgrupacionId).ToList();

                if (!agrupacionIds.Any())
                {
                    _logger.LogInformation("No hay novedades configuradas para empresa {EmpresaId}", request.EmpresaId);
                    return new GetProductosNovedadesQueryResult
                    {
                        Success = true,
                        Message = "No hay novedades configuradas",
                        Productos = new List<ProductoCatalogoDto>(),
                        TotalProductos = 0,
                        EmpresaNombre = empresa.Nombre
                    };
                }

                // Obtener productos de esas agrupaciones
                var productos = await _catalogRepository.GetProductsByAgrupacionIdsAsync(
                    agrupacionIds, 
                    request.EmpresaId, 
                    request.ListaPrecioCodigo);

                // Aplicar ordenamiento
                productos = ApplyOrdering(productos, request.OrdenarPor);

                // Mapear a DTOs
                var productosDto = _mapper.Map<List<ProductoCatalogoDto>>(productos);

                _logger.LogInformation("Se encontraron {Count} productos novedades para empresa {EmpresaId}", 
                    productosDto.Count, request.EmpresaId);

                return new GetProductosNovedadesQueryResult
                {
                    Success = true,
                    Productos = productosDto,
                    TotalProductos = productosDto.Count,
                    EmpresaNombre = empresa.Nombre
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos novedades para empresa {EmpresaId}", request.EmpresaId);
                return new GetProductosNovedadesQueryResult 
                { 
                    Success = false, 
                    Message = "Error al obtener productos novedades",
                    Productos = new List<ProductoCatalogoDto>()
                };
            }
        }

        private List<Domain.Entities.CatalogProduct> ApplyOrdering(List<Domain.Entities.CatalogProduct> productos, string? ordenarPor)
        {
            return ordenarPor?.ToLower() switch
            {
                "precio_asc" => productos.OrderBy(p => p.Precio).ToList(),
                "precio_desc" => productos.OrderByDescending(p => p.Precio).ToList(),
                "nombre_asc" => productos.OrderBy(p => p.Descripcion).ToList(),
                "nombre_desc" => productos.OrderByDescending(p => p.Descripcion).ToList(),
                _ => productos.OrderByDescending(p => p.Destacado).ThenBy(p => p.Descripcion).ToList() // Default: destacados primero, luego por nombre
            };
        }
    }
}