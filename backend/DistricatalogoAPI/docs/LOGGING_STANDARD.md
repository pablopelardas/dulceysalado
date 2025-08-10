# Estándar de Logging - DistriCatalogo API

## Configuración

### Serilog Configuration
- **Console Sink**: Para desarrollo local con formato legible
- **Seq Sink**: Para producción con structured logging
  - URL: `https://seq.districatalogo.com/`
  - API Key: `5yb5XcBnY8CtMgwQd4VQ`

### Configuración en appsettings.json
La configuración de Serilog se encuentra en:
- `appsettings.json`: Configuración base
- `appsettings.Development.json`: Debug level, SQL commands visible
- `appsettings.Production.json`: Information level, solo errores importantes

### Variables de Entorno (Opcional)
```bash
SEQ_URL=https://seq.districatalogo.com/
SEQ_API_KEY=5yb5XcBnY8CtMgwQd4VQ
```

### Configuración por Ambiente

#### Development
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
}
```

#### Production
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    }
  }
}
```

## Niveles de Log

### Debug
- Información detallada para debugging
- Request/Response bodies (solo en desarrollo)
- Valores internos de variables
- SQL queries generadas

### Information
- Flujo normal de la aplicación
- Inicio/fin de operaciones importantes
- Resultados de operaciones exitosas
- Métricas de rendimiento

### Warning
- Condiciones inesperadas pero recuperables
- Requests lentos (>5 segundos)
- Fallbacks a valores por defecto
- Intentos de acceso no autorizado

### Error
- Errores que pueden ser manejados
- Fallos de validación
- Errores de base de datos
- Errores de integración con servicios externos

### Fatal
- Errores que causan terminación de la aplicación
- Problemas de configuración críticos
- Fallos de conexión a base de datos

## Estructura de Logs

### Propiedades Estándar
Todos los logs deben incluir estas propiedades cuando estén disponibles:

```csharp
- CorrelationId: string      // ID único por request
- RequestId: string          // TraceIdentifier de ASP.NET Core
- UserId: int?               // ID del usuario autenticado
- CompanyId: int?            // ID de la empresa
- UserName: string?          // Nombre del usuario
- SourceContext: string      // Nombre de la clase que genera el log
```

### Contexto de Dominio
Para operaciones específicas del dominio:

```csharp
- EntityType: string         // Tipo de entidad (Product, Company, etc.)
- EntityId: int?             // ID de la entidad
- Operation: string          // Operación realizada (Create, Update, Delete, etc.)
- ListaPrecioId: int?        // ID de lista de precios (para operaciones de catálogo)
```

## Patrones de Logging

### 1. Operaciones CQRS

#### Commands
```csharp
public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
{
    using var activity = _logger.BeginScope(new Dictionary<string, object>
    {
        ["Operation"] = "CreateProduct",
        ["EntityType"] = "Product",
        ["CompanyId"] = request.CompanyId
    });

    _logger.LogInformation("Creating product: {ProductCode} for company {CompanyId}", 
        request.Codigo, request.CompanyId);

    try
    {
        var result = await _repository.CreateAsync(product);
        
        _logger.LogInformation("Product created successfully: {ProductId} with code {ProductCode}", 
            result.Id, result.Codigo);
            
        return result;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create product {ProductCode} for company {CompanyId}", 
            request.Codigo, request.CompanyId);
        throw;
    }
}
```

#### Queries
```csharp
public async Task<Result> Handle(GetProductsQuery request, CancellationToken cancellationToken)
{
    _logger.LogInformation("Fetching products for company {CompanyId} with filters: {Filters}", 
        request.CompanyId, new { request.Search, request.CategoryId, request.Page });

    var (products, totalCount) = await _repository.GetProductsAsync(request);
    
    _logger.LogInformation("Retrieved {ProductCount} products (total: {TotalCount}) for company {CompanyId}", 
        products.Count, totalCount, request.CompanyId);

    return new GetProductsQueryResult { Products = products, TotalCount = totalCount };
}
```

