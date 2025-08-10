using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class CategoriasEmpresa
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }

    public int CodigoRubro { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Icono { get; set; }

    public bool? Visible { get; set; }

    public int? Orden { get; set; }

    public string? Color { get; set; }

    public string? Descripcion { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
