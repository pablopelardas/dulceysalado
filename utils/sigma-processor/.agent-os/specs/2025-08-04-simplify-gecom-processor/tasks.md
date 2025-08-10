# Spec Tasks

## Tasks

- [x] 1. Limpiar configuración y modelos
  - [x] 1.1 ~~Escribir tests para verificar configuración simplificada~~ (No hay proyecto de tests)
  - [x] 1.2 Remover propiedades obsoletas de AppSettings.cs (UseParallelProcessing, MaxParallelChunks, UseBulkApi, ListasPreciosConfig)
  - [x] 1.3 Actualizar appsettings.json con la configuración de producción proporcionada
  - [x] 1.4 Eliminar modelos relacionados con bulk API si no se usan (BulkProcessResult, etc.)
  - [x] 1.5 ~~Verificar que todos los tests pasen~~ (No hay proyecto de tests)

- [x] 2. Simplificar Program.cs
  - [x] 2.1 ~~Escribir tests para el flujo simplificado de procesamiento~~ (No hay proyecto de tests)
  - [x] 2.2 Eliminar método ProcessFileWithBulkApiAsync() y toda su lógica
  - [x] 2.3 Eliminar métodos ProcessBatchesInParallelWithPricesAsync() y ProcessBatchesSequentiallyWithPricesAsync()
  - [x] 2.4 Simplificar ProcessDualFilesAsync() removiendo lógica condicional de bulk API
  - [x] 2.5 Eliminar métodos ProcessSingleFileAsync() y ProcessSingleFileWithMultipleListsAsync()
  - [x] 2.6 Simplificar Main() para que siempre use dual processing sin detección de modo
  - [x] 2.7 Remover toda lógica condicional relacionada con UseParallelProcessing y UseBulkApi
  - [x] 2.8 ~~Verificar que todos los tests pasen~~ (No hay proyecto de tests)

- [x] 3. Limpiar capa de servicios
  - [x] 3.1 ~~Escribir tests para servicios simplificados~~ (No hay proyecto de tests)
  - [x] 3.2 Eliminar ListaPreciosService.cs completamente
  - [x] 3.3 Remover métodos de bulk API de ApiService.cs (ProcessProductsBulkAsync, GetListasPreciosAsync)
  - [x] 3.4 Simplificar BatchProcessor.cs removiendo ProcessBatchesInParallelAsync()
  - [x] 3.5 Actualizar ConfigureServicesAsync() removiendo servicios eliminados
  - [x] 3.6 ~~Verificar que todos los tests pasen~~ (No hay proyecto de tests)

- [x] 4. Validación final y limpieza
  - [x] 4.1 ~~Escribir tests de integración para el flujo completo simplificado~~ (No hay proyecto de tests)
  - [x] 4.2 Compilar proyecto y verificar que no hay errores
  - [x] 4.3 ~~Ejecutar tests de funcionalidad dual processing~~ (No hay proyecto de tests)
  - [x] 4.4 Limpiar imports y using statements no utilizados (Verificado - todos son necesarios)
  - [x] 4.5 Revisar logs y mensajes para eliminar referencias a modos eliminados (Verificado - solo quedan referencias válidas)
  - [x] 4.6 ~~Verificar que todos los tests pasen~~ (No hay proyecto de tests)