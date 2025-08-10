# Spec Requirements Document

> Spec: Simplificar Procesador Gecom para Solo Dual Processing
> Created: 2025-08-04
> Status: Planning

## Overview

Simplificar el procesador CSV Gecom eliminando todas las configuraciones y rutas de código que no se utilizan en producción, manteniendo únicamente el modo dual processing (productos + precios). Este refactoring reducirá la complejidad del código, mejorará la mantenibilidad y eliminará puntos de falla innecesarios.

## User Stories

### Desarrollador que Mantiene el Código
As a developer, I want to work with a simplified codebase that only contains the functionality we actually use, so that I can understand, maintain and debug the code more efficiently.

El desarrollador podrá navegar por un código más limpio, sin rutas condicionales complejas ni configuraciones obsoletas. Esto facilitará la implementación de nuevas funcionalidades y la resolución de bugs.

### Administrador de Sistema
As a system administrator, I want a streamlined configuration file that only contains the settings we actually use, so that deployment and configuration management is simpler and less error-prone.

El administrador tendrá una configuración más clara y concisa, eliminando la posibilidad de configurar opciones que no se usan o que pueden causar comportamientos inesperados.

## Spec Scope

1. **Eliminación de Procesamiento Paralelo** - Remover toda la lógica de UseParallelProcessing y MaxParallelChunks
2. **Eliminación de API Bulk** - Remover UseBulkApi y todos los métodos relacionados con ProcessProductsBulkAsync
3. **Eliminación de Configuración de Listas de Precios** - Remover ListasPreciosConfig y listas-config.json ya que no se usa
4. **Simplificación de Program.cs** - Eliminar los métodos de procesamiento legacy y bulk, mantener solo dual processing
5. **Limpieza de AppSettings** - Mantener solo las configuraciones que se usan en producción
6. **Eliminación de Servicios No Utilizados** - Remover ListaPreciosService y otros servicios relacionados con funcionalidades eliminadas

## Out of Scope

- Modificación de la lógica de limpieza y validación de CSV (esta funciona perfectamente)
- Cambios en la integración con la API de DistriCatalogo (DualFileProcessor)
- Modificación de la estructura de archivos de entrada/salida
- Cambios en el sistema de logging estructurado

## Expected Deliverable

1. Codebase simplificado que solo ejecuta dual processing mode sin opciones condicionales
2. Archivo appsettings.json limpio que solo contiene las configuraciones utilizadas en producción
3. Program.cs refactorizado sin rutas de código muertas o configuraciones obsoletas