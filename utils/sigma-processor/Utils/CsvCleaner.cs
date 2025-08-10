using System.Text;
using System.Text.RegularExpressions;

namespace SigmaProcessor.Utils;

public static class CsvCleaner
{
    private static readonly Dictionary<string, string> EncodingFixes = new()
    {
        { "CÃ³digo", "Codigo" },
        { "DescripciÃ³n", "Descripcion" },
        { "caPrecio", "Precio" },
        { "C�digo", "Codigo" },
        { "Descripci�n", "Descripcion" },
        { "Art�culo", "Articulo" },
        { "Art?culo", "Articulo" },
        { "Ã³", "o" },
        { "Ã±", "ñ" },
        { "Ã", "a" },
        { "Ã©", "e" },
        { "Ã­", "i" },
        { "Ãº", "u" }
    };

    public static async Task<string> CleanCsvFileAsync(string inputFilePath, string outputFilePath)
    {
        try
        {
            // Intentar detectar el encoding correcto
            var encoding = DetectEncoding(inputFilePath);
            var lines = await File.ReadAllLinesAsync(inputFilePath, encoding);
            var cleanedLines = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                
                // Aplicar correcciones de encoding si es necesario
                line = FixEncodingIssues(line);
                
                if (i == 0)
                {
                    line = FixHeaders(line);
                }
                else
                {
                    line = CleanDataLine(line);
                }

                cleanedLines.Add(line);
            }

            await File.WriteAllLinesAsync(outputFilePath, cleanedLines, Encoding.UTF8);
            return outputFilePath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error cleaning CSV file: {ex.Message}", ex);
        }
    }

    public static string FixHeaders(string headerLine)
    {
        var cleanHeader = headerLine;

        foreach (var fix in EncodingFixes)
        {
            cleanHeader = cleanHeader.Replace(fix.Key, fix.Value);
        }

        // Correcciones específicas para headers con acentos a sin acentos
        cleanHeader = cleanHeader.Replace("Código", "Codigo");
        cleanHeader = cleanHeader.Replace("Descripción", "Descripcion");
        cleanHeader = cleanHeader.Replace("Artículo", "Articulo");

        // Detectar el delimitador original y convertir a coma
        if (cleanHeader.Contains(";"))
        {
            cleanHeader = cleanHeader.Replace(";", ",");
        }
        else if (cleanHeader.Contains("\t"))
        {
            cleanHeader = cleanHeader.Replace("\t", ",");
        }

        return cleanHeader;
    }

    private static string CleanDataLine(string dataLine)
    {
        // PRIMERO: Arreglar separadores decimales ANTES de cambiar delimitadores
        dataLine = FixDecimalSeparators(dataLine);
        
        // SEGUNDO: Detectar el delimitador y convertir a coma
        if (dataLine.Contains(";"))
        {
            dataLine = dataLine.Replace(";", ",");
        }
        else if (!dataLine.Contains(','))
        {
            dataLine = dataLine.Replace("\t", ",");
        }
        
        // TERCERO: Remover comillas
        dataLine = RemoveQuotes(dataLine);
        
        // CUARTO: Arreglar valores NULL
        dataLine = FixNullValues(dataLine);

        return dataLine;
    }

    private static string RemoveQuotes(string line)
    {
        return Regex.Replace(line, "\"([^\"]*?)\"", "$1");
    }

    private static string FixDecimalSeparators(string line)
    {
        // Patrón para números con separadores de miles (punto) y decimales (coma)
        // Debe funcionar con punto y coma como delimitador original
        // Ejemplo: ";3.382,600;" -> ";3382.600;"
        var pattern = @"(?<=;)(\d{1,3}(?:\.\d{1,3})*),(\d+)(?=;|$)";
        
        var result = Regex.Replace(line, pattern, match =>
        {
            var beforeComma = match.Groups[1].Value;
            var afterComma = match.Groups[2].Value;
            
            // Remover puntos (separadores de miles) y cambiar coma por punto decimal
            var cleanNumber = beforeComma.Replace(".", "") + "." + afterComma;
            return cleanNumber;
        });

        return result;
    }

    private static bool IsDecimalField(string value)
    {
        return Regex.IsMatch(value, @"^\d+[,.]\d+$");
    }

    private static string FixNullValues(string line)
    {
        return line.Replace("NULL", "");
    }

    public static Encoding DetectEncoding(string filePath)
    {
        try
        {
            // IMPORTANTE: Registrar los code pages del sistema para poder usar Windows-1252 e ISO-8859-1
            // Esto es necesario en .NET Core/5+ para poder usar estos encodings
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
        catch { }

        // Leer los primeros bytes para detectar BOM o hacer una detección heurística
        var bytes = File.ReadAllBytes(filePath).Take(1024).ToArray();
        
        // Verificar BOM UTF-8
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return Encoding.UTF8;
        }
        
        // Intentar con Windows-1252 (muy común para CSVs de Windows con acentos)
        try
        {
            var testContent = Encoding.GetEncoding("Windows-1252").GetString(bytes);
            if (testContent.Contains("Código") || testContent.Contains("Descripción"))
            {
                return Encoding.GetEncoding("Windows-1252");
            }
        }
        catch 
        { 
            Console.WriteLine("DEBUG - No se pudo usar Windows-1252");
        }
        
        // Intentar con ISO-8859-1
        try
        {
            var testContent = Encoding.GetEncoding("ISO-8859-1").GetString(bytes);
            if (testContent.Contains("Código") || testContent.Contains("Descripción"))
            {
                return Encoding.GetEncoding("ISO-8859-1");
            }
        }
        catch 
        { 
            Console.WriteLine("DEBUG - No se pudo usar ISO-8859-1");
        }
        
        // Fallback a UTF-8
        Console.WriteLine("DEBUG - Usando encoding UTF-8 por defecto");
        return Encoding.UTF8;
    }

    public static string FixEncodingIssues(string line)
    {
        var result = line;

        // Aplicar correcciones específicas en orden sin usar Dictionary
        // para evitar conflictos con el carácter � de reemplazo
        
        // Palabras completas primero
        result = result.Replace("C�digo", "Código");
        result = result.Replace("Descripci�n", "Descripción");
        
        // Secuencias UTF-8 mal interpretadas
        result = result.Replace("Ã³", "ó");
        result = result.Replace("Ã±", "ñ");
        result = result.Replace("Ã¡", "á");
        result = result.Replace("Ã©", "é");
        result = result.Replace("Ã­", "í");
        result = result.Replace("Ãº", "ú");
        result = result.Replace("Ã", "á");
        
        // Otros casos comunes de encoding incorrecto
        result = result.Replace("â€™", "'");
        result = result.Replace("â€œ", "\"");
        result = result.Replace("â€�", "\"");
        
        return result;
    }

    public static bool ValidateCsvStructure(string filePath)
    {
        try
        {
            var encoding = DetectEncoding(filePath);
            var firstLine = File.ReadLines(filePath, encoding).FirstOrDefault();
            if (string.IsNullOrEmpty(firstLine))
            {
                Console.WriteLine($"DEBUG - Archivo vacío o sin líneas: {filePath}");
                return false;
            }

            Console.WriteLine($"DEBUG - Línea original: {firstLine}");
            
            // Aplicar correcciones de encoding y limpiar headers
            firstLine = FixEncodingIssues(firstLine);
            Console.WriteLine($"DEBUG - Después de FixEncodingIssues: {firstLine}");
            
            var cleanedFirstLine = FixHeaders(firstLine);
            Console.WriteLine($"DEBUG - Después de FixHeaders: {cleanedFirstLine}");
            
            var expectedHeaders = new[]
            {
                "Codigo", "Descripcion", "CodigoRubro", "Precio", "Existencia",
                "Grupo1", "Grupo2", "Grupo3", "FechaAlta", "FechaModi",
                "Imputable", "Disponible", "CodigoUbicacion"
            };

            var headers = cleanedFirstLine.Split(',').Select(h => h.Trim().Trim('"')).ToArray();
            Console.WriteLine($"DEBUG - Headers encontrados: [{string.Join(", ", headers)}]");
            
            // Verificar que al menos tengamos los campos principales
            var requiredHeaders = new[] { "Codigo", "Descripcion", "Precio" };
            
            foreach (var required in requiredHeaders)
            {
                var found = headers.Any(actual => actual.Equals(required, StringComparison.OrdinalIgnoreCase));
                Console.WriteLine($"DEBUG - Header '{required}' encontrado: {found}");
            }
            
            var isValid = requiredHeaders.All(required => 
                headers.Any(actual => actual.Equals(required, StringComparison.OrdinalIgnoreCase)));
                
            Console.WriteLine($"DEBUG - Validación resultado: {isValid}");
            return isValid;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DEBUG - Error en validación: {ex.Message}");
            return false;
        }
    }
}