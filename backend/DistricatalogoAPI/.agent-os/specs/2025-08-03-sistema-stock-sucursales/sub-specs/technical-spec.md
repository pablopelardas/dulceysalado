# Especificación Técnica - Sistema de Stock por Sucursal

## Arquitectura del Sistema

### 1. Capa de Caché (IMemoryCache)

```csharp
public interface IStockCacheService
{
    Task<decimal?> GetStockAsync(int empresaId, int productoId);
    Task SetStockAsync(int empresaId, int productoId, decimal stock);
    Task<List<int>> GetProductosConStockAsync(int empresaId);
    Task SetProductosConStockAsync(int empresaId, List<int> productoIds);
    Task InvalidateAllStockCacheAsync();
    Task InvalidateEmpresaCacheAsync(int empresaId);
    Task<bool> IsCacheWarmAsync(int empresaId);
}

public class StockCacheService : IStockCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<StockCacheService> _logger;
    private const string STOCK_KEY_PREFIX = "stock:empresa:{0}:producto:{1}";
    private const string PRODUCTOS_CON_STOCK_KEY = "productos_con_stock:empresa:{0}";
    private const string CACHE_WARM_KEY = "cache_warm:empresa:{0}";
    private readonly TimeSpan _defaultTTL = TimeSpan.FromHours(6);

    // Implementación...
}
```

### 2. Capa de Dominio

```csharp
// Domain/Entities/ProductoBaseStock.cs
public class ProductoBaseStock : BaseEntity
{
    public int EmpresaId { get; set; }
    public int ProductoBaseId { get; set; }
    public decimal Existencia { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual Company Empresa { get; set; } = null!;
    public virtual ProductBase ProductoBase { get; set; } = null!;
}

// Domain/Interfaces/IProductoBaseStockRepository.cs  
public interface IProductoBaseStockRepository
{
    Task<decimal?> GetStockAsync(int empresaId, int productoBaseId);
    Task<Dictionary<int, decimal>> GetStockBatchAsync(int empresaId, List<int> productoBaseIds);
    Task<List<int>> GetProductosConStockAsync(int empresaId);
    Task UpdateStockAsync(int productoBaseId, int empresaId, decimal stock);
    Task UpdateStockForAllEmpresasAsync(int productoBaseId, decimal stock);
    Task BulkUpdateStockAsync(Dictionary<int, decimal> productosStock, int? empresaId = null);
}
```

### 3. Capa de Infrastructure

```csharp
// Infrastructure/Models/ProductosBaseStock.cs
public partial class ProductosBaseStock
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public int ProductoBaseId { get; set; }
    public decimal Existencia { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Empresa Empresa { get; set; } = null!;
    public virtual ProductosBase ProductoBase { get; set; } = null!;
}

// Infrastructure/Repositories/ProductoBaseStockRepository.cs
public class ProductoBaseStockRepository : IProductoBaseStockRepository
{
    private readonly DistricatalogoContext _context;
    private readonly IStockCacheService _cacheService;
    private readonly ILogger<ProductoBaseStockRepository> _logger;

    public async Task<decimal?> GetStockAsync(int empresaId, int productoBaseId)
    {
        // 1. Intentar desde caché
        var cachedStock = await _cacheService.GetStockAsync(empresaId, productoBaseId);
        if (cachedStock.HasValue)
            return cachedStock.Value;

        // 2. Consultar BD y cachear
        var stock = await _context.ProductosBaseStocks
            .Where(pbs => pbs.EmpresaId == empresaId && pbs.ProductoBaseId == productoBaseId)
            .Select(pbs => pbs.Existencia)
            .FirstOrDefaultAsync();

        if (stock.HasValue)
            await _cacheService.SetStockAsync(empresaId, productoBaseId, stock.Value);

        return stock;
    }

    public async Task<List<int>> GetProductosConStockAsync(int empresaId)
    {
        // 1. Intentar desde caché
        var cached = await _cacheService.GetProductosConStockAsync(empresaId);
        if (cached?.Any() == true)
            return cached;

        // 2. Consultar BD y cachear
        var productos = await _context.ProductosBaseStocks
            .Where(pbs => pbs.EmpresaId == empresaId && pbs.Existencia > 0)
            .Select(pbs => pbs.ProductoBaseId)
            .ToListAsync();

        await _cacheService.SetProductosConStockAsync(empresaId, productos);
        return productos;
    }

    // Otros métodos...
}
```

