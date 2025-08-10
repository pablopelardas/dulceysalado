# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-04-simplify-gecom-processor/spec.md

## Technical Requirements

### Configuration Simplification
- Remover de `AppSettings.cs` las propiedades: `UseParallelProcessing`, `MaxParallelChunks`, `UseBulkApi`, `ListasPreciosConfig`
- Actualizar `appsettings.json` para que coincida exactamente con la configuración de producción proporcionada
- Eliminar validaciones de configuración relacionadas con propiedades removidas

### Code Removal - Program.cs
- Eliminar método `ProcessFileWithBulkApiAsync()` y toda su lógica
- Eliminar método `ProcessBatchesInParallelWithPricesAsync()` y `ProcessBatchesSequentiallyWithPricesAsync()`
- Simplificar `ProcessDualFilesAsync()` removiendo la lógica condicional de bulk API
- Eliminar todo el código condicional que verifica `UseParallelProcessing` y `UseBulkApi`
- Remover los métodos `ProcessSingleFileAsync()` ya que no se usa en dual processing
- Simplificar `Main()` removiendo detección de modo de procesamiento - siempre usar dual processing

### Service Layer Cleanup
- Eliminar `ListaPreciosService.cs` completamente
- Remover métodos relacionados con bulk API de `ApiService.cs`: `ProcessProductsBulkAsync()`, `GetListasPreciosAsync()`
- Simplificar `BatchProcessor.cs` removiendo `ProcessBatchesInParallelAsync()` y manteniendo solo procesamiento secuencial
- Actualizar inyección de dependencias en `ConfigureServicesAsync()` removiendo servicios eliminados

### Model Cleanup
- Verificar si `BulkProcessResult` y otros modelos relacionados con bulk API pueden ser eliminados
- Mantener `ProductWithPrices`, `GecomRecord` y `PriceListRecord` ya que son usados en dual processing

### Error Handling Simplification
- Simplificar manejo de errores removiendo casos específicos de procesamiento paralelo y bulk API
- Mantener logging estructurado pero simplificar mensajes que referencian modos eliminados

### Validation Requirements
- El resultado final debe compilar sin errores
- Debe mantener toda la funcionalidad de dual processing existente
- Los archivos de entrada deben procesarse exactamente igual que antes
- El sistema de logging debe continuar funcionando correctamente