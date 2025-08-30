using System;

namespace DistriCatalogoAPI.Application.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo ArgentinaTimeZone = GetArgentinaTimeZone();

        private static TimeZoneInfo GetArgentinaTimeZone()
        {
            try
            {
                // Intentar Windows ID primero
                return TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
            }
            catch
            {
                try
                {
                    // Intentar IANA ID para Linux/Mac
                    return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
                }
                catch
                {
                    // Fallback: crear zona horaria UTC-3 manual
                    return TimeZoneInfo.CreateCustomTimeZone(
                        "Argentina Custom", 
                        TimeSpan.FromHours(-3), 
                        "Argentina Standard Time", 
                        "Argentina Standard Time");
                }
            }
        }

        /// <summary>
        /// Convierte una fecha UTC a hora local de Argentina
        /// </summary>
        public static DateTime ToArgentinaTime(DateTime utcDateTime)
        {
            try
            {
                // Asegurar que la fecha está en UTC
                var utcTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
                // Convertir a hora de Argentina
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, ArgentinaTimeZone);
            }
            catch
            {
                // Si hay error con la zona horaria, usar UTC-3 manualmente
                return utcDateTime.AddHours(-3);
            }
        }

        /// <summary>
        /// Convierte una fecha nullable UTC a hora local de Argentina
        /// </summary>
        public static DateTime? ToArgentinaTime(DateTime? utcDateTime)
        {
            if (!utcDateTime.HasValue)
                return null;
                
            return ToArgentinaTime(utcDateTime.Value);
        }

        /// <summary>
        /// Obtiene la fecha y hora actual en Argentina
        /// </summary>
        public static DateTime NowInArgentina()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ArgentinaTimeZone);
        }
    }
}