using Microsoft.Extensions.Logging;
using SigmaProcessor.Models.Sigma;

namespace SigmaProcessor.Services.Sigma
{
    public class SigmaValidator
    {
        private readonly ILogger<SigmaValidator> _logger;

        public SigmaValidator(ILogger<SigmaValidator> logger)
        {
            _logger = logger;
        }

        public (bool IsValid, List<string> ValidationErrors) ValidateConfiguration(SigmaProcessConfig config)
        {
            var errors = new List<string>();

            if (!config.Enabled)
            {
                return (false, new List<string> { "Configuración SIGMA está deshabilitada" });
            }

            if (string.IsNullOrWhiteSpace(config.ProductsFileName))
                errors.Add("ProductsFileName no puede estar vacío");

            if (string.IsNullOrWhiteSpace(config.StockFileName))
                errors.Add("StockFileName no puede estar vacío");

            if (config.EmpresaId <= 0)
                errors.Add("EmpresaId debe ser mayor a 0");

            if (config.PriceListMapping == null || !config.PriceListMapping.Any())
                errors.Add("PriceListMapping debe contener al menos una entrada");

            if (config.CategoryMapping == null)
                errors.Add("CategoryMapping no puede ser null");

            return (errors.Count == 0, errors);
        }

        public (bool IsValid, List<string> ValidationErrors) ValidateFilesExist(string inputPath, SigmaProcessConfig config)
        {
            var errors = new List<string>();

            var productsPath = Path.Combine(inputPath, config.ProductsFileName);
            var stockPath = Path.Combine(inputPath, config.StockFileName);

            if (!File.Exists(productsPath))
                errors.Add($"Archivo de productos no encontrado: {productsPath}");

            if (!File.Exists(stockPath))
                errors.Add($"Archivo de stock no encontrado: {stockPath}");

            // Validación adicional del archivo de clientes (opcional)
            if (!string.IsNullOrWhiteSpace(config.ClientsFileName))
            {
                var clientsPath = Path.Combine(inputPath, config.ClientsFileName);
                if (!File.Exists(clientsPath))
                {
                    _logger.LogWarning("Archivo de clientes no encontrado (opcional): {ClientsPath}", clientsPath);
                }
            }

            return (errors.Count == 0, errors);
        }

        public List<string> ValidateProductData(List<SigmaProduct> products)
        {
            var warnings = new List<string>();
            var duplicates = products.GroupBy(p => p.fcodigo)
                                   .Where(g => g.Count() > 1)
                                   .Select(g => g.Key)
                                   .ToList();

            if (duplicates.Any())
            {
                warnings.Add($"Productos duplicados encontrados: {string.Join(", ", duplicates)}");
            }

            var productsWithoutCategory = products.Where(p => string.IsNullOrWhiteSpace(p.fgrupo)).Count();
            if (productsWithoutCategory > 0)
            {
                warnings.Add($"Productos sin categoría (fgrupo): {productsWithoutCategory}");
            }

            var productsWithoutPrices = products.Where(p => p.flispr2 == 0 && p.flispr4 == 0).Count();
            if (productsWithoutPrices > 0)
            {
                warnings.Add($"Productos sin precios: {productsWithoutPrices}");
            }

            var inactiveProducts = products.Where(p => !string.Equals(p.fdelete, "N", StringComparison.OrdinalIgnoreCase)).Count();
            if (inactiveProducts > 0)
            {
                _logger.LogInformation("Productos inactivos (fdelete != 'N'): {Count}", inactiveProducts);
            }

            return warnings;
        }

        public List<string> ValidateStockData(List<SigmaStock> stocks)
        {
            var warnings = new List<string>();

            var duplicates = stocks.GroupBy(s => s.codart)
                                  .Where(g => g.Count() > 1)
                                  .Select(g => g.Key)
                                  .ToList();

            if (duplicates.Any())
            {
                warnings.Add($"Stocks duplicados encontrados: {string.Join(", ", duplicates)}");
            }

            var negativeStocks = stocks.Where(s => s.fstock < 0).Count();
            if (negativeStocks > 0)
            {
                warnings.Add($"Stocks negativos encontrados: {negativeStocks}");
            }

            var zeroStocks = stocks.Where(s => s.fstock == 0).Count();
            if (zeroStocks > 0)
            {
                _logger.LogInformation("Productos con stock cero: {Count}", zeroStocks);
            }

            return warnings;
        }

        public List<string> ValidateProductStockConsistency(List<SigmaProduct> products, List<SigmaStock> stocks)
        {
            var warnings = new List<string>();

            var productCodes = products.Select(p => p.fcodigo.TrimStart('0')).Where(c => !string.IsNullOrEmpty(c)).ToHashSet();
            var stockCodes = stocks.Select(s => s.codart.TrimStart('0')).Where(c => !string.IsNullOrEmpty(c)).ToHashSet();

            // Productos sin stock
            var productsWithoutStock = productCodes.Except(stockCodes).ToList();
            if (productsWithoutStock.Any())
            {
                warnings.Add($"Productos sin registro de stock: {productsWithoutStock.Count} (ej: {string.Join(", ", productsWithoutStock.Take(5))})");
            }

            // Stocks sin producto
            var stocksWithoutProduct = stockCodes.Except(productCodes).ToList();
            if (stocksWithoutProduct.Any())
            {
                warnings.Add($"Stocks sin producto correspondiente: {stocksWithoutProduct.Count} (ej: {string.Join(", ", stocksWithoutProduct.Take(5))})");
            }

            var matchingCount = productCodes.Intersect(stockCodes).Count();
            var matchPercentage = productCodes.Count > 0 ? (double)matchingCount / productCodes.Count * 100 : 0;

            _logger.LogInformation("Coincidencias producto-stock: {Matching}/{Total} ({Percentage:F1}%)", 
                matchingCount, productCodes.Count, matchPercentage);

            return warnings;
        }
    }
}