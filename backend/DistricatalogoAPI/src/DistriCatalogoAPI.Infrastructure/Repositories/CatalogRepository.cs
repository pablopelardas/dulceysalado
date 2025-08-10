using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class CatalogRepository : ICatalogRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<CatalogRepository> _logger;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly IProductoBaseStockRepository _stockRepository;

        public CatalogRepository(DistricatalogoContext context, ILogger<CatalogRepository> logger, IListaPrecioRepository listaPrecioRepository, IProductoBaseStockRepository stockRepository)
        {
            _context = context;
            _logger = logger;
            _listaPrecioRepository = listaPrecioRepository;
            _stockRepository = stockRepository;
        }

        public async Task<(List<Domain.Entities.CatalogProduct> products, int totalCount)> GetCatalogProductsAsync(int empresaId, string? listaPrecioCodigo = null, bool? destacados = null, int? codigoRubro = null, string? busqueda = null, string? ordenarPor = null, int page = 1, int pageSize = 20)
        {
            // Resolver lista de precios por código o usar predeterminada
            int? listaPrecioId = null;
            if (!string.IsNullOrEmpty(listaPrecioCodigo))
            {
                listaPrecioId = await _listaPrecioRepository.GetIdByCodigoAsync(listaPrecioCodigo);
                if (!listaPrecioId.HasValue)
                {
                    return ([], 0); // Si no se encuentra la lista de precios, retornar vacío
                }
            }
            else
            {
                // Si no se especifica código, primero buscar predeterminada de empresa
                var empresa = await _context.Empresas
                    .FirstOrDefaultAsync(e => e.Id == empresaId);
                
                if (empresa?.ListaPrecioPredeterminadaId.HasValue == true)
                {
                    listaPrecioId = empresa.ListaPrecioPredeterminadaId.Value;
                }
                else
                {
                    // Fallback a lista global predeterminada
                    listaPrecioId = await _listaPrecioRepository.GetDefaultListIdAsync();
                }
            }

            if (!listaPrecioId.HasValue)
            {
                return ([], 0); // Si no hay lista de precios, retornar vacío
            }

            // Obtener empresa principal para esta empresa (puede ser ella misma)
            var empresaActual = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId);
            var empresaPrincipalId = empresaActual?.EmpresaPrincipalId ?? empresaId;

            // Construir query LINQ directa igual que la vista SQL original
            var query = from pb in _context.ProductosBases
                       join pbp in _context.ProductosBasePrecios on pb.Id equals pbp.ProductoBaseId
                       join lp in _context.ListasPrecios on pbp.ListaPrecioId equals lp.Id
                       join emp in _context.Empresas on empresaId equals emp.Id
                       from a in _context.Agrupaciones.Where(ag => pb.Grupo3 == ag.Codigo && ag.EmpresaPrincipalId == empresaPrincipalId).DefaultIfEmpty()
                       from eav in _context.EmpresasAgrupacionesVisibles.Where(eav => a != null && a.Id == eav.AgrupacionId && eav.EmpresaId == empresaId).DefaultIfEmpty()
                       where pb.AdministradoPorEmpresaId == empresaPrincipalId &&
                             (pb.Visible ?? false) == true &&
                             lp.Id == listaPrecioId.Value &&
                             lp.Activa == true &&
                             (a == null || (a.Activa ?? false) == true) &&
                             (pb.Grupo3 == null || (eav != null && (eav.Visible ?? false) == true) || 
                              !_context.EmpresasAgrupacionesVisibles.Any(eav2 => eav2.EmpresaId == empresaId))
                       select new 
                       {
                           ProductoId = pb.Id,
                           TipoProducto = "base",
                           Codigo = pb.Codigo,
                           Descripcion = pb.Descripcion ?? "",
                           CodigoRubro = pb.CodigoRubro,
                           Visible = pb.Visible,
                           Destacado = pb.Destacado,
                           ImagenUrl = pb.ImagenUrl,
                           Marca = pb.Marca,
                           UnidadMedida = pb.UnidadMedida,
                           EmpresaId = empresaId,
                           EmpresaNombre = emp.Nombre ?? "",
                           ListaPrecioId = lp.Id,
                           ListaCodigo = lp.Codigo,
                           ListaNombre = lp.Nombre,
                           PrecioFinal = pbp.Precio,
                           PrecioPersonalizado = false,
                           ActualizadoGecom = pb.UpdatedAt,
                           UpdatedAt = pb.UpdatedAt
                       };

            // Aplicar filtros
            if (destacados.HasValue)
                query = query.Where(p => (p.Destacado ?? false) == destacados.Value);

            if (codigoRubro.HasValue)
                query = query.Where(p => p.CodigoRubro == codigoRubro.Value);

            if (!string.IsNullOrEmpty(busqueda))
            {
                var busquedaLower = busqueda.ToLowerInvariant();
                var palabras = busquedaLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                if (palabras.Length > 1)
                {
                    // Búsqueda con múltiples palabras - todas deben estar presentes
                    foreach (var palabra in palabras)
                    {
                        query = query.Where(p => 
                            p.Descripcion.ToLower().Contains(palabra) ||
                            (p.Marca != null && p.Marca.ToLower().Contains(palabra))
                        );
                    }
                }
                else
                {
                    // Búsqueda simple con una palabra
                    query = query.Where(p => 
                        p.Descripcion.ToLower().Contains(busquedaLower) ||
                        (p.Marca != null && p.Marca.ToLower().Contains(busquedaLower))
                    );
                }
            }

            var totalCount = await query.CountAsync();
            
            // Aplicar ordenamiento
            switch (ordenarPor?.ToLowerInvariant())
            {
                case "precio_asc":
                    query = query.OrderBy(p => p.PrecioFinal);
                    break;
                case "precio_desc":
                    query = query.OrderByDescending(p => p.PrecioFinal);
                    break;
                case "nombre_asc":
                    query = query.OrderBy(p => p.Descripcion);
                    break;
                case "nombre_desc":
                    query = query.OrderByDescending(p => p.Descripcion);
                    break;
                default:
                    // Ordenamiento por defecto: destacados primero, luego por nombre
                    query = query.OrderByDescending(p => p.Destacado ?? false)
                                 .ThenBy(p => p.Descripcion);
                    break;
            }
            
            // Obtener productos con stock para filtrar igual que la vista SQL
            var allProducts = await query.ToListAsync();
            
            // Agrupar por ProductoId para eliminar duplicados causados por los JOINs con agrupaciones
            var uniqueProducts = allProducts.GroupBy(p => p.ProductoId).Select(g => g.First()).ToList();
            
            var allProductIds = uniqueProducts.Select(p => p.ProductoId).ToList();
            var stockPorProducto = await _stockRepository.GetStockBatchAsync(empresaId, allProductIds);

            // Filtrar productos con stock > 0 (igual que la vista SQL: pb.existencia > 0)
            var productsWithStock = uniqueProducts.Where(p => stockPorProducto.GetValueOrDefault(p.ProductoId, 0) > 0).ToList();
            
            var totalCountWithStock = productsWithStock.Count;
            
            // Aplicar paginación después del filtro por stock
            var paginatedProducts = productsWithStock
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var mappedProducts = paginatedProducts.Select(p => MapToCatalogProductFromDirectQuery(p, stockPorProducto.GetValueOrDefault(p.ProductoId, 0))).ToList();

            return (mappedProducts, totalCountWithStock);
        }

        public async Task<List<Domain.Entities.CatalogProduct>> GetFeaturedProductsAsync(int empresaId, string? listaPrecioCodigo = null, int limit = 10)
        {
            // Resolver lista de precios por código o usar predeterminada
            int? listaPrecioId = null;
            if (!string.IsNullOrEmpty(listaPrecioCodigo))
            {
                listaPrecioId = await _listaPrecioRepository.GetIdByCodigoAsync(listaPrecioCodigo);
            }
            else
            {
                // Si no se especifica código, primero buscar predeterminada de empresa
                var empresa = await _context.Empresas
                    .FirstOrDefaultAsync(e => e.Id == empresaId);
                
                if (empresa?.ListaPrecioPredeterminadaId.HasValue == true)
                {
                    listaPrecioId = empresa.ListaPrecioPredeterminadaId.Value;
                }
                else
                {
                    // Fallback a lista global predeterminada
                    listaPrecioId = await _listaPrecioRepository.GetDefaultListIdAsync();
                }
            }

            if (!listaPrecioId.HasValue)
            {
                return new List<Domain.Entities.CatalogProduct>();
            }

            // Obtener empresa principal para esta empresa (puede ser ella misma)
            var empresaActual = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId);
            var empresaPrincipalId = empresaActual?.EmpresaPrincipalId ?? empresaId;

            // Construir query LINQ directa igual que la vista SQL original
            var query = from pb in _context.ProductosBases
                       join pbp in _context.ProductosBasePrecios on pb.Id equals pbp.ProductoBaseId
                       join lp in _context.ListasPrecios on pbp.ListaPrecioId equals lp.Id
                       join emp in _context.Empresas on empresaId equals emp.Id
                       from a in _context.Agrupaciones.Where(ag => pb.Grupo3 == ag.Codigo && ag.EmpresaPrincipalId == empresaPrincipalId).DefaultIfEmpty()
                       from eav in _context.EmpresasAgrupacionesVisibles.Where(eav => a != null && a.Id == eav.AgrupacionId && eav.EmpresaId == empresaId).DefaultIfEmpty()
                       where pb.AdministradoPorEmpresaId == empresaPrincipalId &&
                             (pb.Destacado ?? false) == true &&
                             (pb.Visible ?? false) == true &&
                             lp.Id == listaPrecioId.Value &&
                             lp.Activa == true &&
                             (a == null || (a.Activa ?? false) == true) &&
                             (pb.Grupo3 == null || (eav != null && (eav.Visible ?? false) == true) || 
                              !_context.EmpresasAgrupacionesVisibles.Any(eav2 => eav2.EmpresaId == empresaId))
                       select new 
                       {
                           ProductoId = pb.Id,
                           TipoProducto = "base",
                           Codigo = pb.Codigo,
                           Descripcion = pb.Descripcion ?? "",
                           CodigoRubro = pb.CodigoRubro,
                           Visible = pb.Visible,
                           Destacado = pb.Destacado,
                           ImagenUrl = pb.ImagenUrl,
                           Marca = pb.Marca,
                           UnidadMedida = pb.UnidadMedida,
                           EmpresaId = empresaId,
                           EmpresaNombre = emp.Nombre ?? "",
                           ListaPrecioId = lp.Id,
                           ListaCodigo = lp.Codigo,
                           ListaNombre = lp.Nombre,
                           PrecioFinal = pbp.Precio,
                           PrecioPersonalizado = false,
                           ActualizadoGecom = pb.UpdatedAt,
                           UpdatedAt = pb.UpdatedAt
                       };

            var allProducts = await query.ToListAsync();
            
            // Agrupar por ProductoId para eliminar duplicados
            var uniqueProducts = allProducts.GroupBy(p => p.ProductoId).Select(g => g.First()).ToList();

            // Obtener stock para los productos
            var allProductIds = uniqueProducts.Select(p => p.ProductoId).ToList();
            var stockPorProducto = await _stockRepository.GetStockBatchAsync(empresaId, allProductIds);

            // Filtrar productos con stock > 0 y aplicar límite
            var productsWithStock = uniqueProducts
                .Where(p => stockPorProducto.GetValueOrDefault(p.ProductoId, 0) > 0)
                .Take(limit)
                .ToList();

            return productsWithStock.Select(p => MapToCatalogProductFromDirectQuery(p, stockPorProducto.GetValueOrDefault(p.ProductoId, 0))).ToList();
        }

        public async Task<Domain.Entities.CatalogProduct?> GetProductDetailsAsync(string productoCodigo, int empresaId, string? listaPrecioCodigo = null)
        {
            // Resolver lista de precios
            int? listaPrecioId = null;
            if (!string.IsNullOrEmpty(listaPrecioCodigo))
            {
                listaPrecioId = await _listaPrecioRepository.GetIdByCodigoAsync(listaPrecioCodigo);
            }
            else
            {
                // Si no se especifica código, primero buscar predeterminada de empresa
                var empresa = await _context.Empresas
                    .FirstOrDefaultAsync(e => e.Id == empresaId);
                
                if (empresa?.ListaPrecioPredeterminadaId.HasValue == true)
                {
                    listaPrecioId = empresa.ListaPrecioPredeterminadaId.Value;
                }
                else
                {
                    // Fallback a lista global predeterminada
                    listaPrecioId = await _listaPrecioRepository.GetDefaultListIdAsync();
                }
            }

            if (!listaPrecioId.HasValue)
            {
                return null;
            }

            // Obtener empresa principal para esta empresa (puede ser ella misma)
            var empresaActual = await _context.Empresas.FirstOrDefaultAsync(e => e.Id == empresaId);
            var empresaPrincipalId = empresaActual?.EmpresaPrincipalId ?? empresaId;

            // Construir query LINQ directa igual que la vista SQL original
            var query = from pb in _context.ProductosBases
                       join pbp in _context.ProductosBasePrecios on pb.Id equals pbp.ProductoBaseId
                       join lp in _context.ListasPrecios on pbp.ListaPrecioId equals lp.Id
                       join emp in _context.Empresas on empresaId equals emp.Id
                       from a in _context.Agrupaciones.Where(ag => pb.Grupo3 == ag.Codigo && ag.EmpresaPrincipalId == empresaPrincipalId).DefaultIfEmpty()
                       from eav in _context.EmpresasAgrupacionesVisibles.Where(eav => a != null && a.Id == eav.AgrupacionId && eav.EmpresaId == empresaId).DefaultIfEmpty()
                       where pb.Codigo == productoCodigo &&
                             pb.AdministradoPorEmpresaId == empresaPrincipalId &&
                             (pb.Visible ?? false) == true &&
                             lp.Id == listaPrecioId.Value &&
                             lp.Activa == true &&
                             (a == null || (a.Activa ?? false) == true) &&
                             (pb.Grupo3 == null || (eav != null && (eav.Visible ?? false) == true) || 
                              !_context.EmpresasAgrupacionesVisibles.Any(eav2 => eav2.EmpresaId == empresaId))
                       select new 
                       {
                           ProductoId = pb.Id,
                           TipoProducto = "base",
                           Codigo = pb.Codigo,
                           Descripcion = pb.Descripcion ?? "",
                           CodigoRubro = pb.CodigoRubro,
                           Visible = pb.Visible,
                           Destacado = pb.Destacado,
                           ImagenUrl = pb.ImagenUrl,
                           Marca = pb.Marca,
                           UnidadMedida = pb.UnidadMedida,
                           EmpresaId = empresaId,
                           EmpresaNombre = emp.Nombre ?? "",
                           ListaPrecioId = lp.Id,
                           ListaCodigo = lp.Codigo,
                           ListaNombre = lp.Nombre,
                           PrecioFinal = pbp.Precio,
                           PrecioPersonalizado = false,
                           ActualizadoGecom = pb.UpdatedAt,
                           UpdatedAt = pb.UpdatedAt
                       };

            var product = await query.FirstOrDefaultAsync();

            if (product == null)
            {
                return null;
            }

            // Obtener stock para el producto
            var stock = await _stockRepository.GetStockAsync(empresaId, product.ProductoId);

            // Filtrar por stock > 0 (igual que la vista SQL)
            if (stock <= 0)
            {
                return null;
            }

            return MapToCatalogProductFromDirectQuery(product, stock ?? 0);
        }

        public async Task<(List<Domain.Entities.CatalogProduct> products, int totalCount)> SearchCatalogAsync(
            int empresaId, 
            string? texto = null, 
            List<int>? codigosRubro = null,
            decimal? precioMinimo = null,
            decimal? precioMaximo = null,
            bool? soloDestacados = null, 
            List<string>? tags = null, 
            string? marca = null,
            string? orderBy = null, 
            bool ascending = true, 
            int page = 1, 
            int pageSize = 20)
        {
            // Obtener lista de precios predeterminada
            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e => e.Id == empresaId);
            
            int? listaPrecioId = null;
            if (empresa?.ListaPrecioPredeterminadaId.HasValue == true)
            {
                listaPrecioId = empresa.ListaPrecioPredeterminadaId.Value;
            }
            else
            {
                listaPrecioId = await _listaPrecioRepository.GetDefaultListIdAsync();
            }

            if (!listaPrecioId.HasValue)
            {
                return (new List<Domain.Entities.CatalogProduct>(), 0);
            }

            // Construir query LINQ directa
            var query = from pb in _context.ProductosBases
                       join pbp in _context.ProductosBasePrecios on pb.Id equals pbp.ProductoBaseId
                       join lp in _context.ListasPrecios on pbp.ListaPrecioId equals lp.Id
                       join emp in _context.Empresas on empresaId equals emp.Id
                       where pb.AdministradoPorEmpresaId == empresaId &&
                             (pb.Visible ?? false) == true &&
                             lp.Id == listaPrecioId.Value &&
                             lp.Activa == true
                       select new 
                       {
                           ProductoId = pb.Id,
                           TipoProducto = "base",
                           Codigo = pb.Codigo,
                           Descripcion = pb.Descripcion ?? "",
                           CodigoRubro = pb.CodigoRubro,
                           Visible = pb.Visible,
                           Destacado = pb.Destacado,
                           ImagenUrl = pb.ImagenUrl,
                           Marca = pb.Marca,
                           UnidadMedida = pb.UnidadMedida,
                           EmpresaId = empresaId,
                           EmpresaNombre = emp.Nombre ?? "",
                           ListaPrecioId = lp.Id,
                           ListaCodigo = lp.Codigo,
                           ListaNombre = lp.Nombre,
                           PrecioFinal = pbp.Precio,
                           PrecioPersonalizado = false,
                           ActualizadoGecom = pb.UpdatedAt,
                           UpdatedAt = pb.UpdatedAt
                       };

            // Aplicar filtros de búsqueda
            if (!string.IsNullOrEmpty(texto))
            {
                var textoLower = texto.ToLowerInvariant();
                var palabras = textoLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                if (palabras.Length > 1)
                {
                    // Búsqueda con múltiples palabras - todas deben estar presentes
                    foreach (var palabra in palabras)
                    {
                        query = query.Where(p => 
                            p.Descripcion.ToLower().Contains(palabra) ||
                            (p.Marca != null && p.Marca.ToLower().Contains(palabra))
                        );
                    }
                }
                else
                {
                    // Búsqueda simple con una palabra
                    query = query.Where(p => 
                        p.Descripcion.ToLower().Contains(textoLower) ||
                        (p.Marca != null && p.Marca.ToLower().Contains(textoLower))
                    );
                }
            }

            if (codigosRubro?.Any() == true)
                query = query.Where(p => p.CodigoRubro != null && codigosRubro.Contains(p.CodigoRubro.Value));

            if (precioMinimo.HasValue)
                query = query.Where(p => p.PrecioFinal >= precioMinimo.Value);

            if (precioMaximo.HasValue)
                query = query.Where(p => p.PrecioFinal <= precioMaximo.Value);

            if (!string.IsNullOrEmpty(marca))
                query = query.Where(p => p.Marca != null && p.Marca.ToLower().Contains(marca.ToLowerInvariant()));

            if (soloDestacados == true)
                query = query.Where(p => (p.Destacado ?? false) == true);

            // Note: Tags filtering would require joining with ProductosBase.Tags field

            // Aplicar ordenamiento
            if (!string.IsNullOrEmpty(orderBy))
            {
                query = orderBy.ToLowerInvariant() switch
                {
                    "precio" => ascending ? 
                        query.OrderBy(p => p.PrecioFinal) :
                        query.OrderByDescending(p => p.PrecioFinal),
                    "nombre" => ascending ?
                        query.OrderBy(p => p.Descripcion) :
                        query.OrderByDescending(p => p.Descripcion),
                    "destacado" => ascending ?
                        query.OrderBy(p => p.Destacado) :
                        query.OrderByDescending(p => p.Destacado),
                    _ => query
                };
            }

            // Obtener productos con stock para filtrar igual que la vista SQL
            var allProducts = await query.ToListAsync();
            var allProductIds = allProducts.Select(p => p.ProductoId).ToList();
            var stockPorProducto = await _stockRepository.GetStockBatchAsync(empresaId, allProductIds);

            // Filtrar productos con stock > 0 (igual que la vista SQL: pb.existencia > 0)
            var productsWithStock = allProducts.Where(p => stockPorProducto.GetValueOrDefault(p.ProductoId, 0) > 0).ToList();
            
            var totalCountWithStock = productsWithStock.Count;
            
            // Aplicar paginación después del filtro por stock
            var paginatedProducts = productsWithStock
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (paginatedProducts.Select(p => MapToCatalogProductFromDirectQuery(p, stockPorProducto.GetValueOrDefault(p.ProductoId, 0))).ToList(), totalCountWithStock);
        }

        public async Task<List<UnconfiguredProduct>> GetUnconfiguredProductsAsync(int empresaId, int page = 1, int pageSize = 20)
        {
            // Buscar productos base que necesitan configuración web (sin imagen, descripción corta, etc.)
            var products = await _context.ProductosBases
                .Where(p => p.AdministradoPorEmpresaId == empresaId && 
                           (string.IsNullOrEmpty(p.ImagenUrl) || 
                            string.IsNullOrEmpty(p.DescripcionCorta) ||
                            string.IsNullOrEmpty(p.Tags)))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return products.Select(p => new UnconfiguredProduct
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Descripcion = p.Descripcion,
                TieneImagen = !string.IsNullOrEmpty(p.ImagenUrl),
                TieneDescripcionCorta = !string.IsNullOrWhiteSpace(p.DescripcionCorta),
                TieneDescripcionLarga = !string.IsNullOrWhiteSpace(p.DescripcionLarga),
                TieneTags = !string.IsNullOrWhiteSpace(p.Tags),
                EsVisible = p.Visible ?? false,
                EsDestacado = p.Destacado ?? false,
                ConfiguracionFaltante = GetMissingConfiguration(p)
            }).ToList();
        }


        private List<string> GetMissingConfiguration(ProductosBase product)
        {
            var missing = new List<string>();

            if (string.IsNullOrEmpty(product.ImagenUrl))
                missing.Add("Imagen principal");

            if (string.IsNullOrWhiteSpace(product.DescripcionCorta))
                missing.Add("Descripción corta");

            if (string.IsNullOrWhiteSpace(product.DescripcionLarga))
                missing.Add("Descripción detallada");

            if (string.IsNullOrWhiteSpace(product.Tags))
                missing.Add("Tags de búsqueda");

            if (!(product.Visible ?? false))
                missing.Add("Visibilidad activada");

            return missing;
        }

        public async Task<List<CategoryProductCount>> GetCategoriesWithProductCountAsync(int empresaId)
        {
            // Obtener lista de precios predeterminada
            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e => e.Id == empresaId);
            
            int? listaPrecioId = null;
            if (empresa?.ListaPrecioPredeterminadaId.HasValue == true)
            {
                listaPrecioId = empresa.ListaPrecioPredeterminadaId.Value;
            }
            else
            {
                listaPrecioId = await _listaPrecioRepository.GetDefaultListIdAsync();
            }

            if (!listaPrecioId.HasValue)
            {
                return new List<CategoryProductCount>();
            }

            // Obtener empresa principal para esta empresa (puede ser ella misma)
            var empresaPrincipalId = empresa?.EmpresaPrincipalId ?? empresaId;

            // Obtener productos filtrados por agrupaciones
            var productosQuery = from pb in _context.ProductosBases
                                join pbp in _context.ProductosBasePrecios on pb.Id equals pbp.ProductoBaseId
                                join lp in _context.ListasPrecios on pbp.ListaPrecioId equals lp.Id
                                from a in _context.Agrupaciones.Where(ag => pb.Grupo3 == ag.Codigo && ag.EmpresaPrincipalId == empresaPrincipalId).DefaultIfEmpty()
                                from eav in _context.EmpresasAgrupacionesVisibles.Where(eav => a != null && a.Id == eav.AgrupacionId && eav.EmpresaId == empresaId).DefaultIfEmpty()
                                where pb.AdministradoPorEmpresaId == empresaPrincipalId &&
                                      (pb.Visible ?? false) == true &&
                                      pb.CodigoRubro != null &&
                                      lp.Id == listaPrecioId.Value &&
                                      lp.Activa == true &&
                                      (a == null || (a.Activa ?? false) == true) &&
                                      (pb.Grupo3 == null || (eav != null && (eav.Visible ?? false) == true) || 
                                       !_context.EmpresasAgrupacionesVisibles.Any(eav2 => eav2.EmpresaId == empresaId))
                                select new { ProductoId = pb.Id, CodigoRubro = pb.CodigoRubro };

            var productos = await productosQuery.ToListAsync();
            
            // Agrupar por Id para eliminar duplicados causados por los JOINs
            var productosUnicos = productos.GroupBy(p => p.ProductoId).Select(g => g.First()).ToList();
            
            // Obtener stock para filtrar solo productos con stock > 0
            var productIds = productosUnicos.Select(p => p.ProductoId).ToList();
            var stockPorProducto = await _stockRepository.GetStockBatchAsync(empresaId, productIds);
            
            // Filtrar productos con stock > 0 y agrupar por CodigoRubro
            var productCounts = productosUnicos
                .Where(p => stockPorProducto.GetValueOrDefault(p.ProductoId, 0) > 0)
                .GroupBy(p => p.CodigoRubro)
                .Select(g => new 
                {
                    CodigoRubro = g.Key ?? 0,
                    Count = g.Count()
                }).ToList();

            _logger.LogInformation($"DEBUG - Empresa {empresaId} - Códigos de rubro con productos: [{string.Join(", ", productCounts.Select(pc => $"{pc.CodigoRubro}({pc.Count})"))}]");

            var codigosRubroConProductos = productCounts.Select(pc => pc.CodigoRubro).ToList();

            // Usar solo categorías base
            var categorias = await _context.CategoriasBases
                .Where(c => (c.Visible ?? false) == true &&
                           codigosRubroConProductos.Contains(c.CodigoRubro))
                .Select(c => new CategoryProductCount
                {
                    CategoryId = c.Id,
                    CodigoRubro = c.CodigoRubro,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Icono = c.Icono,
                    Color = c.Color,
                    Orden = c.Orden ?? 0,
                    ProductCount = 0 // Se calculará después de la consulta
                })
                .OrderBy(c => c.Orden)
                .ThenBy(c => c.Nombre)
                .ToListAsync();

            // Asignar los conteos de productos después de la consulta
            foreach (var categoria in categorias)
            {
                var count = productCounts.FirstOrDefault(pc => pc.CodigoRubro == categoria.CodigoRubro)?.Count ?? 0;
                categoria.ProductCount = count;
            }

            _logger.LogInformation($"DEBUG - Resultado final desde LINQ: [{string.Join(", ", categorias.Select(c => $"Id:{c.CategoryId}-CR:{c.CodigoRubro}-{c.Nombre}"))}]");

            return categorias;
        }

        public async Task<(List<Domain.Entities.CatalogProduct> products, int totalCount)> GetCatalogProductsByCategoryAsync(
            int empresaId, 
            int codigoRubro, 
            string? orderBy = null, 
            bool ascending = true, 
            int page = 1, 
            int pageSize = 20)
        {
            // Obtener lista de precios predeterminada
            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e => e.Id == empresaId);
            
            int? listaPrecioId = null;
            if (empresa?.ListaPrecioPredeterminadaId.HasValue == true)
            {
                listaPrecioId = empresa.ListaPrecioPredeterminadaId.Value;
            }
            else
            {
                listaPrecioId = await _listaPrecioRepository.GetDefaultListIdAsync();
            }

            if (!listaPrecioId.HasValue)
            {
                return (new List<Domain.Entities.CatalogProduct>(), 0);
            }

            // Construir query LINQ directa
            var query = from pb in _context.ProductosBases
                       join pbp in _context.ProductosBasePrecios on pb.Id equals pbp.ProductoBaseId
                       join lp in _context.ListasPrecios on pbp.ListaPrecioId equals lp.Id
                       join emp in _context.Empresas on empresaId equals emp.Id
                       where pb.AdministradoPorEmpresaId == empresaId &&
                             (pb.Visible ?? false) == true &&
                             pb.CodigoRubro == codigoRubro &&
                             lp.Id == listaPrecioId.Value &&
                             lp.Activa == true
                       select new 
                       {
                           ProductoId = pb.Id,
                           TipoProducto = "base",
                           Codigo = pb.Codigo,
                           Descripcion = pb.Descripcion ?? "",
                           CodigoRubro = pb.CodigoRubro,
                           Visible = pb.Visible,
                           Destacado = pb.Destacado,
                           ImagenUrl = pb.ImagenUrl,
                           Marca = pb.Marca,
                           UnidadMedida = pb.UnidadMedida,
                           EmpresaId = empresaId,
                           EmpresaNombre = emp.Nombre ?? "",
                           ListaPrecioId = lp.Id,
                           ListaCodigo = lp.Codigo,
                           ListaNombre = lp.Nombre,
                           PrecioFinal = pbp.Precio,
                           PrecioPersonalizado = false,
                           ActualizadoGecom = pb.UpdatedAt,
                           UpdatedAt = pb.UpdatedAt
                       };

            // Aplicar ordenamiento
            if (!string.IsNullOrEmpty(orderBy))
            {
                query = orderBy.ToLowerInvariant() switch
                {
                    "precio" => ascending ? 
                        query.OrderBy(p => p.PrecioFinal) :
                        query.OrderByDescending(p => p.PrecioFinal),
                    "nombre" => ascending ?
                        query.OrderBy(p => p.Descripcion) :
                        query.OrderByDescending(p => p.Descripcion),
                    "destacado" => ascending ?
                        query.OrderBy(p => p.Destacado ?? false) :
                        query.OrderByDescending(p => p.Destacado ?? false),
                    _ => query.OrderByDescending(p => p.Destacado ?? false)
                          .ThenBy(p => p.Descripcion)
                };
            }
            else
            {
                // Orden por defecto: destacados primero, luego por nombre
                query = query.OrderByDescending(p => p.Destacado ?? false)
                            .ThenBy(p => p.Descripcion);
            }

            // Obtener productos con stock para filtrar igual que la vista SQL
            var allProducts = await query.ToListAsync();
            var allProductIds = allProducts.Select(p => p.ProductoId).ToList();
            var stockPorProducto = await _stockRepository.GetStockBatchAsync(empresaId, allProductIds);

            // Filtrar productos con stock > 0 (igual que la vista SQL: pb.existencia > 0)
            var productsWithStock = allProducts.Where(p => stockPorProducto.GetValueOrDefault(p.ProductoId, 0) > 0).ToList();
            
            var totalCountWithStock = productsWithStock.Count;
            
            // Aplicar paginación después del filtro por stock
            var paginatedProducts = productsWithStock
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (paginatedProducts.Select(p => MapToCatalogProductFromDirectQuery(p, stockPorProducto.GetValueOrDefault(p.ProductoId, 0))).ToList(), totalCountWithStock);
        }

        private Domain.Entities.CatalogProduct MapToCatalogProductFromPreciosView(VistaProductosPreciosEmpresa vista)
        {
            return new Domain.Entities.CatalogProduct
            {
                Codigo = vista.Codigo.ToString(),
                Nombre = vista.Descripcion,
                Descripcion = vista.Descripcion,
                DescripcionCorta = null, // Not available in this view
                Precio = vista.PrecioFinal,
                Existencia = null, // Not available in this view
                Destacado = vista.Destacado ?? false,
                Visible = vista.Visible ?? false,
                ImagenUrl = vista.ImagenUrl,
                ImagenAlt = null, // Not available in this view
                Tags = new List<string>(), // Not available in this view
                Marca = vista.Marca,
                UnidadMedida = vista.UnidadMedida,
                CodigoBarras = null, // Not available in this view
                CodigoRubro = vista.CodigoRubro,
                TipoProducto = vista.TipoProducto ?? string.Empty,
                EmpresaId = vista.EmpresaId,
                EmpresaNombre = vista.EmpresaNombre ?? string.Empty,
                
                // Price list information
                ListaPrecioId = vista.ListaPrecioId,
                ListaPrecioNombre = vista.ListaNombre,
                ListaPrecioCodigo = vista.ListaCodigo
            };
        }

        private Domain.Entities.CatalogProduct MapToCatalogProductFromDirectQuery(dynamic producto, decimal existencia)
        {
            return new Domain.Entities.CatalogProduct
            {
                Codigo = producto.Codigo.ToString(),
                Nombre = producto.Descripcion,
                Descripcion = producto.Descripcion,
                DescripcionCorta = null, // Not available in direct query
                Precio = producto.PrecioFinal,
                Existencia = existencia,
                Destacado = producto.Destacado ?? false,
                Visible = producto.Visible ?? false,
                ImagenUrl = producto.ImagenUrl,
                ImagenAlt = null, // Not available in direct query
                Tags = new List<string>(), // Not available in direct query
                Marca = producto.Marca,
                UnidadMedida = producto.UnidadMedida,
                CodigoBarras = null, // Not available in direct query
                CodigoRubro = producto.CodigoRubro,
                TipoProducto = producto.TipoProducto ?? string.Empty,
                EmpresaId = producto.EmpresaId,
                EmpresaNombre = producto.EmpresaNombre ?? string.Empty,
                
                // Price list information
                ListaPrecioId = producto.ListaPrecioId,
                ListaPrecioNombre = producto.ListaNombre,
                ListaPrecioCodigo = producto.ListaCodigo
            };
        }

        public async Task<List<CategoryProductCount>> GetCategoriesFromFilteredProductsAsync(int empresaId, string? listaPrecioCodigo = null, bool? destacados = null, int? codigoRubro = null, string? busqueda = null)
        {
            // Obtener TODAS las categorías base que tienen productos
            var todasLasCategorias = await _context.CategoriasBases
                .Where(c => c.Visible == true)
                .OrderBy(c => c.Orden ?? 0)
                .Select(c => new
                {
                    c.Id,
                    c.CodigoRubro,
                    c.Nombre,
                    c.Descripcion,
                    c.Icono,
                    c.Color,
                    Orden = c.Orden ?? 0
                })
                .ToListAsync();

            // Si no hay búsqueda, devolver todas las categorías con conteo 0
            if (string.IsNullOrEmpty(busqueda))
            {
                return todasLasCategorias.Select(c => new CategoryProductCount
                {
                    CategoryId = c.Id,
                    CodigoRubro = c.CodigoRubro,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Icono = c.Icono,
                    Color = c.Color,
                    Orden = c.Orden,
                    ProductCount = 0
                }).ToList();
            }

            // Resolver lista de precios por código o usar predeterminada
            int? listaPrecioId = null;
            if (!string.IsNullOrEmpty(listaPrecioCodigo))
            {
                listaPrecioId = await _listaPrecioRepository.GetIdByCodigoAsync(listaPrecioCodigo);
            }
            else
            {
                var empresa = await _context.Empresas
                    .FirstOrDefaultAsync(e => e.Id == empresaId);
                
                if (empresa?.ListaPrecioPredeterminadaId.HasValue == true)
                {
                    listaPrecioId = empresa.ListaPrecioPredeterminadaId.Value;
                }
                else
                {
                    listaPrecioId = await _listaPrecioRepository.GetDefaultListIdAsync();
                }
            }

            if (!listaPrecioId.HasValue)
            {
                return todasLasCategorias.Select(c => new CategoryProductCount
                {
                    CategoryId = c.Id,
                    CodigoRubro = c.CodigoRubro,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Icono = c.Icono,
                    Color = c.Color,
                    Orden = c.Orden,
                    ProductCount = 0
                }).ToList();
            }

            // Construir query LINQ directa para productos con búsqueda (sin filtro de categoría)
            var queryBusqueda = from pb in _context.ProductosBases
                               join pbp in _context.ProductosBasePrecios on pb.Id equals pbp.ProductoBaseId
                               join lp in _context.ListasPrecios on pbp.ListaPrecioId equals lp.Id
                               where pb.AdministradoPorEmpresaId == empresaId &&
                                     (pb.Visible ?? false) == true &&
                                     lp.Id == listaPrecioId.Value &&
                                     lp.Activa == true
                               select new 
                               {
                                   ProductoId = pb.Id,
                                   Descripcion = pb.Descripcion ?? "",
                                   CodigoRubro = pb.CodigoRubro,
                                   Destacado = pb.Destacado,
                                   Marca = pb.Marca
                               };

            if (destacados.HasValue)
                queryBusqueda = queryBusqueda.Where(p => (p.Destacado ?? false) == destacados.Value);

            // Aplicar filtro de búsqueda
            var busquedaLower = busqueda.ToLowerInvariant();
            var palabras = busquedaLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (palabras.Length > 1)
            {
                // Búsqueda con múltiples palabras - todas deben estar presentes
                foreach (var palabra in palabras)
                {
                    queryBusqueda = queryBusqueda.Where(p => 
                        p.Descripcion.ToLower().Contains(palabra) ||
                        (p.Marca != null && p.Marca.ToLower().Contains(palabra))
                    );
                }
            }
            else
            {
                // Búsqueda simple con una palabra
                queryBusqueda = queryBusqueda.Where(p => 
                    p.Descripcion.ToLower().Contains(busquedaLower) ||
                    (p.Marca != null && p.Marca.ToLower().Contains(busquedaLower))
                );
            }

            // Obtener conteos por categoría para productos que coinciden con la búsqueda
            var categoriesWithCount = await queryBusqueda
                .GroupBy(p => p.CodigoRubro)
                .Select(g => new { CodigoRubro = g.Key, Count = g.Count() })
                .ToListAsync();

            // Mapear TODAS las categorías con sus conteos (0 si no hay productos que coincidan)
            var categorias = todasLasCategorias.Select(c => 
            {
                var count = categoriesWithCount.FirstOrDefault(cc => cc.CodigoRubro == c.CodigoRubro)?.Count ?? 0;
                return new CategoryProductCount
                {
                    CategoryId = c.Id,
                    CodigoRubro = c.CodigoRubro,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    Icono = c.Icono,
                    Color = c.Color,
                    Orden = c.Orden,
                    ProductCount = count
                };
            }).ToList();

            return categorias;
        }

        public async Task<List<Domain.Entities.CatalogProduct>> GetProductsByAgrupacionIdsAsync(List<int> agrupacionIds, int empresaId, string? listaPrecioCodigo = null)
        {
            if (!agrupacionIds.Any())
            {
                return new List<Domain.Entities.CatalogProduct>();
            }

            // Obtener empresa y empresa principal
            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e => e.Id == empresaId);
            
            var empresaPrincipalId = empresa?.EmpresaPrincipalId ?? empresaId;

            // Resolver lista de precios por código o usar predeterminada
            int? listaPrecioId = null;
            if (!string.IsNullOrEmpty(listaPrecioCodigo))
            {
                listaPrecioId = await _listaPrecioRepository.GetIdByCodigoAsync(listaPrecioCodigo);
                if (!listaPrecioId.HasValue)
                {
                    return new List<Domain.Entities.CatalogProduct>(); // Si no se encuentra la lista de precios, retornar vacío
                }
            }
            else
            {
                if (empresa?.ListaPrecioPredeterminadaId.HasValue == true)
                {
                    listaPrecioId = empresa.ListaPrecioPredeterminadaId.Value;
                }
                else
                {
                    // Fallback a lista global predeterminada
                    listaPrecioId = await _listaPrecioRepository.GetDefaultListIdAsync();
                }
            }

            // Estrategia: obtener códigos de productos que tienen las agrupaciones especificadas en su Grupo1
            // Las agrupaciones tipo 1 corresponden al campo Grupo1 de los productos
            var codigosAgrupaciones = await _context.Agrupaciones
                .Where(a => agrupacionIds.Contains(a.Id) && a.Tipo == 1)
                .Select(a => a.Codigo)
                .ToListAsync();

            if (!codigosAgrupaciones.Any())
            {
                _logger.LogWarning("No se encontraron agrupaciones tipo 1 para los IDs {AgrupacionIds}", string.Join(",", agrupacionIds));
                return new List<Domain.Entities.CatalogProduct>();
            }

            var codigosProductos = await _context.ProductosBases
                .Where(pb => pb.AdministradoPorEmpresaId == empresaPrincipalId && 
                            pb.Grupo1.HasValue && 
                            codigosAgrupaciones.Contains(pb.Grupo1.Value))
                .Select(pb => pb.Codigo)
                .Distinct()
                .ToListAsync();

            if (!codigosProductos.Any())
            {
                _logger.LogWarning("No se encontraron productos para las agrupaciones {AgrupacionIds}", string.Join(",", agrupacionIds));
                return new List<Domain.Entities.CatalogProduct>();
            }

            // Para novedades, construir consulta manual sin los filtros restrictivos de la vista
            // IMPORTANTE: Siempre filtrar por una lista de precios específica para evitar duplicados
            if (!listaPrecioId.HasValue)
            {
                _logger.LogWarning("No se pudo determinar lista de precios para empresa {EmpresaId}", empresaId);
                return new List<Domain.Entities.CatalogProduct>();
            }

            var query = from pb in _context.ProductosBases
                       join pbp in _context.ProductosBasePrecios on pb.Id equals pbp.ProductoBaseId
                       join lp in _context.ListasPrecios on pbp.ListaPrecioId equals lp.Id
                       join emp in _context.Empresas on empresaId equals emp.Id
                       where pb.AdministradoPorEmpresaId == empresaPrincipalId &&
                             codigosProductos.Contains(pb.Codigo) &&
                             lp.Id == listaPrecioId.Value &&
                             lp.Activa == true
                       select new { pb, pbp, lp, emp };

            var resultados = await query
                .Select(x => new 
                {
                    Codigo = x.pb.Codigo,
                    Descripcion = x.pb.Descripcion ?? "",
                    DescripcionCorta = x.pb.DescripcionCorta ?? "",
                    Precio = x.pbp != null ? x.pbp.Precio : 0m,
                    Destacado = x.pb.Destacado ?? false,
                    ImagenUrl = x.pb.ImagenUrl,
                    Existencia = 0m, // Se obtiene después de la consulta desde ProductoBaseStockRepository
                    Tags = x.pb.Tags,
                    Marca = x.pb.Marca,
                    UnidadMedida = x.pb.UnidadMedida,
                    CodigoBarras = x.pb.CodigoBarras ?? "",
                    CodigoRubro = x.pb.CodigoRubro ?? 0,
                    ImagenAlt = x.pb.ImagenAlt ?? "",
                    EmpresaNombre = x.emp.Nombre ?? "",
                    ListaPrecioId = x.lp != null ? x.lp.Id : 0,
                    ListaPrecioNombre = x.lp != null ? x.lp.Nombre : "Sin precio",
                    ListaPrecioCodigo = x.lp != null ? x.lp.Codigo : "N/A"
                })
                .ToListAsync();

            // Obtener stock para los productos
            var codigosProductosStock = resultados.Select(x => x.Codigo).Distinct().ToList();
            var productosMap = await _context.ProductosBases
                .Where(pb => codigosProductosStock.Contains(pb.Codigo))
                .ToDictionaryAsync(pb => pb.Codigo, pb => pb.Id);

            var productIds = productosMap.Values.ToList();
            var stockPorProducto = await _stockRepository.GetStockBatchAsync(empresaId, productIds);

            // Filtrar solo productos con stock > 0 (igual que la vista SQL)
            var resultadosConStock = resultados.Where(x => 
            {
                if (productosMap.TryGetValue(x.Codigo, out var productId))
                {
                    return stockPorProducto.GetValueOrDefault(productId, 0) > 0;
                }
                return false;
            }).ToList();

            var productos = resultadosConStock.Select(x => new Domain.Entities.CatalogProduct
            {
                Codigo = x.Codigo.ToString(),
                Nombre = x.Descripcion, // Usar descripción como nombre para consistencia con catálogo
                Descripcion = x.Descripcion,
                DescripcionCorta = x.DescripcionCorta,
                Precio = x.Precio,
                Destacado = x.Destacado,
                ImagenUrl = x.ImagenUrl,
                Existencia = productosMap.TryGetValue(x.Codigo, out var productId) ? stockPorProducto.GetValueOrDefault(productId, 0) : 0m,
                Tags = !string.IsNullOrEmpty(x.Tags) ? x.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>(),
                Marca = x.Marca,
                UnidadMedida = x.UnidadMedida,
                CodigoBarras = x.CodigoBarras,
                CodigoRubro = x.CodigoRubro,
                ImagenAlt = x.ImagenAlt,
                TipoProducto = "base",
                EmpresaId = empresaId,
                EmpresaNombre = x.EmpresaNombre,
                ListaPrecioId = x.ListaPrecioId,
                ListaPrecioNombre = x.ListaPrecioNombre,
                ListaPrecioCodigo = x.ListaPrecioCodigo
            }).ToList();

            _logger.LogInformation("Obtenidos {Count} productos de agrupaciones {AgrupacionIds} para empresa {EmpresaId} (filtrados por stock > 0)",
                productos.Count, string.Join(",", agrupacionIds), empresaId);

            return productos;
        }
    }
}