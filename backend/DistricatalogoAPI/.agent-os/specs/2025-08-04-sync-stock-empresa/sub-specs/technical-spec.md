# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-04-sync-stock-empresa/spec.md

> Created: 2025-08-04
> Version: 1.0.0

## Technical Requirements

### DTO Modification
- Agregar campo obligatorio `stocksPorEmpresa` al `BulkProductDto` existente
- Remover uso del campo `existencia` para stock (mantener para compatibilidad de estructura)
- Validar estructura del array `stocksPorEmpresa` usando FluentValidation

### Handler Logic Updates
- Modificar `ProcessBulkProductsCommandHandler` para procesar únicamente `stocksPorEmpresa`
- Eliminar lógica actual de stock global usando campo `existencia`
- Implementar batch operations para eficiencia en actualizaciones masivas por empresa

### Stock Repository Integration
- Utilizar `IProductoBaseStockRepository.BulkUpdateStockAsync()` para actualizar por empresa
- Crear registros en `productos_base_stock` para combinaciones empresa-producto inexistentes
- Actualizar registros existentes manteniendo metadatos (created_at, updated_at)

### Logging and Monitoring
- Registrar estadísticas detalladas por empresa usando Serilog
- Incluir métricas: productos actualizados por empresa, tiempo de procesamiento, empresas afectadas
- Log de warning si una empresa en `stocksPorEmpresa` no existe

### Error Handling
- Validar que todos los empresaId existan en tabla `empresas`
- Log warning si empresaId inválido pero continuar procesamiento con otros válidos
- Rollback transaccional completo si falla actualización crítica de base de datos