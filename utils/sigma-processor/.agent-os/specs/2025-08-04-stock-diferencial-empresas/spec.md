# Spec Requirements Document

> Spec: Stock Diferencial por Empresas 
> Created: 2025-08-04
> Status: Planning

## Overview

Extender el procesador Gecom para incluir un tercer archivo CSV con stock diferencial por empresa, donde cada columna del archivo representa diferentes empresas del sistema DistriCatalogo. Esta funcionalidad permitirá sincronizar stocks específicos por empresa para cada producto, complementando el procesamiento dual existente de productos y precios.

## User Stories

### Administrador del Sistema
As a system administrator, I want to process a third CSV file containing stock levels per company, so that each company in DistriCatalogo has accurate inventory data for their specific warehouse/location.

El administrador colocará tres archivos en la carpeta input: productos.csv, precios.csv y stocks.csv. El procesador sincronizará automáticamente los stocks por empresa para cada producto, mapeando las columnas del CSV a los IDs de empresa correspondientes en el sistema.

### Empresa Cliente
As a company client, I want my specific stock levels to be accurately reflected in the system, so that I can manage my inventory independently of other companies sharing the same product catalog.

Cada empresa verá únicamente su stock específico en el sistema, permitiendo gestión independiente de inventarios mientras comparten el catálogo de productos común.

## Spec Scope

1. **Procesamiento de Triple Archivo** - Agregar soporte para un tercer archivo stocks.csv en el modo dual processing existente
2. **Mapeo de Empresas** - Configurar mapeo fijo de columnas CSV a IDs de empresa en DistriCatalogo
3. **Extensión de DTOs** - Modificar los modelos existentes para incluir información de stock por empresa
4. **Limpieza de Decimales** - Reutilizar la lógica existente del CsvCleaner para manejar comas decimales en stocks
5. **Envío a API** - Extender el payload enviado a la API para incluir el arreglo de stocks por empresa

## Out of Scope

- Modificación del endpoint de la API DistriCatalogo (se ajustará por separado)
- Configuración dinámica del mapeo empresa-columna (será hardcodeado)
- Soporte para empresas adicionales más allá de SAVIO (se agregará en el futuro)
- Interface web para gestión de stocks
- Validación de existencia de empresas en la API

## Expected Deliverable

1. Procesador que maneja exitosamente tres archivos CSV (productos, precios, stocks) de forma simultánea
2. DTO extendido que incluye array de stocks por empresa enviado correctamente a la API
3. Mapeo correcto de columnas CSV FIAM→ID1, GOLOCINO→ID12&18, CARUPA→ID17&13, BENAVIDES→ID16, SAVIO→preparado para futuro