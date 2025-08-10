# Technical Stack

> Last Updated: 2025-08-01
> Version: 1.0.0

## Application Framework

- **Framework:** ASP.NET Core Web API
- **Version:** .NET 9.0

## Database

- **Primary Database:** MySQL
- **ORM:** Entity Framework Core 9.0.3
- **Provider:** Pomelo.EntityFrameworkCore.MySql 8.0.2

## JavaScript

- **Framework:** N/A (API Backend only)

## CSS Framework

- **Framework:** N/A (API Backend only)

## Architecture Patterns

- **Primary Pattern:** Clean Architecture
- **CQRS Implementation:** MediatR 12.2.0
- **Repository Pattern:** Custom implementation with interfaces

## Authentication & Security

- **Authentication:** JWT Bearer Tokens
- **Password Hashing:** BCrypt.Net-Next 4.0.3
- **Authorization:** Role-based with company isolation

## Validation & Mapping

- **Input Validation:** FluentValidation 11.3.0
- **Object Mapping:** AutoMapper 13.0.1

## Logging & Monitoring

- **Logging Framework:** Serilog
- **Log Aggregation:** Seq Dashboard (https://seq.districatalogo.com/)
- **Health Checks:** ASP.NET Core Health Checks

## Development Tools

- **API Documentation:** Swagger/OpenAPI
- **Development URLs:**
  - Swagger UI: http://localhost:5250/swagger
  - API Base: https://localhost:7000
  - Health Check: http://localhost:5250/health

## External Integrations

- **File Processing:** GECOM file format synchronization
- **Multi-tenant Resolution:** Custom subdomain middleware
- **Image Management:** File system storage with URL generation

## Key Dependencies

### Core Framework
- Microsoft.AspNetCore.App (9.0.0)
- Microsoft.EntityFrameworkCore (9.0.3)
- Microsoft.EntityFrameworkCore.Tools (9.0.3)

### CQRS & Validation
- MediatR (12.2.0)
- FluentValidation (11.3.0)
- FluentValidation.AspNetCore (11.3.0)

### Authentication & Security
- Microsoft.AspNetCore.Authentication.JwtBearer (9.0.0)
- BCrypt.Net-Next (4.0.3)

### Mapping & Utilities
- AutoMapper (13.0.1)
- AutoMapper.Extensions.Microsoft.DependencyInjection (13.0.1)

### Database
- Pomelo.EntityFrameworkCore.MySql (8.0.2)
- Microsoft.EntityFrameworkCore.Design (9.0.3)

### Logging
- Serilog.AspNetCore (8.0.0+)
- Serilog.Sinks.Seq (8.0.0+)