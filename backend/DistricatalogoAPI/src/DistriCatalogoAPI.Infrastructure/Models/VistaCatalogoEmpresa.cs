using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class VistaCatalogoEmpresa
{
    public int Id { get; set; }

    public int Codigo { get; set; }

    public string Descripcion { get; set; } = null!;

    public int? CodigoRubro { get; set; }

    public decimal? Precio { get; set; }

    public decimal? Existencia { get; set; }

    public bool? Visible { get; set; }

    public bool? Destacado { get; set; }

    public int? OrdenCategoria { get; set; }

    public string? ImagenUrl { get; set; }

    public string? ImagenAlt { get; set; }

    public string? DescripcionCorta { get; set; }

    public string? DescripcionLarga { get; set; }

    public string? Tags { get; set; }

    public string? CodigoBarras { get; set; }

    public string? Marca { get; set; }

    public string? UnidadMedida { get; set; }

    public string TipoProducto { get; set; } = null!;

    public int EmpresaId { get; set; }

    public string EmpresaNombre { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }
}
