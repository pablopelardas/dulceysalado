using System.Text;
using System.Xml;

namespace SigmaProcessor.Utils.Sigma
{
    public class XmlStreamParser
    {
        public async Task<List<Dictionary<string, string>>> ParseXmlFileAsync(string filePath)
        {
            var results = new List<Dictionary<string, string>>();
            
            using var reader = XmlReader.Create(filePath, new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
                DtdProcessing = DtdProcessing.Ignore,
                Async = true
            });

            while (await reader.ReadAsync())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "ROW")
                {
                    var row = new Dictionary<string, string>();
                    
                    if (reader.HasAttributes)
                    {
                        for (int i = 0; i < reader.AttributeCount; i++)
                        {
                            reader.MoveToAttribute(i);
                            row[reader.Name] = reader.Value ?? string.Empty;
                        }
                    }
                    
                    reader.MoveToElement();
                    results.Add(row);
                }
            }

            return results;
        }

        public bool ValidateXmlStructure(string filePath)
        {
            try
            {
                using var reader = XmlReader.Create(filePath, new XmlReaderSettings
                {
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    DtdProcessing = DtdProcessing.Ignore,
                    Async = false  // No usamos async en validación
                });

                bool hasRowElements = false;
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "ROW")
                    {
                        hasRowElements = true;
                        break;
                    }
                }

                return hasRowElements;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> CleanXmlFileAsync(string inputPath, string outputPath)
        {
            var content = await File.ReadAllTextAsync(inputPath);
            
            // Limpiar caracteres problemáticos
            content = content.Replace("&#160;", " "); // Espacio no separable
            content = FixEncoding(content);
            
            await File.WriteAllTextAsync(outputPath, content, Encoding.UTF8);
            return outputPath;
        }

        private string FixEncoding(string content)
        {
            // Correcciones básicas de encoding UTF-8 mal interpretado
            content = content.Replace("Ã³", "ó");
            content = content.Replace("Ã¡", "á");
            content = content.Replace("Ã©", "é");
            content = content.Replace("Ã­", "í");
            content = content.Replace("Ãº", "ú");
            content = content.Replace("Ã±", "ñ");
            content = content.Replace("Ã¼", "ü");
            content = content.Replace("Ã¤", "ä");
            content = content.Replace("Ã¶", "ö");

            return content;
        }
    }
}