namespace SigmaProcessor.Models;

/// <summary>
/// Modelo para procesar archivos de stock individual con solo código y existencia
/// </summary>
public class SingleStockRecord
{
    public int Codigo { get; set; }
    public decimal Existencia { get; set; }
}

/// <summary>
/// Modelo simplificado para enviar solo actualizaciones de stock
/// </summary>
public class ProductStockOnly
{
    public int Codigo { get; set; }
    public List<EmpresaStock> StocksPorEmpresa { get; set; } = new();
}