### 4. Capa de Application

```csharp
// Application/Queries/ProductosBase/GetAllProductosBaseQuery.cs
public class GetAllProductosBaseQuery : IRequest<PagedResultDto<ProductoBaseDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Search { get; set; }
    public bool SoloConStock { get; set; } = false;
    public int? EmpresaId { get; set; } // NUEVO
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}

// Application/Handlers/ProductosBase/GetAllProductosBaseQueryHandler.cs
public class GetAllProductosBaseQueryHandler : IRequestHandler<GetAllProductosBaseQuery, PagedResultDto<ProductoBaseDto>>
{
    private readonly IProductBaseRepository _productRepository;
    private readonly IProductoBaseStockRepository _stockRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IMapper _mapper;

    public async Task<PagedResultDto<ProductoBaseDto>> Handle(GetAllProductosBaseQuery request, CancellationToken cancellationToken)
    {
        // 1. Resolver empresa (usar principal si no se especifica)
        var empresaId = request.EmpresaId ?? await GetEmpresaPrincipalIdAsync();

        // 2. Si solo con stock, obtener lista filtrada desde caché
        List<int>? productosConStock = null;
        if (request.SoloConStock)
        {
            productosConStock = await _stockRepository.GetProductosConStockAsync(empresaId);
            if (!productosConStock.Any())
                return new PagedResultDto<ProductoBaseDto>([], 0, request.Page, request.PageSize);
        }

        // 3. Query base de productos
        var query = _productRepository.GetQueryable();
        
        if (productosConStock != null)
            query = query.Where(p => productosConStock.Contains(p.Id));

        // 4. Aplicar filtros y paginación
        var (productos, totalCount) = await _productRepository.GetPagedAsync(
            query, request.Page, request.PageSize, request.Search, request.SortBy, request.SortOrder);

        // 5. Mapear DTOs y resolver stock desde caché/BD
        var productoDtos = new List<ProductoBaseDto>();
        var productoIds = productos.Select(p => p.Id).ToList();
        var stockBatch = await _stockRepository.GetStockBatchAsync(empresaId, productoIds);

        foreach (var producto in productos)
        {
            var dto = _mapper.Map<ProductoBaseDto>(producto);
            dto.Existencia = stockBatch.GetValueOrDefault(producto.Id, 0);
            productoDtos.Add(dto);
        }

        return new PagedResultDto<ProductoBaseDto>(productoDtos, totalCount, request.Page, request.PageSize);
    }
}
```

### 5. Sincronización con Invalidación de Caché

```csharp
// Application/Handlers/Sync/ProcessBulkProductsCommandHandler.cs
public class ProcessBulkProductsCommandHandler : IRequestHandler<ProcessBulkProductsCommand, BulkProcessResultDto>
{
    private readonly IProductBaseRepository _productRepository;
    private readonly IProductoBaseStockRepository _stockRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IStockCacheService _cacheService;

    public async Task<BulkProcessResultDto> Handle(ProcessBulkProductsCommand request, CancellationToken cancellationToken)
    {
        var result = new BulkProcessResultDto();

        try
        {
            // 1. Procesar productos normalmente
            foreach (var productDto in request.Products)
            {
                var producto = await ProcessProductAsync(productDto);
                
                // 2. Actualizar stock para TODAS las empresas
                if (productDto.Existencia.HasValue)
                {
                    await _stockRepository.UpdateStockForAllEmpresasAsync(
                        producto.Id, productDto.Existencia.Value);
                }
                
                result.ProcessedCount++;
            }

            // 3. INVALIDAR TODO EL CACHÉ DE STOCK
            await _cacheService.InvalidateAllStockCacheAsync();
            
            // 4. Opcional: Pre-cargar caché (warm-up)
            if (_configuration.GetValue<bool>("Cache:Stock:WarmUpAfterSync"))
            {
                await WarmUpCacheAsync();
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk product processing");
            throw;
        }

        return result;
    }

    private async Task WarmUpCacheAsync()
    {
        var empresas = await _companyRepository.GetAllActiveAsync();
        foreach (var empresa in empresas)
        {
            // Pre-cargar lista de productos con stock
            await _stockRepository.GetProductosConStockAsync(empresa.Id);
        }
    }
}
```

