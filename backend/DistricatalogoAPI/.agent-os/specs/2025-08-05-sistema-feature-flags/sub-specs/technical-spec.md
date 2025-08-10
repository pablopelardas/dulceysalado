# Especificación Técnica - Sistema Feature Flags

## Arquitectura de Implementación

### 1. Domain Layer

#### FeatureDefinition Entity
```csharp
namespace DistriCatalogoAPI.Domain.Entities
{
    public class FeatureDefinition
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public FeatureValueType TipoValor { get; set; }
        public string? ValorDefecto { get; set; }
        public string? Categoria { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation
        public virtual ICollection<EmpresaFeature> EmpresaFeatures { get; set; }
    }
    
    public enum FeatureValueType
    {
        Boolean,
        String,
        Number,
        Json
    }
}
```

#### EmpresaFeature Entity
```csharp
namespace DistriCatalogoAPI.Domain.Entities
{
    public class EmpresaFeature
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int FeatureId { get; set; }
        public bool Habilitado { get; set; }
        public string? Valor { get; set; }
        public string? Metadata { get; set; } // JSON
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Navigation
        public virtual Company Empresa { get; set; }
        public virtual FeatureDefinition Feature { get; set; }
    }
}
```

#### Repository Interface
```csharp
namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IFeatureRepository
    {
        // Feature Definitions
        Task<IEnumerable<FeatureDefinition>> GetAllDefinitionsAsync();
        Task<FeatureDefinition?> GetDefinitionByCodigoAsync(string codigo);
        Task<FeatureDefinition> CreateDefinitionAsync(FeatureDefinition definition);
        
        // Empresa Features
        Task<IEnumerable<EmpresaFeature>> GetByEmpresaIdAsync(int empresaId);
        Task<EmpresaFeature?> GetByEmpresaAndCodigoAsync(int empresaId, string codigo);
        Task<EmpresaFeature> ConfigureFeatureAsync(EmpresaFeature feature);
        Task<EmpresaFeature> UpdateFeatureAsync(EmpresaFeature feature);
        Task<bool> DeleteFeatureAsync(int empresaId, string codigo);
        
        // Bulk operations
        Task<Dictionary<string, EmpresaFeature>> GetFeaturesDictionaryAsync(int empresaId);
    }
}
```

### 2. Application Layer

#### Commands
```csharp
// ConfigureFeatureCommand.cs
public class ConfigureFeatureCommand : IRequest<EmpresaFeatureDto>
{
    public int EmpresaId { get; set; }
    public string FeatureCode { get; set; }
    public bool Habilitado { get; set; }
    public string? Valor { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

// Handler
public class ConfigureFeatureCommandHandler : IRequestHandler<ConfigureFeatureCommand, EmpresaFeatureDto>
{
    private readonly IFeatureRepository _repository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ConfigureFeatureCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;
    
    public async Task<EmpresaFeatureDto> Handle(ConfigureFeatureCommand request, CancellationToken cancellationToken)
    {
        // Validar empresa existe
        var empresa = await _companyRepository.GetByIdAsync(request.EmpresaId);
        if (empresa == null)
            throw new NotFoundException($"Empresa {request.EmpresaId} no encontrada");
            
        // Validar feature definition existe
        var definition = await _repository.GetDefinitionByCodigoAsync(request.FeatureCode);
        if (definition == null)
            throw new NotFoundException($"Feature {request.FeatureCode} no encontrada");
            
        // Buscar configuración existente
        var existing = await _repository.GetByEmpresaAndCodigoAsync(request.EmpresaId, request.FeatureCode);
        
        if (existing != null)
        {
            // Actualizar
            existing.Habilitado = request.Habilitado;
            existing.Valor = request.Valor;
            existing.Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = _currentUserService.UserId;
            
            var updated = await _repository.UpdateFeatureAsync(existing);
            return _mapper.Map<EmpresaFeatureDto>(updated);
        }
        else
        {
            // Crear nueva
            var feature = new EmpresaFeature
            {
                EmpresaId = request.EmpresaId,
                FeatureId = definition.Id,
                Habilitado = request.Habilitado,
                Valor = request.Valor,
                Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserId
            };
            
            var created = await _repository.ConfigureFeatureAsync(feature);
            return _mapper.Map<EmpresaFeatureDto>(created);
        }
    }
}
```

