using Microsoft.Extensions.Logging;
using SigmaProcessor.Models;
using SigmaProcessor.Models.Sigma;
using SigmaProcessor.Utils.Sigma;

namespace SigmaProcessor.Services.Sigma
{
    public class SigmaDataTransformer : ISigmaDataTransformer
    {
        private readonly ILogger<SigmaDataTransformer> _logger;

        public SigmaDataTransformer(ILogger<SigmaDataTransformer> logger)
        {
            _logger = logger;
        }

        public List<ProductWithPricesAndStock> TransformToApiFormat(
            List<SigmaProduct> products,
            List<SigmaStock> stocks,
            SigmaProcessConfig config)
        {
            _logger.LogInformation("Iniciando transformación de {ProductCount} productos y {StockCount} stocks a formato API",
                products.Count, stocks.Count);

            // Crear diccionario de stocks por código para búsqueda rápida
            var stocksDict = stocks.GroupBy(s => s.codart.Trim())
                                  .ToDictionary(g => g.Key, g => g.First());

            var transformedProducts = new List<ProductWithPricesAndStock>();
            int successCount = 0;
            int errorCount = 0;

            foreach (var product in products)
            {
                try
                {
                    var transformedProduct = TransformProduct(product, stocksDict, config);
                    if (transformedProduct != null)
                    {
                        transformedProducts.Add(transformedProduct);
                        successCount++;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error transformando producto {Codigo}: {Nombre}", 
                        product.fcodigo, product.artnom);
                    errorCount++;
                }
            }

            _logger.LogInformation("Transformación completada: {Success} exitosos, {Errors} errores", 
                successCount, errorCount);

            return transformedProducts;
        }

        private ProductWithPricesAndStock? TransformProduct(
            SigmaProduct product, 
            Dictionary<string, SigmaStock> stocksDict, 
            SigmaProcessConfig config)
        {
            // Validar que el código no esté vacío
            if (string.IsNullOrWhiteSpace(product.fcodigo))
            {
                _logger.LogWarning("Producto tiene código vacío o nulo");
                return null;
            }

            var apiProduct = new ProductWithPricesAndStock
            {
                // Campos básicos del producto - mantener código como string original
                Codigo = product.fcodigo.Trim(),
                Descripcion = product.artnom,
                
                // Mapeo CORREGIDO: fgrupo → categoria_id (NO frubro)
                CodigoRubro = SigmaFieldMapper.GetCategoryFromGroup(product.fgrupo),
                
                // Campos de agrupación
                Grupo1 = null, // No usado según spec
                Grupo2 = SigmaFieldMapper.GetGroup2FromRubro(product.frubro), // frubro → grupo2
                Grupo3 = null, // No usado según spec
                
                // Estados del producto
                Imputable = "S", // Default visible
                Disponible = SigmaFieldMapper.IsProductActive(product.fdelete) ? "S" : "N",
                
                // Fechas
                FechaAlta = DateTime.Now,
                FechaModi = DateTime.Now,
                
                // Campo de ubicación
                CodigoUbicacion = null, // No disponible en SIGMA
                
                // Listas de precios
                ListasPrecios = BuildPriceLists(product, config),
                
                // Stocks por empresa
                StocksPorEmpresa = BuildStocksByCompany(product, stocksDict, config)
            };

            // Log de transformación para debugging
            _logger.LogDebug("Transformado producto {Codigo}: Categoria={Categoria}, Grupo2={Grupo2}, Precios={PreciosCount}, Stock={Stock}",
                apiProduct.Codigo, 
                apiProduct.CodigoRubro, 
                apiProduct.Grupo2,
                apiProduct.ListasPrecios.Count,
                apiProduct.StocksPorEmpresa.FirstOrDefault()?.Stock ?? 0);

            return apiProduct;
        }

        private List<PriceList> BuildPriceLists(SigmaProduct product, SigmaProcessConfig config)
        {
            var priceLists = new List<PriceList>();

            // Mapear flispr2 (Lista SIGMA 2 → API Lista según configuración)
            if (product.flispr2 > 0 && config.PriceListMapping.ContainsKey("2"))
            {
                priceLists.Add(new PriceList
                {
                    ListaId = config.PriceListMapping["2"],
                    Precio = product.flispr2,
                    Fecha = DateTime.Now
                });
            }

            // Mapear flispr4 (Lista SIGMA 4 → API Lista según configuración)  
            if (product.flispr4 > 0 && config.PriceListMapping.ContainsKey("4"))
            {
                priceLists.Add(new PriceList
                {
                    ListaId = config.PriceListMapping["4"],
                    Precio = product.flispr4,
                    Fecha = DateTime.Now
                });
            }

            return priceLists;
        }

        private List<EmpresaStock> BuildStocksByCompany(
            SigmaProduct product, 
            Dictionary<string, SigmaStock> stocksDict, 
            SigmaProcessConfig config)
        {
            var empresaStocks = new List<EmpresaStock>();
            
            // Buscar stock para este producto (mantener código exacto)
            var productCode = product.fcodigo.Trim();

            decimal stockValue = 0;
            if (stocksDict.TryGetValue(productCode, out var stockRecord))
            {
                stockValue = stockRecord.fstock;
            }
            
            // Agregar stock para la empresa configurada
            empresaStocks.Add(new EmpresaStock
            {
                EmpresaId = config.EmpresaId,
                Stock = stockValue
            });

            return empresaStocks;
        }

        public PriceList MapPriceList(decimal price, int sigmaListId, SigmaProcessConfig config)
        {
            var apiListId = SigmaFieldMapper.MapSigmaListToApiList(sigmaListId.ToString(), config);
            
            return new PriceList
            {
                ListaId = apiListId,
                Precio = price,
                Fecha = DateTime.Now
            };
        }

        public EmpresaStock MapStock(SigmaStock stock, int empresaId)
        {
            return new EmpresaStock
            {
                EmpresaId = empresaId,
                Stock = stock.fstock
            };
        }
    }
}