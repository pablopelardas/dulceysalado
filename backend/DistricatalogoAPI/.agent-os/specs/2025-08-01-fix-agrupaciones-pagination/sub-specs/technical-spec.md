# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-01-fix-agrupaciones-pagination/spec.md

## Technical Requirements

- Localizar el handler CQRS que procesa la consulta de agrupaciones (GetAgrupacionesQuery o similar)
- Modificar el valor predeterminado de PageSize de 20 a 100 en el query handler
- Mantener la lógica de paginación existente para compatibilidad futura
- Verificar que no existan límites hardcoded en otros lugares del código que puedan interferir
- Asegurar que el cambio no afecte negativamente el rendimiento del endpoint

## Implementation Details

### Archivo a modificar
- Handler de consulta de agrupaciones en la capa Application
- Posiblemente: `GetAgrupacionesQueryHandler.cs` o archivo similar

### Cambio específico
```csharp
// Cambiar de:
var pageSize = request.PageSize ?? 20;

// A:
var pageSize = request.PageSize ?? 100;
```

### Validaciones
- Confirmar que el total de agrupaciones (28) sea menor que el nuevo límite (100)
- Verificar que la respuesta mantenga la estructura JSON actual
- Comprobar que el campo pagination.total_pages muestre 1 cuando hay 28 elementos