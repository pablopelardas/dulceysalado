# Módulo 03: Sistema de Sincronización - DistriCatalogoAPI

## 📋 Resumen del Módulo
Implementar un sistema de sincronización que permita al proceso externo (ProcesadorGecomCsv) enviar lotes de productos a la API, manteniendo un tracking completo del proceso y respetando los datos configurados desde la web.

## 🎯 Objetivos
1. Crear endpoints compatibles con el servicio C# existente
2. Mantener sesiones de sincronización con estados y métricas
3. Procesar lotes de productos de forma eficiente
4. Preservar configuraciones web (visible, destacado, imágenes, etc.)
5. Crear categorías automáticamente si no existen
6. Generar logs detallados para auditoría

## 🔑 Reglas de Negocio Críticas

### Actualización Selectiva de Campos
**IMPORTANTE**: Durante la sincronización, solo se actualizan campos que vienen de Gecom:
- ✅ **SE ACTUALIZAN SIEMPRE**:
  - `codigo` (identificador único)
  - `descripcion` (nombre del producto)
  - `codigo_rubro` (categoría)
  - `precio`
  - `existencia` (stock)
  - `grupo1`, `grupo2`, `grupo3`
  - `fecha_alta`, `fecha_modi`
  - `imputable`, `disponible`
  - `codigo_ubicacion`
  - `actualizado_gecom` (timestamp)

- ❌ **NUNCA SE ACTUALIZAN** (configurados desde web):
  - `visible` - Visibilidad en catálogo
  - `destacado` - Producto destacado
  - `orden_categoria` - Orden manual
  - `imagen_url`, `imagen_alt` - Imágenes
  - `descripcion_corta`, `descripcion_larga` - Descripciones web
  - `tags` - Etiquetas SEO
  - `codigo_barras` - Código de barras manual
  - `marca` - Marca del producto
  - `unidad_medida` - Unidad configurada

### Otras Reglas
- Solo empresa principal puede sincronizar
- Productos se asignan a empresa principal como administradora
- Categorías se crean automáticamente con valores por defecto
- No se eliminan productos, solo se actualizan o crean
- Sesiones tienen timeout configurable
- Performance tracking obligatorio

## 📡 Endpoints a Implementar

### 1. POST /api/sync/session/start
Inicia una nueva sesión de sincronización
```json
Request:
{
  "total_lotes_esperados": 1,
  "usuario_proceso": "SISTEMA_GECOM"
}

Response:
{
  "success": true,
  "session_id": "guid",
  "empresa_principal": "Empresa Principal SA",
  "fecha_inicio": "2025-01-01T10:00:00Z",
  "total_lotes_esperados": 1
}
```

### 2. POST /api/sync/products/bulk
Procesa un lote de productos
```json
Request:
{
  "session_id": "guid",
  "lote_numero": 1,
  "productos": [
    {
      "codigo": "12345",
      "descripcion": "Producto A",
      "categoria_id": 100,
      "precio": 150.00,
      "stock": 50,
      "grupo1": 1,
      "grupo2": 2,
      "grupo3": 3,
      "fecha_alta": "2025-01-01",
      "fecha_modi": "2025-01-01",
      "imputable": "S",
      "disponible": "S",
      "codigo_ubicacion": "A1"
    }
  ]
}

Response:
{
  "success": true,
  "session_id": "guid",
  "lote_numero": 1,
  "estadisticas": {
    "productos_procesados": 1,
    "productos_nuevos": 0,
    "productos_actualizados": 1,
    "errores": 0
  },
  "tiempo_procesamiento_ms": 150,
  "categorias_info": {
    "categorias_creadas_automaticamente": 0,
    "categorias_creadas_lista": []
  }
}
```

### 3. POST /api/sync/session/{id}/finish
Finaliza una sesión
```json
Request:
{
  "estado": "completada"
}

Response:
{
  "success": true,
  "session_id": "guid",
  "estado_final": "completada",
  "resumen": {
    "productos_totales": 100,
    "productos_actualizados": 80,
    "productos_nuevos": 20,
    "productos_errores": 0,
    "lotes_procesados": 1,
    "tiempo_total_ms": 1500
  }
}
```

### 4. GET /api/sync/session/{id}/status
Obtiene el estado actual de una sesión

### 5. GET /api/sync/sessions
Lista sesiones con paginación y filtros

### 6. GET /api/sync/stats
Estadísticas de sincronización

### 7. DELETE /api/sync/sessions/cleanup
Limpia sesiones antiguas

## 🏗️ Arquitectura Clean

### Domain Layer
```
/src/DistriCatalogoAPI.Domain/
├── Entities/
│   ├── SyncSession.cs
│   └── SyncLog.cs
├── ValueObjects/
│   ├── SessionState.cs
│   └── SyncMetrics.cs
└── Interfaces/
    ├── ISyncSessionRepository.cs
    ├── ISyncLogRepository.cs
    └── IProductBaseRepository.cs
```

