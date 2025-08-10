using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class ProductoImagene
{
    public int Id { get; set; }

    public string TipoProducto { get; set; } = null!;

    public int ProductoId { get; set; }

    public int? EmpresaId { get; set; }

    public string UrlImagen { get; set; } = null!;

    public string? AltText { get; set; }

    public bool? EsPrincipal { get; set; }

    public int? Orden { get; set; }

    public string? TipoImagen { get; set; }

    public int? SizeBytes { get; set; }

    public int? WidthPx { get; set; }

    public int? HeightPx { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
