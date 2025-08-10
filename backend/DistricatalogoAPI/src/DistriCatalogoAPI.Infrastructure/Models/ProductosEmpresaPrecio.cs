using System;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class ProductosEmpresaPrecio
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }

    public int ProductoId { get; set; }

    public string TipoProducto { get; set; } = null!; // 'base' o 'empresa'

    public int ListaPrecioId { get; set; }

    public decimal? PrecioOverride { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual ListasPrecio ListaPrecio { get; set; } = null!;
}