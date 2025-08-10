namespace SigmaProcessor.Models;

/// <summary>
/// Modelo para representar el stock de una empresa específica
/// Utilizado en el DTO enviado a la API DistriCatalogo
/// </summary>
public class EmpresaStock
{
    /// <summary>
    /// ID de la empresa en el sistema DistriCatalogo
    /// </summary>
    public int EmpresaId { get; set; }

    /// <summary>
    /// Cantidad de stock para esta empresa
    /// </summary>
    public decimal Stock { get; set; }
}