using System.ComponentModel.DataAnnotations;

namespace SigmaProcessor.Models;

public class GecomRecord
{
    [Required]
    public int Codigo { get; set; }

    [Required]
    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    public int? CodigoRubro { get; set; }

    public decimal Precio { get; set; }

    public decimal Existencia { get; set; }

    public int? Grupo1 { get; set; }

    public int? Grupo2 { get; set; }

    public int? Grupo3 { get; set; }

    public DateTime? FechaAlta { get; set; }

    public DateTime? FechaModi { get; set; }

    [MaxLength(1)]
    public string Imputable { get; set; } = "S";

    [MaxLength(1)]
    public string Disponible { get; set; } = "S";

    [MaxLength(50)]
    public string? CodigoUbicacion { get; set; }
}