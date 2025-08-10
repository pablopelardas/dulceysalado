# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-04-stock-diferencial-empresas/spec.md

## Technical Requirements

### Archivo de Stock CSV
- **Nombre fijo**: `stocks.csv` (hardcodeado, no configurable)
- **Delimitador**: Punto y coma (;)
- **Formato**: `Artículo;Descripción;Uni;FIAM;GOLOCINO;CARUPA;BENAVIDES;SAVIO;TOTAL`
- **Encoding**: UTF-8 con limpieza automática igual que productos y precios
- **Manejo de decimales**: Reutilizar `CsvCleaner.CleanCsvContent()` para convertir comas a puntos

### Mapeo de Empresas (Hardcodeado)
```csharp
private static readonly Dictionary<string, List<int>> EmpresaMapping = new()
{
    { "FIAM", new List<int> { 1 } },
    { "GOLOCINO", new List<int> { 12, 18 } },
    { "CARUPA", new List<int> { 17, 13 } }, 
    { "BENAVIDES", new List<int> { 16 } },
    { "SAVIO", new List<int>() } // Vacío para futuro uso
};
```

### Nuevos Modelos de Datos

**StockRecord.cs** - Modelo para CSV de stocks:
```csharp
public class StockRecord
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string Uni { get; set; } = string.Empty;
    public decimal? FIAM { get; set; }
    public decimal? GOLOCINO { get; set; }
    public decimal? CARUPA { get; set; }
    public decimal? BENAVIDES { get; set; }
    public decimal? SAVIO { get; set; }
    public decimal? TOTAL { get; set; }
}
```

**EmpresaStock.cs** - Para el DTO:
```csharp
public class EmpresaStock
{
    public int EmpresaId { get; set; }
    public decimal Stock { get; set; }
}
```

**ProductWithPricesAndStock.cs** - Extensión del modelo existente:
```csharp
public class ProductWithPricesAndStock : ProductWithPrices
{
    public List<EmpresaStock> StocksPorEmpresa { get; set; } = new();
}
```

### Modificaciones a Servicios Existentes

**DualFileProcessor → TripleFileProcessor**:
- Renombrar clase a `TripleFileProcessor`
- Agregar procesamiento de `stocks.csv`
- Método `ProcessTripleFilesAsync()` que combine productos, precios y stocks
- Reutilizar `CsvCleaner` para procesar stocks.csv

**Ejemplo de DTO Final** (para el backend):
```json
{
  "codigo": 92,
  "descripcion": "VINO LLUVIA NEGRA X 750",
  "codigoRubro": 5,  
  "precio": 125.50,
  "existencia": 10.00,
  "listasPrecios": [
    {
      "listaId": 1,
      "precio": 445.00,
      "fecha": "2025-08-04T10:00:00"
    },
    {
      "listaId": 5, 
      "precio": 450.00,
      "fecha": "2025-08-04T10:00:00"
    }
  ],
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
      "empresaId": 18, 
      "stock": 0.00
    },
    {
      "empresaId": 17,
      "stock": 0.00
    },
    {
      "empresaId": 13,
      "stock": 0.00
    },
    {
      "empresaId": 16,
      "stock": 443.00
    }
  ]
}
```

### Validaciones y Reglas
- **Stock vacío/nulo**: Convertir a 0.00 decimal
- **Códigos no encontrados**: Log warning pero continuar procesamiento
- **Archivos faltantes**: Stocks.csv es obligatorio en el procesamiento triple
- **Formato de decimales**: Aplicar mismo `CleanDecimalValue()` que precios

### Cambios en appsettings.json
```json
{
  "Processing": {
    "UseDualFileProcessing": true, // Mantener nombre para retrocompatibilidad
    "ProductsFileName": "productos.csv",
    "PricesFileName": "precios.csv", 
    "StocksFileName": "stocks.csv" // Nueva configuración
  }
}
```

### Performance y Compatibilidad
- Mantener procesamiento secuencial por lotes (BatchSize: 500)
- Reutilizar toda la infraestructura de retry y logging existente
- Compatible con el sistema de archivos y paths configurables actual
- No impacta la funcionalidad existente de dual processing