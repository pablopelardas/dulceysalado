using System;
using System.Collections.Generic;

namespace DistriCatalogoAPI.Infrastructure.Models;

public partial class ConfiguracionSistema
{
    public int Id { get; set; }

    public string Clave { get; set; } = null!;

    public string? Valor { get; set; }

    public string? Tipo { get; set; }

    public string? Descripcion { get; set; }

    public bool? Publico { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
