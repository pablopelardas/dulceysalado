using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class CategoriasBase
{
    public int Id { get; set; }

    public int CodigoRubro { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Icono { get; set; }

    public bool? Visible { get; set; }

    public int? Orden { get; set; }

    public string? Color { get; set; }

    public string? Descripcion { get; set; }

    public int CreatedByEmpresaId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa CreatedByEmpresa { get; set; } = null!;

    public virtual ICollection<ProductosBase> ProductosBases { get; set; } = new List<ProductosBase>();
}
