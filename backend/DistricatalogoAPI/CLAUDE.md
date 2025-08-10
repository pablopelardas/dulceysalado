# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Standard Workflow
1. First think through the problem, read the codebase for relevant files, and write a plan to tasks/todo.md. If necesary, create a new todo.md with an id and a slug telling about the change.
2. The plan should have a list of todo items that you can check off as you complete them.
3. Before you begin working, check in with me and I will verify the plan.
4. Then, begin working on the todo items, marking them as a complete as you go.
5. Please every step of the way just give me a high level explanation of what changes you made
6. Make every task and code change you do as simple as possible. We want to avoid making any massive or complex changes. Every change should impact as little code as possible. Everything is about simplicity.
7. Finally, add a review section to the todo.md or task file with a summary of the changes you made and any other relevant information.

## Development Commands

### Build and Run
```bash
# Build the solution
dotnet build

# Run the API (development)
dotnet run --project src/DistriCatalogoAPI.Api

# Run with specific URLs
dotnet run --project src/DistriCatalogoAPI.Api --urls="https://localhost:7000;http://localhost:5250"
```

### Development URLs
- Swagger UI: http://localhost:5250/swagger
- API Base: https://localhost:7000
- Health Check: http://localhost:5250/health

### Logging and Monitoring
- **Seq Dashboard**: https://seq.districatalogo.com/
- **Logging Standard**: See `docs/LOGGING_STANDARD.md`

### Environment Variables
```bash
SEQ_URL=https://seq.districatalogo.com/
```

## Architecture Overview

### Clean Architecture Layers
- **DistriCatalogoAPI.Api**: Web API layer - Controllers, Middleware, Program.cs
- **DistriCatalogoAPI.Application**: Business logic - CQRS Handlers, DTOs, Validators
- **DistriCatalogoAPI.Domain**: Core domain - Entities, Interfaces, Value Objects
- **DistriCatalogoAPI.Infrastructure**: External concerns - EF Core, Repositories, Services

### Key Patterns
- **CQRS with MediatR**: All business logic goes through Command/Query handlers
- **Repository Pattern**: Data access abstracted through interfaces
- **JWT Authentication**: Token-based auth with Bearer scheme
- **Multi-tenant**: Company-based data isolation with subdomain resolution

### Request Flow
1. Controller receives request → validates input
2. Creates Command/Query → sends to MediatR
3. Handler processes → uses repositories/services
4. Returns DTO → Controller maps to response

## Proyecto DistriCatalogo - Estado Actual

### Arquitectura
- **Backend**: ASP.NET Core Web API (.NET 9.0)
- **Patrón**: CQRS con MediatR
- **BD**: MySQL con Entity Framework Core 9.0.3
- **Auth**: JWT Bearer

### Módulos Completados
1. **Usuarios y Empresas** ✅ - Gestión de usuarios multiempresa
2. **Sync Productos/Categorías** ✅ - Sincronización desde sistema GECOM
3. **CRUD Productos/Categorías Base** ✅ - Gestión con patrón CQRS
4. **CRUD Productos/Categorías Empresa** ✅ - Gestión específica por empresa
5. **Catálogo Público** ✅ - API pública con resolución automática por subdominio

### Estado Catálogo Público
- **Endpoints**: `/api/catalog/*` funcionales
- **Subdominio**: `empresa1.districatalogo.com` → resolución automática 
- **Filtros**: Categoría, búsqueda, precios, marca funcionando
- **Override**: `?empresaId=123` para testing
- **Middleware**: CompanyResolutionMiddleware operativo
- **Pendiente**: Fase 2 (ProductCount real, categorías base)

### Convenciones Técnicas
- **Controllers**: Solo routing y validación, delegan a MediatR
- **Handlers**: Lógica de negocio en Application layer
- **Repositories**: Acceso a datos en Infrastructure layer
- **Clean Architecture**: API → Application → Domain → Infrastructure
- **Naming**: PascalCase para C#, snake_case para JSON responses
- **DTOs**: Separate request/response models per handler
- **Validation**: FluentValidation in Application layer

## Key Dependencies
- **MediatR 12.2.0**: CQRS implementation
- **FluentValidation 11.3.0**: Input validation
- **AutoMapper 13.0.1**: Object mapping
- **BCrypt.Net-Next 4.0.3**: Password hashing
- **Microsoft.EntityFrameworkCore 9.0.3**: ORM
- **Pomelo.EntityFrameworkCore.MySql 8.0.2**: MySQL provider
