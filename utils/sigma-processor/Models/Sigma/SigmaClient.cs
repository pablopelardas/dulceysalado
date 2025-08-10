namespace SigmaProcessor.Models.Sigma
{
    public class SigmaClient
    {
        public string fcodigo { get; set; } = string.Empty;      // Código cliente
        public string fnombre { get; set; } = string.Empty;      // Nombre
        public string flispre { get; set; } = string.Empty;      // Lista de precios
        public string fcuit { get; set; } = string.Empty;        // CUIT
        public string tipiva { get; set; } = string.Empty;       // Tipo IVA
    }
}