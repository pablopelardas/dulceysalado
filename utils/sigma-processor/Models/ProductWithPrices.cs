using System.ComponentModel.DataAnnotations;

namespace SigmaProcessor.Models;

public class ProductWithPrices
{
    // Datos del producto (de productos.csv)
    [Required]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    public int? CodigoRubro { get; set; }
    
    public string? CategoriaNombre { get; set; }

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

    // Datos de precios (de precios.csv)
    public List<PriceList> ListasPrecios { get; set; } = new();
    
    public decimal? Porcentaje { get; set; }

    // Datos de existencias por empresa (de existencias.csv)
    public List<EmpresaStock> StocksPorEmpresa { get; set; } = new();

    // Método para obtener precio de una lista específica
    public decimal? GetPrecioLista(int listaId)
    {
        return ListasPrecios.FirstOrDefault(l => l.ListaId == listaId)?.Precio;
    }

    // Método para verificar si tiene precios
    public bool TienePrecios => ListasPrecios.Any();

    // Precio por defecto (Lista 1 si existe, sino el primero disponible)
    public decimal PrecioDefault => GetPrecioLista(1) ?? ListasPrecios.FirstOrDefault()?.Precio ?? 0;
}