### 6. Migración del Catálogo (Eliminar Vista SQL)

```csharp
// Infrastructure/Repositories/CatalogRepository.cs - MIGRADO A CÓDIGO
public class CatalogRepository : ICatalogRepository
{
    public async Task<(List<CatalogProduct> products, int totalCount)> GetCatalogProductsAsync(
        int empresaId, string? listaPrecioCodigo = null, bool? destacados = null, 
        int? codigoRubro = null, string? busqueda = null, string? ordenarPor = null, 
        int page = 1, int pageSize = 20)
    {
        // 1. Obtener productos con stock desde caché
        var productosConStock = await _stockRepository.GetProductosConStockAsync(empresaId);
        if (!productosConStock.Any())
            return ([], 0);

        // 2. Query LINQ reemplazando vista SQL
        var query = from pb in _context.ProductosBase
                    join pbs in _context.ProductosBaseStocks on pb.Id equals pbs.ProductoBaseId
                    join pbp in _context.ProductosBasePrecios on pb.Id equals pbp.ProductoBaseId into preciosGroup
                    from pbp in preciosGroup.DefaultIfEmpty()
                    where pb.Visible == true 
                        && pbs.EmpresaId == empresaId 
                        && productosConStock.Contains(pb.Id) // Filtro con caché
                    select new CatalogProduct
                    {
                        Id = pb.Id,
                        Codigo = pb.Codigo,
                        Descripcion = pb.Descripcion,
                        Existencia = pbs.Existencia, // Desde nueva tabla
                        // ... otros campos
                    };

        // 3. Aplicar filtros adicionales
        if (destacados.HasValue)
            query = query.Where(p => p.Destacado == destacados.Value);

        if (codigoRubro.HasValue)
            query = query.Where(p => p.CodigoRubro == codigoRubro.Value);

        // 4. Paginación y ejecución
        var totalCount = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, totalCount);
    }
}
```

### 7. Configuración y Monitoreo

```csharp
// Extensions/ServiceExtensions.cs
public static class ServiceExtensions
{
    public static IServiceCollection AddStockCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StockCacheOptions>(configuration.GetSection("Cache:Stock"));
        
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = configuration.GetValue<int>("Cache:Stock:MaxMemoryMB") * 1024 * 1024;
        });
        
        services.AddScoped<IStockCacheService, StockCacheService>();
        
        return services;
    }
}

// Configuration/StockCacheOptions.cs
public class StockCacheOptions
{
    public int DefaultTTLHours { get; set; } = 6;
    public int MaxMemoryMB { get; set; } = 100;
    public bool WarmUpAfterSync { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
}

// Middleware/CacheMetricsMiddleware.cs - Para monitoreo
public class CacheMetricsMiddleware
{
    // Implementar métricas de hit/miss ratio
    // Logs de performance
    // Alertas de caché frío
}
```

## Flujo de Datos

### Consulta de Stock (Catálogo/CRUD)
```
Request → Controller → Handler → StockRepository
                                      ↓
                               Cache Hit? → Return cached
                                      ↓ No
                               DB Query → Cache result → Return
```

### Sincronización
```
Sync Request → Handler → Update productos_base_stock (all empresas)
                              ↓
                         Invalidate cache → Optional warm-up
```

### Invalidación de Caché
```
Sync Complete → InvalidateAllStockCacheAsync() → Clear all stock keys
Manual Admin → InvalidateEmpresaCacheAsync(id) → Clear empresa keys  
TTL Expired → Automatic cleanup → Re-query on next access
```