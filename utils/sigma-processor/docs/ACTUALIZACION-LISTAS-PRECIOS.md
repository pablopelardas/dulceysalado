# Actualización del Procesador para Múltiples Listas de Precios

## Resumen del Cambio

El sistema API ha sido actualizado para soportar múltiples listas de precios por producto. El procesador debe ser modificado para soportar este nuevo esquema donde **cada sesión de sincronización procesa una lista específica**.

## Cambios Requeridos en el Procesador

### 1. **Actualizar Configuración (appsettings.json)**

Agregar configuración para especificar qué lista de precios procesar:

```json
{
  "Api": {
    "BaseUrl": "http://localhost:5250",
    "Username": "admin@principal.com",
    "Password": "Pablo2846",
    "TimeoutMinutes": 5
  },
  "Processing": {
    "BatchSize": 500,
    "UseParallelProcessing": false,
    "MaxParallelChunks": 4,
    "MaxRetries": 3,
    "RetryDelaySeconds": 5,
    "InputPath": "D:\\distri\\input",
    "ProcessedPath": "D:\\distri\\processed",
    "ErrorPath": "D:\\distri\\errors",
    "TempPath": "D:\\distri\\temp",
    
    // NUEVO: Configuración de listas de precios
    "ListasPreciosConfig": {
      "ProcesarTodasLasListas": true,  // Si es true, procesa todas las listas disponibles
      "ListasPorDefecto": ["LISTA1"],  // Lista(s) específica(s) si ProcesarTodasLasListas = false
      "ArchivoConfigListas": "listas-config.json"  // Archivo externo para configurar listas por archivo
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

### 2. **Crear Archivo de Configuración de Listas (listas-config.json)**

Archivo opcional para configurar qué lista procesar por archivo CSV:

```json
{
  "configuraciones": [
    {
      "patron_archivo": "*-lista1-*",
      "lista_codigo": "LISTA1",
      "descripcion": "Archivos de lista general"
    },
    {
      "patron_archivo": "*-mayorista-*",
      "lista_codigo": "LISTA2", 
      "descripcion": "Archivos de precios mayoristas"
    },
    {
      "patron_archivo": "*-distribuidor-*",
      "lista_codigo": "LISTA3",
      "descripcion": "Archivos de precios para distribuidores"
    }
  ],
  "lista_por_defecto": "LISTA1"
}
```

### 3. **Modificar ApiService.cs**

#### 3.1. Actualizar StartSyncSessionAsync

```csharp
// ANTES:
public async Task<ApiResponse<SyncSession>> StartSyncSessionAsync(string fileName)
{
    var sessionRequest = new
    {
        total_lotes_esperados = 1,
        usuario_proceso = "SISTEMA_GECOM"
    };
    // ...
}

// DESPUÉS:
public async Task<ApiResponse<SyncSession>> StartSyncSessionAsync(string fileName, string? listaCodigo = null)
{
    var sessionRequest = new
    {
        total_lotes_esperados = 1,
        usuario_proceso = "SISTEMA_GECOM",
        lista_codigo = listaCodigo  // NUEVO: Especificar lista de precios
    };
    // ...
}
```

#### 3.2. El ProcessBatchAsync NO necesita cambios

El método `ProcessBatchAsync` ya envía el precio correctamente. El API determinará automáticamente a qué lista pertenece basándose en la sesión activa.

### 4. **Agregar Nuevo Servicio: ListaPreciosService.cs**

```csharp
using Microsoft.Extensions.Logging;
using ProcesadorGecomCsv.Config;
using System.Text.Json;

namespace ProcesadorGecomCsv.Services;

public class ListaPreciosService
{
    private readonly ILogger<ListaPreciosService> _logger;
    private readonly ProcessingConfig _config;
    private readonly ApiService _apiService;

    public ListaPreciosService(
        ILogger<ListaPreciosService> logger,
        ProcessingConfig config,
        ApiService apiService)
    {
        _logger = logger;
        _config = config;
        _apiService = apiService;
    }

