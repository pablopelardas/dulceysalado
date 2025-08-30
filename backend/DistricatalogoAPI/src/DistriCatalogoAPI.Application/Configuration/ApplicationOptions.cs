namespace DistriCatalogoAPI.Application.Configuration
{
    public class ApplicationOptions
    {
        public const string SectionName = "Application";
        
        /// <summary>
        /// URL base de la API
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// URL del catálogo público para generar enlaces de corrección
        /// </summary>
        public string CatalogUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Ruta donde se almacenan las subidas de archivos
        /// </summary>
        public string UploadsPath { get; set; } = string.Empty;
    }
}