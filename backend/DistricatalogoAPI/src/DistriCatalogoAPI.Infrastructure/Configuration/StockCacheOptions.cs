namespace DistriCatalogoAPI.Infrastructure.Configuration
{
    public class StockCacheOptions
    {
        public const string SectionName = "StockCache";
        
        /// <summary>
        /// Tiempo de vida del caché en horas. Por defecto 6 horas.
        /// </summary>
        public int TtlHours { get; set; } = 6;
        
        /// <summary>
        /// Tamaño máximo del caché (número de entradas). Por defecto 10000.
        /// </summary>
        public int MaxCacheSize { get; set; } = 10000;
        
        /// <summary>
        /// Habilitar estadísticas de caché para diagnóstico. Por defecto true.
        /// </summary>
        public bool EnableCacheStatistics { get; set; } = true;
        
        /// <summary>
        /// Porcentaje de compactación cuando se alcanza el límite. Por defecto 0.2 (20%).
        /// </summary>
        public double CompactionPercentage { get; set; } = 0.2;
        
        /// <summary>
        /// Obtiene el TimeSpan del TTL basado en TtlHours
        /// </summary>
        public TimeSpan GetTtlTimeSpan() => TimeSpan.FromHours(TtlHours);
    }
}