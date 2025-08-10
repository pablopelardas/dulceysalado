# Sistema de Stock por Sucursal - Resumen

## Problema
Actualmente el stock se maneja como un campo único (`Existencia`) en la tabla `productos_base`. Esto genera problemas cuando tenemos múltiples sucursales (empresas) que necesitan manejar inventarios independientes.

## Solución Propuesta
Extraer el stock a una tabla separada que relacione:
- Empresa ID (cada empresa es una sucursal)
- Producto Base ID
- Cantidad de stock

## Puntos de Impacto

### 1. CRUD Productos Base
- Permitir que el frontend envíe un `empresa_id` opcional para ver stock específico
- Si no se envía `empresa_id`, usar la empresa principal por defecto
- El filtro de "solo con stock" debe considerar el stock de la empresa especificada
- Mantener compatibilidad con DTOs actuales

### 2. Catálogo Público
- Modificar consultas para filtrar por stock de la empresa del contexto
- Evaluar migrar vista SQL a código C# para mejor mantenibilidad
- Mantener filtro de existencia > 0 pero por empresa

## Beneficios
- **Gestión independiente**: Cada sucursal/empresa maneja su propio inventario
- **Flexibilidad**: Frontend puede consultar stock de cualquier sucursal
- **Compatibilidad**: DTOs sin cambios, solo lógica interna

## Consideraciones
- NO migrar datos existentes (sincronización diaria lo recreará)
- DTOs mantienen campo existencia/stock para compatibilidad
- Stock se resuelve dinámicamente según contexto
- Empresa principal como valor por defecto cuando no se especifica