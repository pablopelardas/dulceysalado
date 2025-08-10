namespace DistriCatalogoAPI.Domain.Entities
{
    /// <summary>
    /// Producto del catálogo con toda la información necesaria para la API pública
    /// </summary>
    public class CatalogProduct
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? DescripcionCorta { get; set; }
        public decimal? Precio { get; set; }
        public decimal? Existencia { get; set; }
        public bool Destacado { get; set; }
        public bool Visible { get; set; }
        public string? ImagenUrl { get; set; }
        public IReadOnlyList<string> Tags { get; set; } = new List<string>();
        public string? Marca { get; set; }
        public string? UnidadMedida { get; set; }
        public int? CodigoRubro { get; set; }
        public string TipoProducto { get; set; } = string.Empty;
        public string? CodigoBarras { get; set; }
        public string? ImagenAlt { get; set; }
        public int EmpresaId { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        
        // Price list information
        public int? ListaPrecioId { get; set; }
        public string? ListaPrecioNombre { get; set; }
        public string? ListaPrecioCodigo { get; set; }
    }
}