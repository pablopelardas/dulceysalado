namespace SigmaProcessor.Models;

/// <summary>
/// Modelo para mapear registros del archivo CSV de stocks diferencial por empresa
/// </summary>
public class StockRecord
{
    /// <summary>
    /// Código del artículo/producto
    /// </summary>
    public int Codigo { get; set; }

    /// <summary>
    /// Descripción del producto
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Unidad (siempre UNI)
    /// </summary>
    public string Uni { get; set; } = string.Empty;

    /// <summary>
    /// Stock para empresa FIAM (ID: 1)
    /// </summary>
    public decimal? FIAM { get; set; }

    /// <summary>
    /// Stock para empresas GOLOCINO (ID: 12, 18)
    /// </summary>
    public decimal? GOLOCINO { get; set; }

    /// <summary>
    /// Stock para empresas CARUPA (ID: 17, 13)
    /// </summary>
    public decimal? CARUPA { get; set; }

    /// <summary>
    /// Stock para empresa BENAVIDES (ID: 16)
    /// </summary>
    public decimal? BENAVIDES { get; set; }

    /// <summary>
    /// Stock para empresa SAVIO (reservado para futuro uso)
    /// </summary>
    public decimal? SAVIO { get; set; }

    /// <summary>
    /// Total de stock (suma de todas las empresas)
    /// </summary>
    public decimal? TOTAL { get; set; }
}