#### Queries
```csharp
// GetFeaturesByEmpresaQuery.cs
public class GetFeaturesByEmpresaQuery : IRequest<IEnumerable<FeatureConfigurationDto>>
{
    public int EmpresaId { get; set; }
}

// Handler
public class GetFeaturesByEmpresaQueryHandler : IRequestHandler<GetFeaturesByEmpresaQuery, IEnumerable<FeatureConfigurationDto>>
{
    private readonly IFeatureRepository _repository;
    private readonly IMapper _mapper;
    
    public async Task<IEnumerable<FeatureConfigurationDto>> Handle(GetFeaturesByEmpresaQuery request, CancellationToken cancellationToken)
    {
        // Obtener todas las definiciones
        var definitions = await _repository.GetAllDefinitionsAsync();
        
        // Obtener configuraciones de la empresa
        var empresaFeatures = await _repository.GetFeaturesDictionaryAsync(request.EmpresaId);
        
        // Combinar con valores por defecto
        var result = new List<FeatureConfigurationDto>();
        
        foreach (var definition in definitions.Where(d => d.Activo))
        {
            var config = new FeatureConfigurationDto
            {
                Codigo = definition.Codigo,
                Nombre = definition.Nombre,
                Descripcion = definition.Descripcion,
                TipoValor = definition.TipoValor.ToString(),
                Categoria = definition.Categoria
            };
            
            if (empresaFeatures.TryGetValue(definition.Codigo, out var empresaFeature))
            {
                config.Habilitado = empresaFeature.Habilitado;
                config.Valor = empresaFeature.Valor ?? definition.ValorDefecto;
                config.Metadata = string.IsNullOrEmpty(empresaFeature.Metadata) 
                    ? null 
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(empresaFeature.Metadata);
                config.UpdatedAt = empresaFeature.UpdatedAt;
                config.UpdatedBy = empresaFeature.UpdatedBy;
            }
            else
            {
                config.Habilitado = false;
                config.Valor = definition.ValorDefecto;
            }
            
            result.Add(config);
        }
        
        return result;
    }
}
```

#### Feature Flag Service
```csharp
namespace DistriCatalogoAPI.Application.Services
{
    public interface IFeatureFlagService
    {
        Task<bool> IsFeatureEnabledAsync(int empresaId, string featureCode);
        Task<T?> GetFeatureValueAsync<T>(int empresaId, string featureCode);
        Task InvalidateCacheAsync(int empresaId);
    }
}

// Implementation in Infrastructure
public class FeatureFlagService : IFeatureFlagService
{
    private readonly IMemoryCache _cache;
    private readonly IFeatureRepository _repository;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
    
    public async Task<bool> IsFeatureEnabledAsync(int empresaId, string featureCode)
    {
        var cacheKey = $"feature_{empresaId}_{featureCode}";
        
        if (_cache.TryGetValue<bool>(cacheKey, out var cached))
            return cached;
            
        var feature = await _repository.GetByEmpresaAndCodigoAsync(empresaId, featureCode);
        var enabled = feature?.Habilitado ?? false;
        
        _cache.Set(cacheKey, enabled, _cacheExpiration);
        return enabled;
    }
    
    public async Task<T?> GetFeatureValueAsync<T>(int empresaId, string featureCode)
    {
        var feature = await _repository.GetByEmpresaAndCodigoAsync(empresaId, featureCode);
        if (feature == null || !feature.Habilitado)
            return default;
            
        if (string.IsNullOrEmpty(feature.Valor))
            return default;
            
        try
        {
            if (typeof(T) == typeof(string))
                return (T)(object)feature.Valor;
                
            return JsonSerializer.Deserialize<T>(feature.Valor);
        }
        catch
        {
            return default;
        }
    }
    
    public async Task InvalidateCacheAsync(int empresaId)
    {
        // Implementar patrón de invalidación
        // Por ahora simple, en el futuro usar Redis pub/sub
    }
}
```

### 3. Infrastructure Layer

