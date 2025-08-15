using System.Globalization;
using SigmaProcessor.Models.Sigma;

namespace SigmaProcessor.Utils.Sigma
{
    public static class SigmaFieldMapper
    {
        public static int? ParseIntSafely(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Remover ceros a la izquierda para convertir "010" -> 10
            value = value.TrimStart('0');
            if (string.IsNullOrEmpty(value))
                return 0;

            if (int.TryParse(value, out int result))
                return result;

            return null;
        }

        public static decimal ParseDecimalSafely(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0m;

            // Limpiar espacios y normalizar formato decimal europeo
            value = value.Trim();
            
            // Detectar formato europeo: punto como separador de miles, coma como decimal
            // Ejemplos: "3.430,00", "5.557,50", "387,56"
            if (value.Contains(","))
            {
                // Si tiene tanto punto como coma, el punto es separador de miles
                if (value.Contains(".") && value.Contains(","))
                {
                    // Formato: "3.430,00" → "3430.00"
                    var parts = value.Split(',');
                    if (parts.Length == 2)
                    {
                        var integerPart = parts[0].Replace(".", ""); // Remover separadores de miles
                        var decimalPart = parts[1];
                        value = $"{integerPart}.{decimalPart}";
                    }
                }
                else
                {
                    // Solo coma, es separador decimal: "387,56" → "387.56"
                    value = value.Replace(",", ".");
                }
            }
            
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            return 0m;
        }

        public static bool IsProductActive(string fdelete)
        {
            // fdelete="N" significa activo, cualquier otra cosa es inactivo
            return string.Equals(fdelete, "N", StringComparison.OrdinalIgnoreCase);
        }

        public static int MapSigmaListToApiList(string sigmaListId, SigmaProcessConfig config)
        {
            if (config.PriceListMapping.TryGetValue(sigmaListId, out int apiListId))
                return apiListId;

            // Fallback - si no está configurado, usar el mismo ID
            if (int.TryParse(sigmaListId, out int fallbackId))
                return fallbackId;

            return 1; // Default a lista 1
        }

        public static int? GetCategoryFromGroup(string fgrupo)
        {
            // fgrupo es la categoría según la corrección
            var result = ParseIntSafely(fgrupo);
            
            
            return result;
        }

        public static int? GetGroup2FromRubro(string frubro)
        {
            // frubro va a grupo2 según la corrección
            return ParseIntSafely(frubro);
        }
    }
}