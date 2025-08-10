using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class ProductosBase
{
    public int Id { get; set; }

    public string Codigo { get; set; } = null!;

    public string Descripcion { get; set; } = null!;

    public int? CodigoRubro { get; set; }

    // public decimal? Precio { get; set; } // ELIMINADO: Ahora los precios están en productos_base_precios


    public int? Grupo1 { get; set; }

    public int? Grupo2 { get; set; }

    public int? Grupo3 { get; set; }

    public DateTime? FechaAlta { get; set; }

    public DateTime? FechaModi { get; set; }

    public string? Imputable { get; set; }

    public string? Disponible { get; set; }

    public string? CodigoUbicacion { get; set; }

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

    public int AdministradoPorEmpresaId { get; set; }

    public DateTime? ActualizadoGecom { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa AdministradoPorEmpresa { get; set; } = null!;

    public virtual CategoriasBase? CodigoRubroNavigation { get; set; }

    public virtual ICollection<ProductosBasePrecio> ProductosBasePrecios { get; set; } = new List<ProductosBasePrecio>();
}