### Application Layer
```
/DistriCatalogoAPI.Application/
├── Commands/Sync/
│   ├── StartSyncSessionCommand.cs
│   ├── ProcessBulkProductsCommand.cs
│   ├── FinishSyncSessionCommand.cs
│   └── CleanupOldSessionsCommand.cs
├── Queries/Sync/
│   ├── GetSyncSessionQuery.cs
│   ├── GetSyncSessionsQuery.cs
│   └── GetSyncStatsQuery.cs
├── Handlers/Sync/
│   ├── StartSyncSessionCommandHandler.cs
│   ├── ProcessBulkProductsCommandHandler.cs
│   ├── FinishSyncSessionCommandHandler.cs
│   ├── GetSyncSessionQueryHandler.cs
│   └── [otros handlers...]
├── DTOs/Sync/
│   ├── SyncSessionDto.cs
│   ├── BulkProductDto.cs
│   ├── SyncStatsDto.cs
│   └── SyncMetricsDto.cs
└── Validators/Sync/
    ├── StartSyncSessionCommandValidator.cs
    ├── ProcessBulkProductsCommandValidator.cs
    └── BulkProductDtoValidator.cs
```

### Infrastructure Layer
```
/DistriCatalogoAPI.Infrastructure/
└── Repositories/
    ├── SyncSessionRepository.cs
    ├── SyncLogRepository.cs
    └── ProductBaseRepository.cs (extender)
```

### API Layer
```
/src/DistriCatalogoAPI.Api/
└── Controllers/
    └── SyncController.cs
```

## 📝 Plan de Implementación

### Fase 1: Domain y Application Core
1. Crear entidades de dominio adaptadas
2. Implementar value objects para estados
3. Definir interfaces de repositorio
4. Crear commands y queries base

### Fase 2: Handlers y Lógica de Negocio
1. StartSyncSessionCommandHandler
2. ProcessBulkProductsCommandHandler (crítico)
3. FinishSyncSessionCommandHandler
4. Query handlers para consultas

### Fase 3: Infrastructure
1. Implementar repositorios con EF Core
2. Optimizar queries para bulk operations
3. Configurar transacciones y performance

### Fase 4: API y Validaciones
1. Crear SyncController con autorización
2. Implementar validadores FluentValidation
3. Configurar middleware de errores
4. Testing con proceso real

## 🔐 Seguridad y Autorización
- Solo usuarios de empresa principal pueden sincronizar
- JWT token requerido en todos los endpoints
- Validación de pertenencia de sesión a empresa
- Rate limiting para proteger recursos

## 📊 Métricas y Logging
- Tiempo de procesamiento por lote
- Productos por segundo
- Errores detallados con contexto
- Performance warnings
- Auditoría completa en SyncLog

## 🚀 Optimizaciones
- Bulk insert/update con EF Core
- Transacciones por lote
- Índices en campos clave
- Cache de categorías en memoria
- Paralelización donde sea posible

## ✅ Criterios de Éxito
1. Compatible 100% con servicio C# existente
2. Preserva configuraciones web sin modificar
3. Performance < 10s para lotes de 1000 productos
4. Creación automática de categorías funcional
5. Logging completo para debugging
6. Manejo robusto de errores con recuperación

## ✅ ESTADO: COMPLETADO

**Fecha de Completación**: 28 de Junio 2025  
**Estado**: ✅ **MÓDULO COMPLETADO Y FUNCIONAL**

### 🎯 Resultados Alcanzados
- **Sistema de sincronización 100% funcional** con el proceso ProcesadorGecomCsv
- **90% mejora de rendimiento**: De ~8 minutos a ~48 segundos para 3864 productos  
- **Soporte para paralelismo**: 4-8 requests HTTP simultáneos
- **Compatibilidad total** con el sistema C# existente
- **Preservación de configuraciones web** implementada correctamente
- **Logging y auditoría completos** con SyncLog detallado

### 📡 Endpoints Implementados y Probados
```
✅ POST /api/sync/session/start           - Iniciar sesión
✅ POST /api/sync/products/bulk           - Procesar productos  
✅ POST /api/sync/session/{id}/finish     - Finalizar sesión
✅ GET  /api/sync/session/{id}/status     - Estado de sesión
✅ GET  /api/sync/sessions                - Listar sesiones
✅ GET  /api/sync/stats                   - Estadísticas
✅ GET  /api/sync/logs                    - Logs de debug
✅ DELETE /api/sync/sessions/cleanup      - Limpieza
```

### 🔧 Problemas Resueltos
1. **Concurrencia DbContext** → Paralelismo a nivel HTTP
2. **Mapeo de entidades** → Backing fields implementation  
3. **Productos siempre "nuevos"** → Mapeo ProductBase corregido
4. **Campos requeridos** → DTOs con opcionales
5. **Claves duplicadas** → Validación robusta
6. **Double chunking** → Procesamiento directo

### 📊 Métricas Finales
- **Tiempo**: 15.4s para 3864 productos
- **Performance**: 250+ productos/segundo
- **Tasa éxito**: 100%
- **Paralelismo**: 4 requests simultáneos
- **Compatibilidad**: 100% con sistema existente

**Ver detalles completos en**: `tasks/completed/module-03-sync-completion.md`

---

## 🔄 Siguiente Paso
**Módulo completado.** Listo para el siguiente módulo del sistema.