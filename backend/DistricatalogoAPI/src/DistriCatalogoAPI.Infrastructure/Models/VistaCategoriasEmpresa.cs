using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class VistaCategoriasEmpresa
{
    public int Id { get; set; }

    public int CodigoRubro { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Icono { get; set; }

    public bool? Visible { get; set; }

    public int? Orden { get; set; }

    public string? Color { get; set; }

    public string? Descripcion { get; set; }

    public string TipoCategoria { get; set; } = null!;

    public int EmpresaId { get; set; }

    public string EmpresaNombre { get; set; } = null!;
}
