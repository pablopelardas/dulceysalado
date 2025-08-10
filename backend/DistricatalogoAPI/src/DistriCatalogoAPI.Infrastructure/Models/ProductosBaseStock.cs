using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

/// <summary>
/// Modelo de Infrastructure para stock de productos base por empresa
/// Mapea directamente a la tabla productos_base_stock
/// </summary>
public partial class ProductosBaseStock
{
    /// <summary>
    /// ID único del registro de stock
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID de la empresa propietaria del stock
    /// </summary>
    public int EmpresaId { get; set; }

    /// <summary>
    /// ID del producto base
    /// </summary>
    public int ProductoBaseId { get; set; }

    /// <summary>
    /// Cantidad disponible en stock
    /// </summary>
    public decimal Existencia { get; set; }

    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Empresa propietaria del stock
    /// </summary>
    public virtual Empresa Empresa { get; set; } = null!;

    /// <summary>
    /// Producto base al que pertenece el stock
    /// </summary>
    public virtual ProductosBase ProductoBase { get; set; } = null!;
}