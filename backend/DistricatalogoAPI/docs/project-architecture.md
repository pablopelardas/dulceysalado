# Arquitectura del Proyecto DistriCatalogoAPI

## Estructura de Capas

### 1. DistriCatalogoAPI.Domain
- **Entidades**: Modelos de dominio puros sin dependencias de infraestructura
- **Value Objects**: Objetos de valor inmutables
- **Interfaces**: Contratos de repositorios y servicios
- **Excepciones**: Excepciones específicas del dominio
- **Enums**: Enumeraciones del dominio

### 2. DistriCatalogoAPI.Application
- **DTOs**: Data Transfer Objects para entrada/salida
- **Commands**: Comandos para operaciones de escritura (MediatR)
- **Queries**: Consultas para operaciones de lectura (MediatR)
- **Handlers**: Manejadores de comandos y consultas
- **Validators**: Validadores con FluentValidation
- **Interfaces**: Contratos de servicios de aplicación
- **Mappings**: Perfiles de AutoMapper

### 3. DistriCatalogoAPI.Infrastructure
- **Models**: Modelos de Entity Framework (Database First)
- **Repositories**: Implementaciones de repositorios
- **Data**: DbContext y configuraciones de EF
- **Services**: Implementaciones de servicios externos
- **Migrations**: Migraciones de base de datos

### 4. DistriCatalogoAPI.Api
- **Controllers**: Controladores REST API
- **Middleware**: Middleware personalizado (autenticación, manejo de errores)
- **Filters**: Filtros de acción y excepción
- **Configuration**: Configuración de servicios y DI

## Patrones y Prácticas

### CQRS con MediatR
- **Commands**: Operaciones que modifican estado
- **Queries**: Operaciones de solo lectura
- **Handlers**: Un handler por comando/query
- **Pipeline Behaviors**: Logging, validación, transacciones

### Repository Pattern
- Interfaces definidas en Domain
- Implementaciones en Infrastructure
- Inyección por DI en handlers

### Dependency Injection
- Registro de servicios por capas
- Scoped para DbContext y repositorios
- Transient para handlers de MediatR

## Funcionalidades Core

### 1. Gestión de Usuarios
- CRUD de usuarios
- Autenticación JWT
- Permisos y roles
- Cambio de contraseña

### 2. Gestión de Empresas
- Empresa principal (única)
- Empresas cliente
- Configuración visual
- Permisos por empresa

### 3. Gestión de Productos
- Productos base (empresa principal)
- Productos empresa (clientes)
- Categorías base y por empresa
- Gestión de imágenes

## Tecnologías

- **.NET 8**: Framework principal
- **Entity Framework Core**: ORM (Database First)
- **MediatR**: Implementación de CQRS
- **AutoMapper**: Mapeo de objetos
- **FluentValidation**: Validación de comandos
- **JWT Bearer**: Autenticación
- **Swagger/OpenAPI**: Documentación de API

## Convenciones de Nomenclatura

### Commands
- `Create[Entity]Command`
- `Update[Entity]Command`
- `Delete[Entity]Command`

### Queries
- `Get[Entity]ByIdQuery`
- `Get[Entity]ListQuery`
- `Search[Entity]Query`

### Handlers
- `Create[Entity]CommandHandler`
- `Get[Entity]ByIdQueryHandler`

### DTOs
- `[Entity]Dto`: DTO de respuesta
- `Create[Entity]Dto`: DTO de creación
- `Update[Entity]Dto`: DTO de actualización

## Flujo de Trabajo

1. **Request** → Controller
2. **Controller** → MediatR (Command/Query)
3. **MediatR** → Handler
4. **Handler** → Repository/Service
5. **Repository** → Database
6. **Response** → AutoMapper → DTO → Controller → Client