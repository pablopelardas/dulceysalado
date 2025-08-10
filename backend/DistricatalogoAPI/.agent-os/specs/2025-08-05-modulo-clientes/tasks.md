# Spec Tasks - Módulo de Clientes con Autenticación

Estas son las tareas a completar para la especificación detallada en @.agent-os/specs/2025-08-05-modulo-clientes/spec.md

> Created: 2025-08-05
> Status: Ready for Implementation

## Tasks

### 1. Modelo de Dominio y Base de Datos
- [ ] 1.1 Crear script SQL para tabla `clientes` con todos los campos
- [ ] 1.2 Crear script SQL para tablas auxiliares (refresh_tokens, login_history)
- [ ] 1.3 Crear entidad `Cliente` en Domain/Entities
- [ ] 1.4 Crear interface `IClienteRepository` en Domain/Interfaces
- [ ] 1.5 Crear interface `IClienteAuthService` en Domain/Interfaces
- [ ] 1.6 Definir enums y constantes necesarias

### 2. Infraestructura y Persistencia
- [ ] 2.1 Crear `ClienteConfiguration` para Entity Framework
- [ ] 2.2 Agregar DbSet de Cliente al `DistricatalogoContext`
- [ ] 2.3 Implementar `ClienteRepository` con operaciones CRUD
- [ ] 2.4 Implementar métodos de búsqueda y filtrado en repository
- [ ] 2.5 Implementar `ClienteAuthService` para autenticación
- [ ] 2.6 Registrar servicios en dependency injection

### 3. DTOs y Mapeos
- [ ] 3.1 Crear DTOs para Cliente (ClienteDto, CreateClienteDto, UpdateClienteDto)
- [ ] 3.2 Crear DTOs para autenticación (LoginDto, TokenResponseDto)
- [ ] 3.3 Crear DTOs para sincronización (CustomerSyncDto, SyncSessionDto)
- [ ] 3.4 Configurar mapeos con AutoMapper en `ClienteMappingProfile`
- [ ] 3.5 Crear validators con FluentValidation

### 4. Capa de Aplicación - Commands
- [ ] 4.1 Crear `CreateClienteCommand` y handler
- [ ] 4.2 Crear `UpdateClienteCommand` y handler
- [ ] 4.3 Crear `DeleteClienteCommand` y handler
- [ ] 4.4 Crear `CreateClienteCredentialsCommand` y handler
- [ ] 4.5 Crear `UpdateClientePasswordCommand` y handler
- [ ] 4.6 Implementar logging y manejo de errores

### 5. Capa de Aplicación - Queries
- [ ] 5.1 Crear `GetClientesQuery` con paginación y filtros
- [ ] 5.2 Crear `GetClienteByIdQuery` y handler
- [ ] 5.3 Crear `GetClienteByUsernameQuery` para login
- [ ] 5.4 Implementar proyecciones optimizadas en queries
- [ ] 5.5 Agregar includes para lista de precios

### 6. Sincronización con Sesiones
- [ ] 6.1 Crear commands para manejo de sesiones (Start, Process, End)
- [ ] 6.2 Implementar `ProcessBulkCustomersCommand` y handler
- [ ] 6.3 Crear servicio de detección de cambios
- [ ] 6.4 Implementar lógica de mapeo desde CustomerSyncDto
- [ ] 6.5 Agregar validación y reporte de errores
- [ ] 6.6 Implementar estadísticas de sincronización

### 7. API Controllers - Gestión
- [ ] 7.1 Crear `ClientesController` con endpoints CRUD
- [ ] 7.2 Implementar endpoints de búsqueda con filtros
- [ ] 7.3 Crear `ClienteCredentialsController` para gestión de accesos
- [ ] 7.4 Agregar autorización y validación en controllers
- [ ] 7.5 Documentar con Swagger/OpenAPI

### 8. API Controllers - Sincronización
- [ ] 8.1 Crear `CustomerSyncController` con patrón de sesiones
- [ ] 8.2 Implementar endpoint start-session
- [ ] 8.3 Implementar endpoint process-bulk
- [ ] 8.4 Implementar endpoint end-session
- [ ] 8.5 Implementar endpoint de consulta de sesión
- [ ] 8.6 Agregar validación y manejo de errores

### 9. Autenticación de Clientes
- [ ] 9.1 Crear `ClienteAuthController` para el frontend
- [ ] 9.2 Implementar endpoint de login con JWT
- [ ] 9.3 Implementar refresh token functionality
- [ ] 9.4 Implementar endpoint de cambio de contraseña
- [ ] 9.5 Integrar con feature flag de autenticación obligatoria
- [ ] 9.6 Configurar políticas de autorización

### 10. Testing y Validación
- [ ] 10.1 Crear tests para el repository
- [ ] 10.2 Tests para handlers de commands y queries
- [ ] 10.3 Tests de integración para sincronización
- [ ] 10.4 Tests para autenticación y autorización
- [ ] 10.5 Validar con datos reales del XML proporcionado

## Review

### Decisiones de Diseño

1. **Autenticación Separada**: Sistema JWT independiente del backoffice para mayor flexibilidad y seguridad.

2. **Username por Empresa**: Permite que diferentes empresas tengan clientes con mismo username sin colisiones.

3. **Sincronización por Sesiones**: Mantiene consistencia con el patrón existente de productos.

4. **Validaciones Flexibles**: CUIT sin formato estricto ni unicidad según requerimientos del negocio.

5. **Soft Delete + Is_Active**: Doble control para mantener historial pero controlar acceso.

---

**Estado:** Listo para implementación
**Prioridad:** Alta - Bloquea funcionalidad de pedidos con autenticación
**Estimación:** 4-5 días de desarrollo