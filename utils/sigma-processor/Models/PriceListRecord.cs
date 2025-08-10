using System.ComponentModel.DataAnnotations;

namespace SigmaProcessor.Models;

public class PriceListRecord
{
    [Required]
    public int Codigo { get; set; }

    [Required]
    [MaxLength(500)]
    public string Nombre { get; set; } = string.Empty;

    public string? CampoVacio { get; set; } // Columna 3 vacía

    // Lista 1
    public DateTime? FechaLista1 { get; set; }
    public decimal? PrecioLista1 { get; set; }

    // Lista 5
    public DateTime? FechaLista5 { get; set; }
    public decimal? PrecioLista5 { get; set; }

    // Porcentaje
    public decimal? Porcentaje { get; set; }

    // Lista 50
    public DateTime? FechaLista50 { get; set; }
    public decimal? PrecioLista50 { get; set; }

    public string? CampoExtra { get; set; } // Última columna
}

public class PriceList
{
    public int ListaId { get; set; }
    public decimal Precio { get; set; }
    public DateTime? Fecha { get; set; }
}

public class ProductPrices
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public List<PriceList> ListasPrecios { get; set; } = new();
    public decimal? Porcentaje { get; set; }
}