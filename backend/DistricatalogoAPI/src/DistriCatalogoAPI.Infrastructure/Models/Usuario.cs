using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public int EmpresaId { get; set; }

    public string Email { get; set; } = null!;

    public string? Username { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string? Rol { get; set; }

    public bool? PuedeGestionarProductosBase { get; set; }

    public bool? PuedeGestionarProductosEmpresa { get; set; }

    public bool? PuedeGestionarCategoriasBase { get; set; }

    public bool? PuedeGestionarCategoriasEmpresa { get; set; }

    public bool? PuedeGestionarUsuarios { get; set; }

    public bool? PuedeVerEstadisticas { get; set; }

    public bool? Activo { get; set; }

    public DateTime? UltimoLogin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
}
