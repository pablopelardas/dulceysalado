using System;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class EmpresasOferta
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }

    public int AgrupacionId { get; set; }

    public bool? Visible { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;

    public virtual Agrupaciones Agrupacion { get; set; } = null!;
}