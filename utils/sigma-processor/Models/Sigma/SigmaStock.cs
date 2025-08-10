namespace SigmaProcessor.Models.Sigma
{
    public class SigmaStock
    {
        public string codart { get; set; } = string.Empty;       // Código artículo
        public string fdescri { get; set; } = string.Empty;      // Descripción
        public decimal fstock { get; set; }      // Stock actual
    }
}