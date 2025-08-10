namespace DistriCatalogoAPI.Domain.Constants
{
    public static class TipoIvaConstants
    {
        public const string ConsumidorFinal = "CONS. FINAL";
        public const string Inscripto = "INSCRIPTO";
        public const string Monotributo = "MONOTRIBUTO";
        public const string Exento = "EXENTO";
        public const string NoResponsable = "NO RESPONSABLE";
        public const string ResponsableNoInscripto = "RESP. NO INSCRIPTO";
        
        public static readonly string[] ValidTipos = new[]
        {
            ConsumidorFinal,
            Inscripto,
            Monotributo,
            Exento,
            NoResponsable,
            ResponsableNoInscripto
        };
        
        public static bool IsValid(string? tipoIva)
        {
            if (string.IsNullOrWhiteSpace(tipoIva))
                return true; // Campo opcional
                
            return Array.Exists(ValidTipos, t => t.Equals(tipoIva, StringComparison.OrdinalIgnoreCase));
        }
    }
}