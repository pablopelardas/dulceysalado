using System;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class VistaProductosPreciosEmpresa
{
    public int ProductoId { get; set; }

    public string TipoProducto { get; set; } = null!;

    public int Codigo { get; set; }

    public string Descripcion { get; set; } = null!;

    public int? CodigoRubro { get; set; }

    public bool? Visible { get; set; }

    public bool? Destacado { get; set; }

    public string? ImagenUrl { get; set; }

    public string? Marca { get; set; }

    public string? UnidadMedida { get; set; }

    public int EmpresaId { get; set; }

    public string EmpresaNombre { get; set; } = null!;

    public int ListaPrecioId { get; set; }

    public string ListaCodigo { get; set; } = null!;

    public string ListaNombre { get; set; } = null!;

    public decimal PrecioFinal { get; set; }

    public bool PrecioPersonalizado { get; set; }

    public DateTime? ActualizadoGecom { get; set; }

    public DateTime? UpdatedAt { get; set; }
}