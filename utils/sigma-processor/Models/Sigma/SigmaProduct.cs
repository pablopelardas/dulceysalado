namespace SigmaProcessor.Models.Sigma
{
    public class SigmaProduct
    {
        public string fcodigo { get; set; } = string.Empty;      // Código producto
        public string artnom { get; set; } = string.Empty;       // Nombre
        public string fgrupo { get; set; } = string.Empty;       // GRUPO (categoría)
        public string grunom { get; set; } = string.Empty;       // Nombre grupo
        public string frubro { get; set; } = string.Empty;       // RUBRO (marca/grupo2)
        public string rubnom { get; set; } = string.Empty;       // Nombre rubro
        public string codprov { get; set; } = string.Empty;      // Código proveedor
        public string pronom { get; set; } = string.Empty;       // Nombre proveedor
        public string fdelete { get; set; } = string.Empty;      // Activo (N=activo)
        public decimal fuxb { get; set; }        // Unidades por bulto
        public decimal fuxd { get; set; }        // Unidades por display
        public decimal fcanmin { get; set; }     // Cantidad mínima
        public decimal flispr2 { get; set; }     // Lista precio 2
        public decimal flispr4 { get; set; }     // Lista precio 4
        public string foto { get; set; } = string.Empty;         // Tiene foto
    }
}