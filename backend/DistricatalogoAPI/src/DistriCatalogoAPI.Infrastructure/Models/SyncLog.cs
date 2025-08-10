using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class SyncLog
{
    public int Id { get; set; }

    public int EmpresaPrincipalId { get; set; }

    public string ArchivoNombre { get; set; } = null!;

    public DateTime? FechaProcesamiento { get; set; }

    public int? ProductosActualizados { get; set; }

    public int? ProductosNuevos { get; set; }

    public int? Errores { get; set; }

    public int? TiempoProcesamientoMs { get; set; }

    public string? Estado { get; set; }

    public string? DetallesErrores { get; set; }

    public string? UsuarioProceso { get; set; }

    public virtual Empresa EmpresaPrincipal { get; set; } = null!;
}