### 2. Repositorios

#### Operaciones de Base de Datos
```csharp
public async Task<Product> CreateAsync(Product product)
{
    using var activity = _logger.BeginScope(new Dictionary<string, object>
    {
        ["EntityType"] = "Product",
        ["Operation"] = "Create"
    });

    _logger.LogDebug("Executing database insert for product {ProductCode}", product.Codigo);

    try
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Product {ProductId} created in database", product.Id);
        return product;
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Database error creating product {ProductCode}", product.Codigo);
        throw;
    }
}
```

### 3. Autenticación y Autorización

```csharp
_logger.LogInformation("Login attempt for user {UserName}", request.UserName);

// Successful login
_logger.LogInformation("User {UserId} ({UserName}) logged in successfully from {IpAddress}", 
    user.Id, user.UserName, ipAddress);

// Failed login
_logger.LogWarning("Failed login attempt for user {UserName} from {IpAddress}: {Reason}", 
    request.UserName, ipAddress, "Invalid credentials");

// Permission denied
_logger.LogWarning("Access denied for user {UserId} to resource {Resource}: {Reason}", 
    userId, resourcePath, "Insufficient permissions");
```

### 4. Performance Monitoring

```csharp
using var activity = _logger.BeginScope("PerformanceTrace");
var stopwatch = Stopwatch.StartNew();

try
{
    var result = await ExpensiveOperation();
    stopwatch.Stop();
    
    if (stopwatch.ElapsedMilliseconds > 1000)
    {
        _logger.LogWarning("Slow operation detected: {OperationName} took {ElapsedMs}ms", 
            nameof(ExpensiveOperation), stopwatch.ElapsedMilliseconds);
    }
    else
    {
        _logger.LogDebug("Operation {OperationName} completed in {ElapsedMs}ms", 
            nameof(ExpensiveOperation), stopwatch.ElapsedMilliseconds);
    }
    
    return result;
}
catch (Exception ex)
{
    stopwatch.Stop();
    _logger.LogError(ex, "Operation {OperationName} failed after {ElapsedMs}ms", 
        nameof(ExpensiveOperation), stopwatch.ElapsedMilliseconds);
    throw;
}
```

## Mejores Prácticas

### ✅ DO
- Usar structured logging con propiedades tipadas
- Incluir contexto relevante (IDs, nombres, valores)
- Logear tanto éxitos como fallos
- Usar scopes para agrupar operaciones relacionadas
- Incluir timing para operaciones importantes
- Maskear información sensible

### ❌ DON'T
- No logear passwords, tokens, o datos sensibles
- No usar string interpolation con `_logger.LogXxx($"...")`
- No logear en loops sin throttling
- No incluir stack traces en logs informativos
- No usar niveles incorrectos (Error para validaciones)

### Ejemplos de Información Sensible a Maskear
```csharp
// ❌ Malo
_logger.LogInformation("User logged in with password {Password}", password);

// ✅ Bueno
_logger.LogInformation("User {UserId} logged in successfully", userId);

// ❌ Malo
_logger.LogDebug("JWT Token: {Token}", jwtToken);

// ✅ Bueno
_logger.LogDebug("JWT Token generated for user {UserId}, expires at {ExpiresAt}", 
    userId, expiresAt);
```

## Queries Útiles en Seq

### Requests por endpoint
```
RequestPath is not null
| summarize count() by RequestPath
| order by count desc
```

### Errores por usuario
```
@Level = 'Error' and UserId is not null
| summarize count() by UserId, UserName
| order by count desc
```

### Performance por operación
```
has(ResponseTime)
| where ResponseTime > 1000
| summarize avg(ResponseTime), max(ResponseTime), count() by RequestPath
| order by avg desc
```

### Logs por empresa
```
CompanyId is not null
| summarize count() by CompanyId, @Level
| order by CompanyId, @Level
```