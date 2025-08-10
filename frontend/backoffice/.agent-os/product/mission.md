# Product Mission

## Pitch

DistriCatalogo Manager es un sistema de backoffice multi-tenant que permite a una empresa principal gestionar catálogos web que son consumidos por múltiples sucursales (empresas cliente), cada una con personalización propia, bajo un modelo Hub-and-Spoke completamente funcional.

## Users

### Primary Customers

- **Empresas Distribuidoras**: Compañías que necesitan gestionar un catálogo centralizado para múltiples sucursales o franquicias
- **Redes de Sucursales**: Organizaciones con múltiples puntos de venta que requieren catálogos personalizados pero centralizados
- **Franquicias**: Sistemas de franquicia que necesitan mantener consistencia en productos pero permitir personalización local

### User Personas

**Administrador de Empresa Principal** (35-50 años)
- **Role:** Director de Sistemas / Gerente de Operaciones
- **Context:** Gestiona el catálogo centralizado y configura todas las sucursales
- **Pain Points:** Dificultad para mantener catálogos actualizados en múltiples ubicaciones, falta de control centralizado
- **Goals:** Centralizar gestión de productos, mantener consistencia de marca, facilitar operaciones de sucursales

**Administrador de Empresa Cliente** (30-45 años)
- **Role:** Gerente de Sucursal / Encargado de Ventas
- **Context:** Administra su sucursal específica con acceso a catálogo base más personalización local
- **Pain Points:** Dependencia de administradores centrales para cambios, limitaciones en personalización
- **Goals:** Personalizar su catálogo local, gestionar productos propios, mantener autonomía operativa

## The Problem

### Gestión Fragmentada de Catálogos Multi-Sucursal

Las empresas distribuidoras enfrentan el desafío de mantener catálogos actualizados y personalizados para múltiples sucursales. Los sistemas tradicionales requieren gestión manual por sucursal, resultando en inconsistencias y duplicación de esfuerzos que afecta hasta el 40% del tiempo productivo del personal administrativo.

**Our Solution:** Sistema centralizado Hub-and-Spoke que permite gestión unificada del catálogo base con personalización automática por sucursal.

### Falta de Personalización por Sucursal

Las sucursales necesitan reflejar su identidad local (colores, logos, productos específicos) pero los sistemas centralizados típicos no permiten esta flexibilidad. Esto resulta en baja adopción y satisfacción del cliente final.

**Our Solution:** Sistema de personalización visual completo con gestión de productos propios por sucursal según permisos configurables.

### Sincronización Manual de Datos

La actualización de precios, stock y productos desde sistemas ERP como Gecom requiere procesos manuales propensos a errores. Esto genera inconsistencias que afectan las operaciones diarias.

**Our Solution:** Sistema de sincronización automática desde Gecom con logs detallados y manejo de errores.

## Differentiators

### Arquitectura Hub-and-Spoke Nativa

Unlike sistemas de catálogo tradicionales que requieren instalaciones separadas por ubicación, DistriCatalogo Manager proporciona una arquitectura nativa multi-tenant con acceso por subdominio. This results in reducción de costos operativos del 60% y administración centralizada completa.

### Personalización Granular por Empresa Cliente

Unlike plataformas de e-commerce genéricas, ofrecemos personalización completa por sucursal (visual, productos, permisos) manteniendo consistencia del catálogo base. This results in mayor adopción por parte de sucursales y mejor experiencia de marca.

### Integración Nativa con Sistemas ERP

Unlike soluciones que requieren middleware complejo, proporcionamos integración directa con sistemas como Gecom con sincronización automática y manejo de errores. This results in datos siempre actualizados y reducción de trabajo manual administrativo.

## Key Features

### Core Features

- **Gestión de Catálogo Base:** Administración centralizada de productos y categorías principal con control completo desde empresa hub
- **Gestión Multi-Tenant:** Sistema de empresas cliente con acceso por subdominio y permisos granulares configurables
- **Autenticación JWT Completa:** Sistema de autenticación robusto con refresh tokens y permisos contextuales por empresa
- **Personalización Visual:** Configuración completa de colores, logos, favicon y datos de contacto por empresa cliente

### Collaboration Features

- **Sistema de Permisos Granulares:** Control fino de qué puede hacer cada tipo de usuario en cada empresa cliente
- **Gestión de Usuarios Multi-Empresa:** Administradores principales pueden gestionar usuarios de todas las empresas cliente
- **Productos y Categorías por Empresa:** Empresas cliente pueden agregar productos propios según permisos configurados
- **Sincronización Automática Gecom:** Integración directa con sistema ERP Gecom para actualización automática de datos

### Advanced Features

- **Sistema de Agrupaciones:** Organización flexible de productos por empresa cliente con drag-and-drop
- **Gestión de Listas de Precios:** Múltiples listas de precios por empresa con configuración flexible
- **Sistema de Imágenes Avanzado:** Upload, redimensionamiento automático y gestión de múltiples formatos de imagen
- **Dashboard con Estadísticas:** Métricas y reportes contextuales según tipo de empresa y permisos