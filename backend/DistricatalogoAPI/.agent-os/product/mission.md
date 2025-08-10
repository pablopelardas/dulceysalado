# Product Mission

> Last Updated: 2025-08-01
> Version: 1.0.0

## Pitch

DistriCatalogoAPI es una plataforma de catálogos distribuidos que permite a empresas grandes con múltiples divisiones disponibilizar catálogos públicos de productos con precios para compradores, facilitando la creación de listas de compras y la consulta de información comercial.

## Users

### Empresas Grandes con Divisiones
- **Necesidad**: Disponibilizar catálogos públicos para compradores con información actualizada de productos y precios
- **Beneficio**: Gestión centralizada con personalización por división (colores, datos de contacto, subdominio)
- **Casos de uso**: Configuración de catálogos por división, gestión de productos e imágenes, sincronización con sistemas internos

### Compradores/Clientes
- **Necesidad**: Acceso fácil y rápido a catálogos de productos con precios actualizados
- **Beneficio**: Consulta sin autenticación, creación de listas de compras, información comercial actualizada
- **Casos de uso**: Búsqueda de productos, filtrado por categorías, consulta de precios, armado de listas

### Administradores de Sistema
- **Necesidad**: Gestión eficiente de productos, categorías, precios e imágenes
- **Beneficio**: Interfaz administrativa robusta con sincronización automática
- **Casos de uso**: CRUD de productos/categorías, gestión de listas de precios, configuración de divisiones

## The Problem

Las empresas grandes con múltiples divisiones enfrentan el desafío de disponibilizar catálogos públicos actualizados para sus compradores. Los problemas incluyen:

1. **Fragmentación de información**: Cada división maneja sus propios catálogos de forma independiente
2. **Desactualización constante**: Los precios y productos cambian frecuentemente y no se reflejan en tiempo real
3. **Falta de personalización**: No pueden adaptar la presentación del catálogo a cada división
4. **Complejidad técnica**: Requiere desarrollo específico para cada caso de uso
5. **Sincronización manual**: Actualización manual de productos desde sistemas internos como GECOM

## Differentiators

### Multi-tenant con Personalización por División
- Cada división mantiene su identidad visual (colores, logos, datos de contacto)
- Subdominio personalizado para cada división (division.empresamatriz.com)
- Configuración independiente manteniendo catálogo base centralizado

### Sincronización Automática con GECOM
- Integración nativa con archivos GECOM para productos, categorías y precios
- Actualización automática sin intervención manual
- Mantenimiento de consistencia entre sistema interno y catálogo público

### Acceso Público Sin Autenticación
- Compradores acceden directamente sin registro previo
- Resolución automática por subdominio
- Experiencia de usuario optimizada para consulta rápida

### Arquitectura Moderna y Escalable
- Clean Architecture con .NET 9.0
- Patrón CQRS para separación de responsabilidades
- API RESTful bien documentada con Swagger

## Key Features

### Gestión Multi-tenant
- Configuración por división: colores, logos, datos de contacto
- Resolución automática por subdominio
- Herencia de catálogo base con personalizaciones específicas

### Catálogo Público Avanzado
- Búsqueda y filtrado por categorías, marcas y precios
- Sistema de agrupaciones para destacar novedades y ofertas
- Gestión de imágenes de productos
- API pública sin autenticación requerida

### Sincronización Inteligente
- Import automático desde archivos GECOM
- Sincronización de productos, categorías y listas de precios
- Mantenimiento de relaciones y jerarquías

### Sistema de Administración
- Autenticación JWT para administradores
- CRUD completo para productos, categorías y configuraciones
- Dashboard de gestión con métricas y reportes

### Tecnología Robusta
- Backend: ASP.NET Core Web API (.NET 9.0)
- Base de datos: MySQL con Entity Framework Core
- Logging: Serilog con dashboard Seq
- Validación: FluentValidation con AutoMapper