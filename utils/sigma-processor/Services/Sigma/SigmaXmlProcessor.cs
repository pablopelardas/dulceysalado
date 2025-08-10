using Microsoft.Extensions.Logging;
using SigmaProcessor.Models;
using SigmaProcessor.Models.Sigma;

namespace SigmaProcessor.Services.Sigma
{
    public class SigmaXmlProcessor
    {
        private readonly ILogger<SigmaXmlProcessor> _logger;
        private readonly SigmaProductProcessor _productProcessor;
        private readonly SigmaStockProcessor _stockProcessor;
        private readonly SigmaDataTransformer _dataTransformer;

        public SigmaXmlProcessor(
            ILogger<SigmaXmlProcessor> logger,
            SigmaProductProcessor productProcessor,
            SigmaStockProcessor stockProcessor,
            SigmaDataTransformer dataTransformer)
        {
            _logger = logger;
            _productProcessor = productProcessor;
            _stockProcessor = stockProcessor;
            _dataTransformer = dataTransformer;
        }

        public async Task<List<ProductWithPricesAndStock>> ProcessSigmaFilesAsync(
            string inputPath, 
            string tempPath,
            SigmaProcessConfig config)
        {
            _logger.LogInformation("=== INICIANDO PROCESAMIENTO SIGMA XML ===");
            _logger.LogInformation("Configuración: Empresa={EmpresaId}, Productos={ProductsFile}, Stock={StockFile}", 
                config.EmpresaId, config.ProductsFileName, config.StockFileName);

            var productsFilePath = Path.Combine(inputPath, config.ProductsFileName);
            var stockFilePath = Path.Combine(inputPath, config.StockFileName);

            // Validar existencia de archivos
            if (!File.Exists(productsFilePath))
            {
                throw new FileNotFoundException($"Archivo de productos no encontrado: {productsFilePath}");
            }

            if (!File.Exists(stockFilePath))
            {
                throw new FileNotFoundException($"Archivo de stock no encontrado: {stockFilePath}");
            }

            // Validar estructura de archivos XML
            if (!_productProcessor.ValidateFileStructure(productsFilePath))
            {
                throw new InvalidOperationException($"El archivo {config.ProductsFileName} no tiene una estructura XML válida");
            }

            if (!_stockProcessor.ValidateFileStructure(stockFilePath))
            {
                throw new InvalidOperationException($"El archivo {config.StockFileName} no tiene una estructura XML válida");
            }

            try
            {
                // Procesar productos
                _logger.LogInformation("Procesando archivo de productos: {File}", config.ProductsFileName);
                var products = await _productProcessor.ProcessFileAsync(productsFilePath, tempPath);
                _logger.LogInformation("Productos procesados: {Count}", products.Count);

                // Procesar stocks
                _logger.LogInformation("Procesando archivo de stock: {File}", config.StockFileName);
                var stocks = await _stockProcessor.ProcessFileAsync(stockFilePath, tempPath);
                _logger.LogInformation("Stocks procesados: {Count}", stocks.Count);

                // Validar coincidencias productos-stock
                var validatedStocks = _stockProcessor.ValidateStockProductMatches(stocks, products);
                _logger.LogInformation("Stocks validados contra productos: {ValidatedCount} de {TotalCount}", 
                    validatedStocks.Count, stocks.Count);

                // Transformar a formato API
                _logger.LogInformation("Transformando datos a formato API...");
                var transformedProducts = _dataTransformer.TransformToApiFormat(products, validatedStocks, config);
                _logger.LogInformation("Productos transformados: {Count}", transformedProducts.Count);

                // Log de estadísticas
                LogProcessingStatistics(transformedProducts, config);

                _logger.LogInformation("=== PROCESAMIENTO SIGMA XML COMPLETADO ===");
                return transformedProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el procesamiento SIGMA XML");
                throw;
            }
        }

        public bool DetectSigmaFiles(string inputPath, SigmaProcessConfig config)
        {
            var productsExists = File.Exists(Path.Combine(inputPath, config.ProductsFileName));
            var stockExists = File.Exists(Path.Combine(inputPath, config.StockFileName));

            _logger.LogInformation("Detección archivos SIGMA - Productos: {ProductsExists}, Stock: {StockExists}", 
                productsExists, stockExists);

            return productsExists && stockExists;
        }

        private void LogProcessingStatistics(List<ProductWithPricesAndStock> products, SigmaProcessConfig config)
        {
            var totalProducts = products.Count;
            var productsWithPrices = products.Count(p => p.ListasPrecios.Any());
            var productsWithStock = products.Count(p => p.StocksPorEmpresa.Any(s => s.Stock > 0));
            var activeProducts = products.Count(p => p.Disponible == "S");

            var totalPriceLists = products.SelectMany(p => p.ListasPrecios).Count();
            var averagePricesPerProduct = totalProducts > 0 ? (double)totalPriceLists / totalProducts : 0;

            var totalStock = products.SelectMany(p => p.StocksPorEmpresa).Sum(s => s.Stock);

            _logger.LogInformation("=== ESTADÍSTICAS PROCESAMIENTO SIGMA ===");
            _logger.LogInformation("Total productos: {Total}", totalProducts);
            _logger.LogInformation("Productos activos: {Active} ({Percentage:F1}%)", 
                activeProducts, totalProducts > 0 ? (double)activeProducts / totalProducts * 100 : 0);
            _logger.LogInformation("Productos con precios: {WithPrices} ({Percentage:F1}%)", 
                productsWithPrices, totalProducts > 0 ? (double)productsWithPrices / totalProducts * 100 : 0);
            _logger.LogInformation("Productos con stock: {WithStock} ({Percentage:F1}%)", 
                productsWithStock, totalProducts > 0 ? (double)productsWithStock / totalProducts * 100 : 0);
            _logger.LogInformation("Total listas de precios: {Total} (promedio: {Average:F1} por producto)", 
                totalPriceLists, averagePricesPerProduct);
            _logger.LogInformation("Stock total: {Total:F2}", totalStock);
            
            // Estadísticas de mapeo de listas de precios
            foreach (var mapping in config.PriceListMapping)
            {
                var pricesInList = products.SelectMany(p => p.ListasPrecios)
                                          .Count(lp => lp.ListaId == mapping.Value);
                _logger.LogInformation("Lista SIGMA {SigmaList} → API {ApiList}: {Count} precios", 
                    mapping.Key, mapping.Value, pricesInList);
            }

            // Estadísticas de categorías y grupos
            var categoriesUsed = products.Where(p => p.CodigoRubro.HasValue)
                                        .GroupBy(p => p.CodigoRubro.Value)
                                        .ToDictionary(g => g.Key, g => g.Count());

            var grupo2Used = products.Where(p => p.Grupo2.HasValue)
                                   .GroupBy(p => p.Grupo2.Value)
                                   .ToDictionary(g => g.Key, g => g.Count());

            _logger.LogInformation("Categorías distintas (fgrupo→categoria_id): {Count}", categoriesUsed.Count);
            _logger.LogInformation("Grupos 2 distintos (frubro→grupo2): {Count}", grupo2Used.Count);

            _logger.LogInformation("=======================================");
        }
    }
}