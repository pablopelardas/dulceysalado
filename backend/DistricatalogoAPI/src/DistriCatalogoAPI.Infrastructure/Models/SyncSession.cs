using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class SyncSession
{
    public Guid Id { get; set; }

    public int EmpresaPrincipalId { get; set; }

    public int? ListaPrecioId { get; set; }

    public string? Estado { get; set; }

    public int? TotalLotesEsperados { get; set; }

    public int? LotesProcesados { get; set; }

    public int? ProductosTotales { get; set; }

    public int? ProductosActualizados { get; set; }

    public int? ProductosNuevos { get; set; }

    public int? ProductosErrores { get; set; }

    public string? ErroresDetalle { get; set; }

    public string? Metricas { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? TiempoTotalMs { get; set; }

    public string? UsuarioProceso { get; set; }

    public string? IpOrigen { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa EmpresaPrincipal { get; set; } = null!;

    public virtual ListasPrecio? ListaPrecio { get; set; }
}
