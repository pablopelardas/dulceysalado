using System;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class ProductosBasePrecio
{
    public int Id { get; set; }

    public int ProductoBaseId { get; set; }

    public int ListaPrecioId { get; set; }

    public decimal Precio { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ActualizadoGecom { get; set; }

    public virtual ProductosBase ProductoBase { get; set; } = null!;

    public virtual ListasPrecio ListaPrecio { get; set; } = null!;
}