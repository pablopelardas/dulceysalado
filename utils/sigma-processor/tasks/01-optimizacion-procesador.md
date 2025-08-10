Modifica el procesador CSV Gecom para que sea m�s simple y eficiente, enviando datos a la API en lugar de procesar directamente la base de datos.

CAMBIOS REQUERIDOS:

1. **Simplificar responsabilidades del procesador:**
   - Mantener solo: limpieza de CSV, validaci�n b�sica de estructura, env�o a API
   - Eliminar: conexi�n directa a MySQL, l�gica de negocio, logging en base de datos

2. **Implementar procesamiento por lotes:**
   - Dividir archivos grandes en lotes de 100-500 productos
   - Configurar tama�o de lote en appsettings.json
   - Procesar lotes secuencialmente con retry logic

3. **Integraci�n con API:**
   - Agregar HttpClient para comunicaci�n con API
   - Configurar endpoints de API en appsettings.json
   - Implementar autenticaci�n JWT para llamadas a API
   - Manejar respuestas de la API (�xito/error por lote)

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