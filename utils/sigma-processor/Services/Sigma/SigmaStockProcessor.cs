using Microsoft.Extensions.Logging;
using SigmaProcessor.Models.Sigma;
using SigmaProcessor.Utils.Sigma;

namespace SigmaProcessor.Services.Sigma
{
    public class SigmaStockProcessor : ISigmaProcessor<SigmaStock>
    {
        private readonly ILogger<SigmaStockProcessor> _logger;
        private readonly XmlStreamParser _xmlParser;

        public SigmaStockProcessor(ILogger<SigmaStockProcessor> logger)
        {
            _logger = logger;
            _xmlParser = new XmlStreamParser();
        }

        public async Task<List<SigmaStock>> ProcessFileAsync(string filePath, string tempFolder)
        {
            _logger.LogInformation("Iniciando procesamiento de archivo stock SIGMA: {FilePath}", filePath);

            // Limpiar archivo XML
            var tempFilePath = Path.Combine(tempFolder, $"stock_clean_{DateTime.Now:yyyyMMdd_HHmmss}.xml");
            await _xmlParser.CleanXmlFileAsync(filePath, tempFilePath);

            // Parsear XML
            var xmlData = await _xmlParser.ParseXmlFileAsync(tempFilePath);
            _logger.LogInformation("Parseados {Count} registros de stock del XML", xmlData.Count);

            var stocks = new List<SigmaStock>();
            int successCount = 0;
            int errorCount = 0;

            foreach (var row in xmlData)
            {
                try
                {
                    var stock = MapXmlRowToStock(row);
                    if (IsValidStock(stock))
                    {
                        stocks.Add(stock);
                        successCount++;
                    }
                    else
                    {
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando registro de stock XML: {Row}", string.Join(", ", row.Select(kv => $"{kv.Key}={kv.Value}")));
                    errorCount++;
                }
            }

            // Limpiar archivo temporal
            try
            {
                File.Delete(tempFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar archivo temporal: {TempFile}", tempFilePath);
            }

            _logger.LogInformation("Procesamiento de stocks completado: {Success} exitosos, {Errors} errores", successCount, errorCount);
            return stocks;
        }

        public bool ValidateFileStructure(string filePath)
        {
            try
            {
                return _xmlParser.ValidateXmlStructure(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando estructura XML de stock: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<string> CleanFileAsync(string inputPath, string outputPath)
        {
            return await _xmlParser.CleanXmlFileAsync(inputPath, outputPath);
        }

        private SigmaStock MapXmlRowToStock(Dictionary<string, string> row)
        {
            return new SigmaStock
            {
                codart = SigmaEncodingHelper.CleanXmlValue(row.GetValueOrDefault("codart", "")),
                fdescri = SigmaEncodingHelper.CleanXmlValue(row.GetValueOrDefault("fdescri", "")),
                fstock = SigmaFieldMapper.ParseDecimalSafely(row.GetValueOrDefault("fstock", "0"))
            };
        }

        private bool IsValidStock(SigmaStock stock)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(stock.codart))
            {
                _logger.LogWarning("Stock sin código de artículo válido");
                return false;
            }

            // Ya no validamos que sea numérico, permitimos códigos alfanuméricos
            var cleanCode = stock.codart.Trim();
            if (cleanCode.Length == 0)
            {
                _logger.LogWarning("Stock {Codigo} tiene código vacío después de limpiar", stock.codart);
                return false;
            }

            // El stock puede ser 0 o positivo
            if (stock.fstock < 0)
            {
                return false;
            }

            return true;
        }

        public List<SigmaStock> ValidateStockProductMatches(List<SigmaStock> stocks, List<SigmaProduct> products)
        {
            _logger.LogInformation("Validando coincidencias entre {StockCount} stocks y {ProductCount} productos", 
                stocks.Count, products.Count);

            var productCodes = products.Select(p => p.fcodigo.Trim()).ToHashSet();
            var validStocks = new List<SigmaStock>();
            var unmatchedStocks = new List<string>();

            foreach (var stock in stocks)
            {
                var cleanStockCode = stock.codart.Trim();

                if (productCodes.Contains(cleanStockCode))
                {
                    validStocks.Add(stock);
                }
                else
                {
                    unmatchedStocks.Add(stock.codart);
                }
            }

            if (unmatchedStocks.Count > 0)
            {
                _logger.LogWarning("Encontrados {Count} stocks sin producto correspondiente: {Codes}", 
                    unmatchedStocks.Count, string.Join(", ", unmatchedStocks.Take(10)));
            }

            _logger.LogInformation("Validación completada: {Valid} stocks válidos de {Total} totales", 
                validStocks.Count, stocks.Count);

            return validStocks;
        }
    }
}