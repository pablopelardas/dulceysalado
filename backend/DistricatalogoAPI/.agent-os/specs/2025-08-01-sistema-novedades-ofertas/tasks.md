# Spec Tasks

## Tasks

- [x] 1. Implementar cambios en base de datos
  - [x] 1.1 Crear migration para agregar campo `tipo` a tabla `agrupaciones`
  - [x] 1.2 Crear migration para tabla `empresas_novedades`
  - [x] 1.3 Crear migration para tabla `empresas_ofertas`
  - [x] 1.4 Crear índices de performance para las nuevas tablas
  - [x] 1.5 Scripts SQL creados (pendiente ejecutar en BD cuando esté listo)

- [x] 2. Implementar entidades Domain
  - [x] 2.1 Modificar entidad `Agrupacion` agregando campo `Tipo`
  - [x] 2.2 Crear entidad `EmpresaNovedad` en Domain layer
  - [x] 2.3 Crear entidad `EmpresaOferta` en Domain layer
  - [x] 2.4 Actualizar contexto EF Core con nuevas entidades

- [x] 3. Implementar repositorios Infrastructure
  - [x] 3.1 Crear interface `IEmpresaNovedadRepository` en Domain con métodos CRUD y drag-and-drop
  - [x] 3.2 Crear interface `IEmpresaOfertaRepository` en Domain con métodos CRUD y drag-and-drop
  - [x] 3.3 Implementar `EmpresaNovedadRepository` con `GetAgrupacionesWithNovedadStatusAsync` y `SetNovedadesForEmpresaAsync`
  - [x] 3.4 Implementar `EmpresaOfertaRepository` con `GetAgrupacionesWithOfertaStatusAsync` y `SetOfertasForEmpresaAsync`
  - [x] 3.5 Registrar repositorios en DI container

- [x] 4. Implementar CQRS Commands/Queries para Novedades
  - [x] 4.1 Crear `GetAgrupacionesNovedadesForEmpresaQuery` y handler (para drag-and-drop)
  - [x] 4.2 Crear `SetNovedadesForEmpresaCommand` y handler (actualización masiva)
  - [x] 4.3 Crear `GetNovedadesByEmpresaQuery` y handler (vista global)
  - [x] 4.4 Crear `GetNovedadByIdQuery` y handler
  - [x] 4.5 Crear `CreateEmpresaNovedadCommand` y handler
  - [x] 4.6 Crear `UpdateEmpresaNovedadCommand` y handler
  - [x] 4.7 Crear `DeleteEmpresaNovedadCommand` y handler
  - [x] 4.8 Crear DTOs y validators con FluentValidation

- [x] 5. Implementar CQRS Commands/Queries para Ofertas
  - [x] 5.1 Crear `GetAgrupacionesOfertasForEmpresaQuery` y handler (para drag-and-drop)
  - [x] 5.2 Crear `SetOfertasForEmpresaCommand` y handler (actualización masiva)
  - [x] 5.3 Crear `GetOfertasByEmpresaQuery` y handler (vista global)
  - [x] 5.4 Crear `GetOfertaByIdQuery` y handler
  - [x] 5.5 Crear `CreateEmpresaOfertaCommand` y handler
  - [x] 5.6 Crear `UpdateEmpresaOfertaCommand` y handler
  - [x] 5.7 Crear `DeleteEmpresaOfertaCommand` y handler
  - [x] 5.8 Crear DTOs y validators con FluentValidation

- [x] 6. Implementar Controllers Administrativos
  - [x] 6.1 Crear `EmpresasNovedadesController` con endpoints drag-and-drop: `GET/PUT /api/empresas/{empresaId}/novedades`
  - [x] 6.2 Crear `EmpresasOfertasController` con endpoints drag-and-drop: `GET/PUT /api/empresas/{empresaId}/ofertas`
  - [x] 6.3 Crear `GlobalNovedadesController` con CRUD tradicional: `/api/empresas-novedades/*`
  - [x] 6.4 Crear `GlobalOfertasController` con CRUD tradicional: `/api/empresas-ofertas/*`
  - [x] 6.5 Implementar validaciones de autorización (solo empresa principal)
  - [x] 6.6 Implementar métodos de actualización masiva similares a `SetVisibleAgrupaciones`
  - [x] 6.7 Agregar manejo de errores y logging siguiendo patrón existente
  - [x] 6.8 Documentar endpoints en Swagger

- [x] 7. Implementar endpoints públicos del catálogo
  - [x] 7.1 Crear `GetProductosNovedadesQuery` y handler
  - [x] 7.2 Crear `GetProductosOfertasQuery` y handler
  - [x] 7.3 Extender `CatalogController` con endpoints `/novedades` y `/ofertas`
  - [x] 7.4 Integrar con `CompanyResolutionMiddleware` existente
  - [x] 7.5 Optimizar queries sin paginación para performance

- [x] 8. Testing y validación
  - [x] 8.1 Compilar proyecto y verificar que no hay errores
  - [x] 8.2 Probar endpoints administrativos con Postman/Swagger
  - [x] 8.3 Probar endpoints públicos de catálogo
  - [x] 8.4 Verificar resolución automática de empresa por subdomain
  - [x] 8.5 Validar que productos aparecen en novedades/ofertas Y categorías normales

- [x] 9. Documentación y deployment
  - [x] 9.1 Actualizar documentación de API
  - [x] 9.2 Crear scripts de migración para producción
  - [x] 9.3 Verificar logs de Serilog para nuevos endpoints
  - [x] 9.4 Confirmar que middleware de seguridad funciona correctamente