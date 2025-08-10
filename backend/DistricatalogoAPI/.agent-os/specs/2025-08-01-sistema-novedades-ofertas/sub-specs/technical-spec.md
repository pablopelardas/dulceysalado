# Technical Specification

This is the technical specification for the spec detailed in @.agent-os/specs/2025-08-01-sistema-novedades-ofertas/spec.md

## Technical Requirements

### Database Schema Extensions
- Modificar tabla `agrupaciones`: Agregar campo `tipo` (TINYINT) para diferenciar Grupo 1, 2, 3
- Crear tabla `empresas_novedades`: Similar a `empresas_agrupaciones_visibles` pero para novedades
- Crear tabla `empresas_ofertas`: Similar a `empresas_agrupaciones_visibles` pero para ofertas
- Mantener integridad referencial con empresas y agrupaciones de tipo 1

### Backend Architecture (Clean Architecture + CQRS)
- **Domain Layer**: Nuevas entidades `EmpresaNovedad` y `EmpresaOferta`
- **Application Layer**: Commands/Queries para CRUD de novedades y ofertas
- **Infrastructure Layer**: Repositorios para nuevas entidades
- **API Layer**: Controladores administrativos y públicos

### API Endpoints Design
- **Administrativos** (con auth): `/api/empresas-novedades/*` y `/api/empresas-ofertas/*`
- **Públicos** (sin auth): `/api/catalog/novedades` y `/api/catalog/ofertas`
- **Resolución automática**: Usar middleware `CompanyResolutionMiddleware` existente

### Performance Considerations
- Endpoints públicos sin paginación para facilitar cacheo frontend
- Consultas optimizadas con joins para obtener productos completos
- Índices en campos de relación (empresa_id, agrupacion_id)

## Implementation Details

### Extensión del Modelo de Agrupaciones
```csharp
// Modificar entidad Agrupacion existente
public class Agrupacion 
{
    // ... propiedades existentes
    public int Tipo { get; set; } // 1=Grupo1, 2=Grupo2, 3=Grupo3
}
```

### Nuevas Entidades Domain
```csharp
public class EmpresaNovedad : BaseEntity
{
    public int EmpresaId { get; set; }
    public int AgrupacionId { get; set; }
    public bool Visible { get; set; }
    
    // Navigation properties
    public Company Empresa { get; set; }
    public Agrupacion Agrupacion { get; set; }
}

public class EmpresaOferta : BaseEntity
{
    public int EmpresaId { get; set; }
    public int AgrupacionId { get; set; }
    public bool Visible { get; set; }
    
    // Navigation properties
    public Company Empresa { get; set; }
    public Agrupacion Agrupacion { get; set; }
}
```

### Repositorios e Interfaces
```csharp
public interface IEmpresaNovedadRepository
{
    // CRUD tradicional
    Task<IEnumerable<EmpresaNovedad>> GetByEmpresaIdAsync(int empresaId);
    Task<EmpresaNovedad> CreateAsync(EmpresaNovedad novedad);
    Task UpdateAsync(EmpresaNovedad novedad);
    Task DeleteAsync(int id);
    
    // Métodos para drag-and-drop (similares a agrupaciones)
    Task<IEnumerable<Agrupacion>> GetAgrupacionesWithNovedadStatusAsync(int empresaId);
    Task SetNovedadesForEmpresaAsync(int empresaId, IEnumerable<int> agrupacionesIds);
}

public interface IEmpresaOfertaRepository
{
    // CRUD tradicional
    Task<IEnumerable<EmpresaOferta>> GetByEmpresaIdAsync(int empresaId);
    Task<EmpresaOferta> CreateAsync(EmpresaOferta oferta);
    Task UpdateAsync(EmpresaOferta oferta);
    Task DeleteAsync(int id);
    
    // Métodos para drag-and-drop
    Task<IEnumerable<Agrupacion>> GetAgrupacionesWithOfertaStatusAsync(int empresaId);
    Task SetOfertasForEmpresaAsync(int empresaId, IEnumerable<int> agrupacionesIds);
}
```

### CQRS Commands/Queries

#### Para Drag-and-Drop (Actualización Masiva)
- `GetAgrupacionesNovedadesForEmpresaQuery` - Retorna todas las agrupaciones con flag de si están como novedades
- `SetNovedadesForEmpresaCommand` - Configura qué agrupaciones son novedades para una empresa
- `GetAgrupacionesOfertasForEmpresaQuery` - Retorna todas las agrupaciones con flag de si están como ofertas  
- `SetOfertasForEmpresaCommand` - Configura qué agrupaciones son ofertas para una empresa

#### Para CRUD Tradicional
- `GetNovedadesByEmpresaQuery` / `GetOfertasByEmpresaQuery`
- `CreateEmpresaNovedadCommand` / `CreateEmpresaOfertaCommand`
- `UpdateEmpresaNovedadCommand` / `UpdateEmpresaOfertaCommand`
- `DeleteEmpresaNovedadCommand` / `DeleteEmpresaOfertaCommand`

#### Para Catálogo Público
- `GetProductosNovedadesQuery` / `GetProductosOfertasQuery` (sin paginación)

### Validaciones con FluentValidation
- Validar que agrupación sea de tipo 1
- Validar que empresa sea válida y activa
- Validar permisos de empresa principal para gestión

### Migration Strategy
1. Agregar campo `tipo` a tabla `agrupaciones` con valor por defecto 3 (manteniendo Grupo 3 actual)
2. Crear tablas `empresas_novedades` y `empresas_ofertas`
3. Actualizar registros existentes de agrupaciones según corresponda