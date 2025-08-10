Modifica el procesador CSV Gecom para que sea más simple y eficiente, enviando datos a la API en lugar de procesar directamente la base de datos.

CAMBIOS REQUERIDOS:

1. **Simplificar responsabilidades del procesador:**
   - Mantener solo: limpieza de CSV, validación básica de estructura, envío a API
   - Eliminar: conexión directa a MySQL, lógica de negocio, logging en base de datos

2. **Implementar procesamiento por lotes:**
   - Dividir archivos grandes en lotes de 100-500 productos
   - Configurar tamaño de lote en appsettings.json
   - Procesar lotes secuencialmente con retry logic

3. **Integración con API:**
   - Agregar HttpClient para comunicación con API
   - Configurar endpoints de API en appsettings.json
   - Implementar autenticación JWT para llamadas a API
   - Manejar respuestas de la API (éxito/error por lote)

4. **Nuevas configuraciones en appsettings.json:**
   ```json
   {
     "Api": {
       "BaseUrl": "http://localhost:3000",
       "Username": "admin@empresa.com",
       "Password": "password",
       "TimeoutMinutes": 5
     },
     "Processing": {
       "BatchSize": 200,
       "MaxRetries": 3,
       "RetryDelaySeconds": 5
     }
   }