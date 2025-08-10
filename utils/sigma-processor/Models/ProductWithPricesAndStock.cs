namespace SigmaProcessor.Models;

/// <summary>
/// Extensión del modelo ProductWithPrices para incluir información de stock por empresa
/// Utilizado para el procesamiento triple de archivos (productos + precios + stocks)
/// </summary>
public class ProductWithPricesAndStock : ProductWithPrices
{
    /// <summary>
    /// Lista de stocks por empresa para este producto
    /// Cada elemento contiene el ID de empresa y su stock correspondiente
    /// </summary>
    public new List<EmpresaStock> StocksPorEmpresa { get; set; } = new();
}