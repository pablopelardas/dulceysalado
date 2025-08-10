# Módulo 01: Gestión de Usuarios

## Objetivo
Implementar el sistema completo de gestión de usuarios con autenticación JWT, siguiendo la arquitectura en capas y utilizando CQRS con MediatR.

## Tareas

### 1. Análisis del Modelo Existente
- [ ] Revisar el modelo `Usuario.cs` en Infrastructure/Models
- [ ] Identificar campos y relaciones
- [ ] Documentar restricciones y reglas de negocio

### 2. Capa Domain - Entidades y Contratos
- [ ] Crear entidad `User` en Domain/Entities
- [ ] Crear value objects: `Email`, `Password` (hashed)
- [ ] Definir enums: `UserRole`, `UserStatus`
- [ ] Crear interface `IUserRepository` en Domain/Interfaces
- [ ] Definir excepciones: `UserNotFoundException`, `DuplicateEmailException`

### 3. Capa Infrastructure - Repositorio
- [ ] Implementar `UserRepository` en Infrastructure/Repositories
- [ ] Configurar mapeo entre modelo EF y entidad Domain
- [ ] Implementar métodos:
  - `GetByIdAsync`
  - `GetByEmailAsync`
  - `GetAllByCompanyAsync`
  - `CreateAsync`
  - `UpdateAsync`
  - `DeleteAsync`
  - `ExistsByEmailAsync`

### 4. Capa Application - Commands
- [ ] Instalar paquetes: MediatR, FluentValidation, AutoMapper
- [ ] Crear DTOs:
  - `UserDto`
  - `CreateUserDto`
  - `UpdateUserDto`
  - `LoginDto`
  - `AuthResponseDto`
- [ ] Implementar Commands:
  - `CreateUserCommand` + Handler + Validator
  - `UpdateUserCommand` + Handler + Validator
  - `DeleteUserCommand` + Handler
  - `ChangePasswordCommand` + Handler + Validator
  - `LoginCommand` + Handler + Validator

### 5. Capa Application - Queries
- [ ] Implementar Queries:
  - `GetUserByIdQuery` + Handler
  - `GetUsersListQuery` + Handler (con paginación)
  - `GetUsersByCompanyQuery` + Handler

### 6. Capa Application - Servicios
- [ ] Crear interface `IAuthService`
- [ ] Implementar `JwtService` para generación de tokens
- [ ] Configurar AutoMapper profiles

### 7. Capa API - Controllers
- [ ] Crear `AuthController`:
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
  - `POST /api/auth/logout`
- [ ] Crear `UsersController`:
  - `GET /api/users`
  - `GET /api/users/{id}`
  - `POST /api/users`
  - `PUT /api/users/{id}`
  - `DELETE /api/users/{id}`
  - `PUT /api/users/{id}/password`

### 8. Configuración y Middleware
- [ ] Configurar autenticación JWT en Program.cs
- [ ] Crear middleware de manejo de errores
- [ ] Configurar DI para todos los servicios
- [ ] Añadir políticas de autorización

### 9. Testing y Documentación
- [x] Probar todos los endpoints con Swagger
- [x] Verificar validaciones
- [x] Probar flujo completo de autenticación
- [x] Documentar endpoints en Swagger

## Estructura de Archivos Esperada

```
DistriCatalogoAPI.Domain/
├── Entities/
│   └── User.cs
├── ValueObjects/
│   ├── Email.cs
│   └── Password.cs
├── Enums/
│   ├── UserRole.cs
│   └── UserStatus.cs
├── Interfaces/
│   └── IUserRepository.cs
└── Exceptions/
    ├── UserNotFoundException.cs
    └── DuplicateEmailException.cs

DistriCatalogoAPI.Application/
├── DTOs/
│   ├── UserDto.cs
│   ├── CreateUserDto.cs
│   ├── UpdateUserDto.cs
│   ├── LoginDto.cs
│   └── AuthResponseDto.cs
├── Commands/
│   ├── CreateUserCommand.cs
│   ├── UpdateUserCommand.cs
│   ├── DeleteUserCommand.cs
│   ├── ChangePasswordCommand.cs
│   └── LoginCommand.cs
├── Handlers/
│   ├── CreateUserCommandHandler.cs
│   ├── UpdateUserCommandHandler.cs
│   ├── DeleteUserCommandHandler.cs
│   ├── ChangePasswordCommandHandler.cs
│   └── LoginCommandHandler.cs
├── Queries/
│   ├── GetUserByIdQuery.cs
│   ├── GetUsersListQuery.cs
│   └── GetUsersByCompanyQuery.cs
├── Validators/
│   ├── CreateUserValidator.cs
│   ├── UpdateUserValidator.cs
│   └── LoginValidator.cs
├── Interfaces/
│   └── IAuthService.cs
└── Mappings/
    └── UserMappingProfile.cs

DistriCatalogoAPI.Infrastructure/
├── Repositories/
│   └── UserRepository.cs
└── Services/
    └── JwtService.cs

DistriCatalogoAPI.Api/
├── Controllers/
│   ├── AuthController.cs
│   └── UsersController.cs
└── Middleware/
    └── ErrorHandlingMiddleware.cs
```

## Criterios de Aceptación

1. Los usuarios pueden autenticarse y recibir un JWT token
2. Solo usuarios autenticados pueden acceder a los endpoints protegidos
3. Los usuarios de empresa principal pueden gestionar usuarios de cualquier empresa
4. Los usuarios de empresa cliente solo pueden gestionar usuarios de su propia empresa
5. Las validaciones funcionan correctamente (email único, contraseña segura, etc.)
6. El sistema maneja errores gracefully y retorna respuestas HTTP apropiadas

## Próximos Pasos

Una vez completado este módulo, procederemos con:
- Módulo 02: Gestión de Empresas
- Módulo 03: Gestión de Productos Base
- Módulo 04: Gestión de Productos por Empresa