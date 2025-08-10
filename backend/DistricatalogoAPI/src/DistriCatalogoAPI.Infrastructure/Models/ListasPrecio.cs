using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class ListasPrecio
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activa { get; set; } = true;

    public bool EsPredeterminada { get; set; } = false;

    public int? Orden { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ProductosBasePrecio> ProductosBasePrecios { get; set; } = new List<ProductosBasePrecio>();

    public virtual ICollection<ProductosEmpresaPrecio> ProductosEmpresaPrecios { get; set; } = new List<ProductosEmpresaPrecio>();
}