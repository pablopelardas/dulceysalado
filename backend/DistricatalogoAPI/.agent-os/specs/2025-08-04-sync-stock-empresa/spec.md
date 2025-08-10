# Spec Requirements Document

> Spec: Sincronización de Stock por Empresa
> Created: 2025-08-04
> Status: Planning

## Overview

Actualizar el sistema de sincronización para soportar stock diferenciado por empresa, permitiendo que cada empresa mantenga inventarios independientes según los datos enviados por el procesador externo. Esto reemplaza el modelo actual donde todas las empresas reciben el mismo stock global.

## User Stories

### Administrador del Sistema - Sincronización Diferenciada

Como administrador del sistema, quiero que la sincronización procese el campo `stocksPorEmpresa` enviado por el procesador, para que cada empresa tenga stock específico según su inventario real.

El procesador enviará productos con un array `stocksPorEmpresa` donde cada elemento contiene `empresaId` y `stock`. El sistema debe actualizar o crear registros en `productos_base_stock` para cada empresa especificada, manteniendo el stock existente para empresas no incluidas en la sincronización.

### Empresa Cliente - Stock Específico

Como empresa cliente, quiero ver únicamente el stock disponible para mi empresa en los catálogos públicos, para que mis clientes tengan información precisa de disponibilidad de productos.

La consulta de catálogo debe filtrar productos usando el stock específico de la empresa resolvida por subdominio, mostrando solo productos con stock > 0 cuando se aplique el filtro correspondiente.

## Spec Scope

1. **Procesamiento de stocksPorEmpresa** - Modificar ProcessBulkProductsCommandHandler para procesar el nuevo campo stocksPorEmpresa del DTO de sincronización
2. **Actualización de Stock por Empresa** - Implementar lógica para actualizar/crear registros en productos_base_stock para cada empresa especificada
3. **Mantenimiento de Stock Existente** - Preservar stock de empresas no incluidas en la sincronización actual
4. **Invalidación de Caché** - Limpiar caché de stock después de procesar actualizaciones por empresa
5. **Logging de Sincronización** - Registrar estadísticas de actualización por empresa para monitoreo

## Out of Scope

- Modificación del formato base del DTO de sincronización (se mantiene compatibilidad)
- Cambios en endpoints públicos de catálogo (ya funcionan correctamente)
- Alteración del sistema de caché existente (solo invalidación)
- Modificación de endpoints administrativos de stock

## Expected Deliverable

1. Sincronización procesa correctamente productos con campo `stocksPorEmpresa` y actualiza stock por empresa específica
2. Empresas no incluidas en `stocksPorEmpresa` mantienen su stock previo sin cambios
3. Caché de stock se invalida automáticamente después de cada sincronización