# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-08-04-sync-stock-empresa/spec.md

> Created: 2025-08-04
> Version: 1.0.0

## DTO Modifications

### BulkProductDto Extension

**Ubicación:** `DistriCatalogoAPI.Application.DTOs.Sync.BulkProductDto`

**Nueva propiedad obligatoria:**
```csharp
public class BulkProductDto
{
    // ... propiedades existentes ...
    [Required]
    public List<StockPorEmpresaDto> StocksPorEmpresa { get; set; } = new();
}

public class StockPorEmpresaDto
{
    [Required]
    public int EmpresaId { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public decimal Stock { get; set; }
}
```

**Validación:**
```csharp
public class BulkProductDtoValidator : AbstractValidator<BulkProductDto>
{
    public BulkProductDtoValidator()
    {
        // ... validaciones existentes ...
        
        RuleFor(x => x.StocksPorEmpresa)
            .NotNull()
            .NotEmpty()
            .WithMessage("StocksPorEmpresa es obligatorio");
            
        RuleForEach(x => x.StocksPorEmpresa)
            .SetValidator(new StockPorEmpresaDtoValidator());
    }
}

public class StockPorEmpresaDtoValidator : AbstractValidator<StockPorEmpresaDto>
{
    public StockPorEmpresaDtoValidator()
    {
        RuleFor(x => x.EmpresaId)
            .GreaterThan(0)
            .WithMessage("EmpresaId debe ser mayor a 0");
            
        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El stock no puede ser negativo");
    }
}
```

## Endpoints Affected

### POST /api/sync/bulk-products

**Request Body Actualizado:**
```json
{
  "usuarioProceso": "GECOM_SYNC",
  "productos": [
    {
      "codigo": 92,
      "descripcion": "VINO LLUVIA NEGRA X 750",
      "codigoRubro": 5,
      "precio": 125.50,
      "existencia": 10.00,
      "listasPrecios": [...],
      "stocksPorEmpresa": [
        {
          "empresaId": 1,
          "stock": 2.00
        },
        {
          "empresaId": 12,
          "stock": 0.00
        },
        {
          "empresaId": 16,
          "stock": 443.00
        }
      ]
    }
  ]
}
```

**Response:** Sin cambios - mantiene formato de estadísticas existente

**Comportamiento:**
- Campo `stocksPorEmpresa` es obligatorio
- Campo `existencia` se ignora para propósitos de stock
- Procesa stock únicamente según array `stocksPorEmpresa`
- Estadísticas de respuesta incluyen contadores por empresa procesada