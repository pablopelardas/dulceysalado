using Microsoft.Extensions.Logging;
using SigmaProcessor.Models.Sigma;
using SigmaProcessor.Utils.Sigma;

namespace SigmaProcessor.Services.Sigma
{
    public class SigmaProductProcessor : ISigmaProcessor<SigmaProduct>
    {
        private readonly ILogger<SigmaProductProcessor> _logger;
        private readonly XmlStreamParser _xmlParser;

        public SigmaProductProcessor(ILogger<SigmaProductProcessor> logger)
        {
            _logger = logger;
            _xmlParser = new XmlStreamParser();
        }

        public async Task<List<SigmaProduct>> ProcessFileAsync(string filePath, string tempFolder)
        {
            _logger.LogInformation("Iniciando procesamiento de archivo productos SIGMA: {FilePath}", filePath);

            // Limpiar archivo XML
            var tempFilePath = Path.Combine(tempFolder, $"productos_clean_{DateTime.Now:yyyyMMdd_HHmmss}.xml");
            await _xmlParser.CleanXmlFileAsync(filePath, tempFilePath);

            // Parsear XML
            var xmlData = await _xmlParser.ParseXmlFileAsync(tempFilePath);
            _logger.LogInformation("Parseados {Count} registros del XML", xmlData.Count);

            var products = new List<SigmaProduct>();
            int successCount = 0;
            int errorCount = 0;

            foreach (var row in xmlData)
            {
                try
                {
                    var product = MapXmlRowToProduct(row);
                    if (IsValidProduct(product))
                    {
                        products.Add(product);
                        successCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Producto inválido saltado: {Codigo}", row.GetValueOrDefault("fcodigo", "N/A"));
                        errorCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando registro XML: {Row}", string.Join(", ", row.Select(kv => $"{kv.Key}={kv.Value}")));
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

            _logger.LogInformation("Procesamiento completado: {Success} exitosos, {Errors} errores", successCount, errorCount);
            return products;
        }

        public bool ValidateFileStructure(string filePath)
        {
            try
            {
                return _xmlParser.ValidateXmlStructure(filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando estructura XML: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<string> CleanFileAsync(string inputPath, string outputPath)
        {
            return await _xmlParser.CleanXmlFileAsync(inputPath, outputPath);
        }

        private SigmaProduct MapXmlRowToProduct(Dictionary<string, string> row)
        {
            var fcodigo = SigmaEncodingHelper.CleanXmlValue(row.GetValueOrDefault("fcodigo", ""));
            
            // Debug raw values from XML before cleaning
            var rawFgrupo = row.GetValueOrDefault("fgrupo", "");
            var rawGrunom = row.GetValueOrDefault("grunom", "");
            
            var fgrupo = SigmaEncodingHelper.CleanXmlValue(rawFgrupo);
            var grunom = SigmaEncodingHelper.CleanXmlValue(rawGrunom);
            
            
            return new SigmaProduct
            {
                fcodigo = fcodigo,
                artnom = SigmaEncodingHelper.CleanXmlValue(row.GetValueOrDefault("artnom", "")),
                fgrupo = fgrupo,
                grunom = grunom,
                frubro = SigmaEncodingHelper.CleanXmlValue(row.GetValueOrDefault("frubro", "")),
                rubnom = SigmaEncodingHelper.CleanXmlValue(row.GetValueOrDefault("rubnom", "")),
                codprov = SigmaEncodingHelper.CleanXmlValue(row.GetValueOrDefault("codprov", "")),
                pronom = SigmaEncodingHelper.CleanXmlValue(row.GetValueOrDefault("pronom", "")),
                fdelete = SigmaEncodingHelper.CleanXmlValue(row.GetValueOrDefault("fdelete", "N")),
                fuxb = SigmaFieldMapper.ParseDecimalSafely(row.GetValueOrDefault("fuxb", "0")),
                fuxd = SigmaFieldMapper.ParseDecimalSafely(row.GetValueOrDefault("fuxd", "0")),
                fcanmin = SigmaFieldMapper.ParseDecimalSafely(row.GetValueOrDefault("fcanmin", "0")),
                flispr2 = SigmaFieldMapper.ParseDecimalSafely(row.GetValueOrDefault("flispr2", "0")),
                flispr4 = SigmaFieldMapper.ParseDecimalSafely(row.GetValueOrDefault("flispr4", "0")),
                foto = SigmaEncodingHelper.CleanXmlValue(row.GetValueOrDefault("foto", ""))
            };
        }

        private bool IsValidProduct(SigmaProduct product)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(product.fcodigo))
            {
                _logger.LogWarning("Producto sin código válido");
                return false;
            }

            if (string.IsNullOrWhiteSpace(product.artnom))
            {
                _logger.LogWarning("Producto {Codigo} sin nombre válido", product.fcodigo);
                return false;
            }

            // Ya no validamos que sea numérico, permitimos códigos alfanuméricos
            // Solo verificamos que no tenga caracteres problemáticos
            var cleanCode = product.fcodigo.Trim();
            if (cleanCode.Length == 0)
            {
                _logger.LogWarning("Producto {Codigo} tiene código vacío después de limpiar", product.fcodigo);
                return false;
            }

            return true;
        }
    }
}