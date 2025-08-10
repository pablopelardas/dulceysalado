# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-08-04-stock-diferencial-empresas/spec.md

## Endpoints

### POST /api/sync/products/bulk (Modificación Existente)

**Purpose:** Modificar el endpoint existente para aceptar información adicional de stocks por empresa
**Parameters:** Se mantienen los existentes, se extiende el payload
**Response:** Se mantiene el formato actual de respuesta
**Errors:** Se mantienen los códigos de error existentes

### Payload Extendido

**Estructura actual que se debe extender:**
```json
{
  "sessionId": "string",
  "products": [
    {
      "codigo": 92,
      "descripcion": "VINO LLUVIA NEGRA X 750",
      "codigoRubro": 5,
      "precio": 125.5,
      "existencia": 10.0,
      "grupo1": null,
      "grupo2": null, 
      "grupo3": null,
      "fechaAlta": "2024-01-15",
      "fechaModi": "2024-06-20",
      "imputable": "S",
      "disponible": "S",
      "codigoUbicacion": "A1-B2",
      "listasPrecios": [
        {
          "listaId": 1,
          "precio": 445.0,
          "fecha": "2025-08-04T10:00:00"
        }
      ],
      // NUEVA SECCIÓN A AGREGAR:
      "stocksPorEmpresa": [
        {
          "empresaId": 1,
          "stock": 2.0
        },
        {
          "empresaId": 12,
          "stock": 0.0
        },
        {
          "empresaId": 18,
          "stock": 0.0
        },
        {
          "empresaId": 17,
          "stock": 0.0
        },
        {
          "empresaId": 13,
          "stock": 0.0
        },
        {
          "empresaId": 16,
          "stock": 443.0
        }
      ]
    }
  ]
}
```

## Cambios Requeridos en la API Backend

### Modelo del DTO
Agregar al modelo existente de producto:

```typescript
// O en C# según el backend
interface EmpresaStock {
  empresaId: number;
  stock: number;
}

interface ProductoDTO {
  // ... propiedades existentes
  stocksPorEmpresa?: EmpresaStock[]; // Nueva propiedad opcional
}
```

### Lógica de Procesamiento
1. **Validar empresaId**: Verificar que existan en la tabla empresas
2. **Actualizar stocks**: Por cada EmpresaStock en el array, actualizar la tabla de stocks correspondiente
3. **Manejo de stock 0**: Actualizar explícitamente a 0 cuando se reciba stock: 0.0
4. **Logging**: Registrar cuántos stocks por empresa se procesaron exitosamente

### Endpoints Adicionales (Opcional para el Futuro)
```
GET /api/empresas/activas - Obtener lista de empresas para validación
GET /api/stocks/empresa/{empresaId} - Consultar stocks por empresa específica
```

## Retrocompatibilidad

**Importante**: El campo `stocksPorEmpresa` debe ser opcional para mantener la compatibilidad con el procesamiento dual existente que no incluye stocks.

**Comportamiento esperado:**
- Si `stocksPorEmpresa` está presente → Procesar stocks por empresa
- Si `stocksPorEmpresa` está ausente o es null → Ignorar, procesamiento normal
- Si `stocksPorEmpresa` está vacío → No actualizar stocks para ninguna empresa

## Validaciones en la API

1. **Validación de empresaId**: Debe existir en la tabla empresas y estar activa
2. **Validación de stock**: Debe ser número decimal >= 0
3. **Validación de duplicados**: No debe haber empresaId duplicados en el mismo producto
4. **Logging de errores**: Registrar productos que no pudieron actualizar stocks con el motivo específico