    public async Task<List<string>> DeterminarListasParaProcesarAsync(string nombreArchivo)
    {
        var listasConfig = _config.ListasPreciosConfig;
        
        if (listasConfig.ProcesarTodasLasListas)
        {
            return await ObtenerTodasLasListasDisponiblesAsync();
        }

        // Verificar si hay configuración específica por archivo
        var listaEspecifica = DeterminarListaPorArchivo(nombreArchivo);
        if (listaEspecifica != null)
        {
            return new List<string> { listaEspecifica };
        }

        // Usar listas por defecto
        return listasConfig.ListasPorDefecto?.ToList() ?? new List<string> { "LISTA1" };
    }

    private async Task<List<string>> ObtenerTodasLasListasDisponiblesAsync()
    {
        try
        {
            var response = await _apiService.GetListasPreciosAsync();
            if (response.Success && response.Data != null)
            {
                return response.Data.Select(l => l.Codigo).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error obteniendo listas de precios, usando LISTA1 por defecto");
        }

        return new List<string> { "LISTA1" };
    }

    private string? DeterminarListaPorArchivo(string nombreArchivo)
    {
        var configPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            _config.ListasPreciosConfig.ArchivoConfigListas
        );

        if (!File.Exists(configPath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<ListasConfigFile>(json);

            foreach (var regla in config.Configuraciones)
            {
                if (CumplePatron(nombreArchivo, regla.PatronArchivo))
                {
                    _logger.LogInformation("Archivo {Archivo} coincide con patrón {Patron}, usando lista {Lista}",
                        nombreArchivo, regla.PatronArchivo, regla.ListaCodigo);
                    return regla.ListaCodigo;
                }
            }

            return config.ListaPorDefecto;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error leyendo configuración de listas, usando por defecto");
            return null;
        }
    }

    private bool CumplePatron(string nombreArchivo, string patron)
    {
        // Implementación simple de wildcard matching
        if (patron.Contains("*"))
        {
            var regex = patron.Replace("*", ".*");
            return System.Text.RegularExpressions.Regex.IsMatch(
                nombreArchivo, regex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        
        return string.Equals(nombreArchivo, patron, StringComparison.OrdinalIgnoreCase);
    }

    public class ListasConfigFile
    {
        public List<ConfiguracionLista> Configuraciones { get; set; } = new();
        public string ListaPorDefecto { get; set; } = "LISTA1";
    }

    public class ConfiguracionLista
    {
        public string PatronArchivo { get; set; } = "";
        public string ListaCodigo { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }
}
```

### 5. **Agregar Método al ApiService para Obtener Listas**

```csharp
public async Task<ApiResponse<List<ListaPrecio>>> GetListasPreciosAsync()
{
    try
    {
        if (!await EnsureAuthenticatedAsync())
        {
            return new ApiResponse<List<ListaPrecio>>
            {
                Success = false,
                ErrorMessage = "Error de autenticación"
            };
        }

        var response = await _httpClient.GetAsync("/api/listas-precios");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error obteniendo listas de precios: {StatusCode} - {Content}",
                response.StatusCode, responseContent);
            return new ApiResponse<List<ListaPrecio>>
            {
                Success = false,
                ErrorMessage = $"Error HTTP: {response.StatusCode}"
            };
        }

        var apiResponse = JsonSerializer.Deserialize<ApiListasPreciosResponse>(responseContent);
        
        return new ApiResponse<List<ListaPrecio>>
        {
            Success = apiResponse?.success ?? false,
            Data = apiResponse?.listas?.ToList() ?? new List<ListaPrecio>(),
            ErrorMessage = apiResponse?.success == false ? "Error en respuesta API" : null
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error obteniendo listas de precios");
        return new ApiResponse<List<ListaPrecio>>
        {
            Success = false,
            ErrorMessage = ex.Message
        };
    }
}

public class ListaPrecio
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activa { get; set; }
    public bool EsPredeterminada { get; set; }
}

public class ApiListasPreciosResponse
{
    public bool success { get; set; }
    public ListaPrecio[] listas { get; set; } = Array.Empty<ListaPrecio>();
}
```

### 6. **Actualizar la Lógica Principal de Procesamiento**

Modificar el flujo principal para procesar múltiples listas por archivo:

```csharp
// En Program.cs o donde se maneje el procesamiento principal

public async Task ProcesarArchivoConMultiplesListasAsync(string rutaArchivo)
{
    var nombreArchivo = Path.GetFileName(rutaArchivo);
    var listasService = new ListaPreciosService(logger, config, apiService);
    
    // Determinar qué listas procesar para este archivo
    var listasProcesar = await listasService.DeterminarListasParaProcesarAsync(nombreArchivo);
    
    _logger.LogInformation("Procesando archivo {Archivo} para {Cantidad} lista(s): {Listas}",
        nombreArchivo, listasProcesar.Count, string.Join(", ", listasProcesar));

    var resultadosGlobales = new List<BatchProcessingSummary>();

    foreach (var listaCodigo in listasProcesar)
    {
        _logger.LogInformation("=== Procesando Lista: {ListaCodigo} ===", listaCodigo);
        
        try
        {
            // 1. Iniciar sesión para esta lista específica
            var sessionResult = await apiService.StartSyncSessionAsync(nombreArchivo, listaCodigo);
            if (!sessionResult.Success)
            {
                _logger.LogError("Error iniciando sesión para lista {Lista}: {Error}", 
                    listaCodigo, sessionResult.ErrorMessage);
                continue;
            }

            var sessionId = sessionResult.Data.SessionId;
            _logger.LogInformation("Sesión iniciada para lista {Lista}: {SessionId}", listaCodigo, sessionId);

            // 2. Procesar todos los registros para esta lista
            var records = await csvProcessor.ProcessFileAsync(rutaArchivo);
            var summary = await gecomApiService.ProcessRecordsAsync(records, sessionId);

            // 3. Finalizar sesión
            await apiService.FinishSyncSessionAsync(sessionId);
            
            resultadosGlobales.Add(summary);
            
            _logger.LogInformation("Lista {Lista} procesada: {Nuevos} nuevos, {Actualizados} actualizados, {Errores} errores",
                listaCodigo, summary.TotalProductosNuevos, summary.TotalProductosActualizados, summary.TotalErrores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando lista {Lista}", listaCodigo);
        }
    }

    // 4. Reporte consolidado
    var resumenFinal = ConsolidarResultados(resultadosGlobales, listasProcesar);
    MostrarResumenFinal(nombreArchivo, resumenFinal);
}

private ResumenConsolidado ConsolidarResultados(List<BatchProcessingSummary> resultados, List<string> listas)
{
    return new ResumenConsolidado
    {
        ListasProcesadas = listas.Count,
        ListasExitosas = resultados.Count(r => r.IsSuccessful),
        TotalProductosNuevos = resultados.Sum(r => r.TotalProductosNuevos),
        TotalProductosActualizados = resultados.Sum(r => r.TotalProductosActualizados),
        TotalErrores = resultados.Sum(r => r.TotalErrores),
        TiempoTotal = resultados.Sum(r => r.TotalProcessingTime.TotalMilliseconds)
    };
}
```

### 7. **Actualizar Configuración (Config/AppSettings.cs)**

```csharp
public class ProcessingConfig
{
    public int BatchSize { get; set; } = 500;
    public bool UseParallelProcessing { get; set; } = false;
    public int MaxParallelChunks { get; set; } = 4;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 5;
    public string InputPath { get; set; } = "";
    public string ProcessedPath { get; set; } = "";
    public string ErrorPath { get; set; } = "";
    public string TempPath { get; set; } = "";
    
    // NUEVO: Configuración de listas de precios
    public ListasPreciosConfig ListasPreciosConfig { get; set; } = new();
}

public class ListasPreciosConfig
{
    public bool ProcesarTodasLasListas { get; set; } = true;
    public string[] ListasPorDefecto { get; set; } = { "LISTA1" };
    public string ArchivoConfigListas { get; set; } = "listas-config.json";
}
```

## Flujo de Procesamiento Actualizado

### Flujo Anterior (Una sola lista)
1. Leer archivo CSV
2. Iniciar sesión → `POST /api/sync/session/start`
3. Procesar lotes → `POST /api/sync/products/bulk` 
4. Finalizar sesión → `POST /api/sync/session/{id}/finish`

### Flujo Nuevo (Múltiples listas)
1. Leer archivo CSV
2. **Determinar listas a procesar** (configuración + API)
3. **Para cada lista:**
   - Iniciar sesión con lista específica → `POST /api/sync/session/start` + `lista_codigo`
   - Procesar lotes → `POST /api/sync/products/bulk` (mismo payload)
   - Finalizar sesión → `POST /api/sync/session/{id}/finish`
4. **Consolidar resultados** de todas las listas

## Estrategias de Configuración

### Estrategia 1: Procesar Todas las Listas
```json
{
  "ListasPreciosConfig": {
    "ProcesarTodasLasListas": true
  }
}
```
**Comportamiento:** Cada archivo CSV se procesa para todas las listas de precios disponibles en el sistema.

### Estrategia 2: Listas Específicas
```json
{
  "ListasPreciosConfig": {
    "ProcesarTodasLasListas": false,
    "ListasPorDefecto": ["LISTA1", "LISTA2"]
  }
}
```
**Comportamiento:** Cada archivo se procesa solo para LISTA1 y LISTA2.

### Estrategia 3: Por Patrón de Archivo
```json
{
  "ListasPreciosConfig": {
    "ProcesarTodasLasListas": false,
    "ArchivoConfigListas": "listas-config.json"
  }
}
```
**Comportamiento:** Usa el archivo `listas-config.json` para determinar qué lista procesar según el nombre del archivo.

## Logs Mejorados

Los logs ahora incluirán información de la lista procesada:

```
[INFO] Procesando archivo productos-2024-01-15.csv para 3 lista(s): LISTA1, LISTA2, LISTA3
[INFO] === Procesando Lista: LISTA1 ===
[INFO] Sesión iniciada para lista LISTA1: a1b2c3d4-e5f6-7890-abcd-ef1234567890
[INFO] Lista LISTA1 procesada: 150 nuevos, 350 actualizados, 2 errores
[INFO] === Procesando Lista: LISTA2 ===
[INFO] Sesión iniciada para lista LISTA2: b2c3d4e5-f6g7-8901-bcde-f23456789012
[INFO] Lista LISTA2 procesada: 0 nuevos, 500 actualizados, 0 errores
[INFO] === RESUMEN FINAL ===
[INFO] Archivo productos-2024-01-15.csv procesado exitosamente:
[INFO] - 3/3 listas procesadas exitosamente
[INFO] - Total: 150 nuevos, 850 actualizados, 2 errores
[INFO] - Tiempo total: 45.2 segundos
```

## Consideraciones de Implementación

1. **Orden de Implementación:**
   - Primero actualizar `ApiService.StartSyncSessionAsync`
   - Crear `ListaPreciosService`
   - Modificar lógica principal de procesamiento
   - Probar con archivos pequeños

2. **Manejo de Errores:**
   - Si falla una lista, continuar con las siguientes
   - Registrar errores específicos por lista
   - Mantener logs detallados para debugging

3. **Performance:**
   - Las sesiones son secuenciales por lista (no paralelas)
   - El mismo archivo se lee una vez, se procesa para múltiples listas
   - Considerar cache de datos CSV si el archivo es muy grande

4. **Compatibilidad:**
   - El cambio es retrocompatible
   - Si no se especifica `lista_codigo`, usa la lista predeterminada
   - Los archivos existentes siguen funcionando igual

## Testing

1. **Configurar lista predeterminada:**
   ```json
   {"ListasPreciosConfig": {"ProcesarTodasLasListas": false, "ListasPorDefecto": ["LISTA1"]}}
   ```

2. **Probar con múltiples listas:**
   ```json
   {"ListasPreciosConfig": {"ProcesarTodasLasListas": true}}
   ```

3. **Verificar logs:** Confirmar que cada lista se procesa en sesiones separadas

4. **Verificar BD:** Confirmar que los precios se almacenan en las tablas correctas por lista