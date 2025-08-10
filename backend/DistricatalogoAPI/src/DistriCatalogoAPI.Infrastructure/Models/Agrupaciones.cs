using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class Agrupaciones
{
    public int Id { get; set; }

    public int Codigo { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activa { get; set; }

    public int EmpresaPrincipalId { get; set; }

    public int Tipo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa EmpresaPrincipal { get; set; } = null!;

    public virtual ICollection<EmpresasAgrupacionesVisible> EmpresasAgrupacionesVisibles { get; set; } = new List<EmpresasAgrupacionesVisible>();

    public virtual ICollection<EmpresasNovedad> EmpresasNovedades { get; set; } = new List<EmpresasNovedad>();

    public virtual ICollection<EmpresasOferta> EmpresasOfertas { get; set; } = new List<EmpresasOferta>();
}