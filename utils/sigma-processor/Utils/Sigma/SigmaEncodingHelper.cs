namespace SigmaProcessor.Utils.Sigma
{
    public static class SigmaEncodingHelper
    {
        public static string DetectAndFixEncoding(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Detectar si el texto tiene problemas de encoding común
            var commonEncodingIssues = new Dictionary<string, string>
            {
                { "Ã³", "ó" },
                { "Ã¡", "á" }, 
                { "Ã©", "é" },
                { "Ã­", "í" },
                { "Ãº", "ú" },
                { "Ã±", "ñ" },
                { "Ã‡", "Ç" },
                { "Ã¼", "ü" },
                { "Ã¤", "ä" },
                { "Ã¶", "ö" }
            };

            foreach (var fix in commonEncodingIssues)
            {
                input = input.Replace(fix.Key, fix.Value);
            }

            return input;
        }

        public static string NormalizeSpaces(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Reemplazar múltiples espacios con uno solo
            return System.Text.RegularExpressions.Regex.Replace(input.Trim(), @"\s+", " ");
        }

        public static string CleanXmlValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            value = DetectAndFixEncoding(value);
            value = NormalizeSpaces(value);
            
            // Limpiar comillas si están al inicio y final
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Substring(1, value.Length - 2);
            }

            return value;
        }
    }
}