#### Entity Configuration
```csharp
public class FeatureDefinitionConfiguration : IEntityTypeConfiguration<FeatureDefinition>
{
    public void Configure(EntityTypeBuilder<FeatureDefinition> builder)
    {
        builder.ToTable("feature_definitions");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Codigo)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.HasIndex(e => e.Codigo)
            .IsUnique();
            
        builder.Property(e => e.Nombre)
            .HasMaxLength(255)
            .IsRequired();
            
        builder.Property(e => e.TipoValor)
            .HasConversion<string>()
            .HasDefaultValue(FeatureValueType.Boolean);
            
        builder.Property(e => e.Categoria)
            .HasMaxLength(100);
            
        builder.HasIndex(e => e.Categoria);
    }
}

public class EmpresaFeatureConfiguration : IEntityTypeConfiguration<EmpresaFeature>
{
    public void Configure(EntityTypeBuilder<EmpresaFeature> builder)
    {
        builder.ToTable("empresa_features");
        
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => new { e.EmpresaId, e.FeatureId })
            .IsUnique();
            
        builder.Property(e => e.Metadata)
            .HasColumnType("json");
            
        builder.Property(e => e.CreatedBy)
            .HasMaxLength(255);
            
        builder.Property(e => e.UpdatedBy)
            .HasMaxLength(255);
            
        builder.HasOne(e => e.Empresa)
            .WithMany()
            .HasForeignKey(e => e.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(e => e.Feature)
            .WithMany(f => f.EmpresaFeatures)
            .HasForeignKey(e => e.FeatureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 4. API Layer

#### FeaturesController
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeaturesController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpGet("definitions")]
    public async Task<ActionResult<IEnumerable<FeatureDefinitionDto>>> GetDefinitions()
    {
        var query = new GetFeatureDefinitionsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet("empresa/{empresaId}")]
    public async Task<ActionResult<IEnumerable<FeatureConfigurationDto>>> GetByEmpresa(int empresaId)
    {
        var query = new GetFeaturesByEmpresaQuery { EmpresaId = empresaId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpPost("empresa/{empresaId}")]
    public async Task<ActionResult<EmpresaFeatureDto>> ConfigureFeature(
        int empresaId,
        [FromBody] ConfigureFeatureRequest request)
    {
        var command = new ConfigureFeatureCommand
        {
            EmpresaId = empresaId,
            FeatureCode = request.FeatureCode,
            Habilitado = request.Habilitado,
            Valor = request.Valor,
            Metadata = request.Metadata
        };
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    
    [HttpPut("empresa/{empresaId}/{featureCode}")]
    public async Task<ActionResult<EmpresaFeatureDto>> UpdateFeature(
        int empresaId,
        string featureCode,
        [FromBody] UpdateFeatureRequest request)
    {
        var command = new UpdateFeatureValueCommand
        {
            EmpresaId = empresaId,
            FeatureCode = featureCode,
            Valor = request.Valor,
            Metadata = request.Metadata
        };
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    
    [HttpDelete("empresa/{empresaId}/{featureCode}")]
    public async Task<ActionResult> DisableFeature(int empresaId, string featureCode)
    {
        var command = new DisableFeatureCommand
        {
            EmpresaId = empresaId,
            FeatureCode = featureCode
        };
        
        await _mediator.Send(command);
        return NoContent();
    }
}
```

#### PublicFeaturesController
```csharp
[ApiController]
[Route("api/public/[controller]")]
[AllowAnonymous]
public class PublicFeaturesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFeatureFlagService _featureService;
    private readonly ICompanyResolutionService _companyResolution;
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PublicFeatureDto>>> GetFeatures()
    {
        var empresaId = await _companyResolution.GetCompanyIdAsync(Request);
        if (!empresaId.HasValue)
            return BadRequest("No se pudo identificar la empresa");
            
        var query = new GetPublicFeaturesQuery { EmpresaId = empresaId.Value };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet("{featureCode}")]
    public async Task<ActionResult<PublicFeatureDto>> CheckFeature(string featureCode)
    {
        var empresaId = await _companyResolution.GetCompanyIdAsync(Request);
        if (!empresaId.HasValue)
            return BadRequest("No se pudo identificar la empresa");
            
        var query = new CheckFeatureQuery 
        { 
            EmpresaId = empresaId.Value,
            FeatureCode = featureCode
        };
        
        var result = await _mediator.Send(query);
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }
}
```

## Consideraciones de Performance

### Caché Distribuido
Para ambientes con múltiples instancias:
```csharp
// Usar Redis para caché distribuido
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Configuration.GetConnectionString("Redis");
    options.InstanceName = "DistriCatalogo";
});
```

### Índices Recomendados
```sql
-- Búsquedas frecuentes
CREATE INDEX idx_empresa_features_lookup 
ON empresa_features(empresa_id, feature_id) 
INCLUDE (habilitado, valor);

-- Para queries de categoría
CREATE INDEX idx_feature_category 
ON feature_definitions(categoria, activo);
```

### Telemetría
```csharp
// Agregar métricas de uso
_telemetryClient.TrackEvent("FeatureChecked", new Dictionary<string, string>
{
    ["EmpresaId"] = empresaId.ToString(),
    ["FeatureCode"] = featureCode,
    ["Enabled"] = enabled.ToString()
});
```