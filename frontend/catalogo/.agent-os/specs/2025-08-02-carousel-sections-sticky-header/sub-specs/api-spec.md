# API Specification

This is the API specification for the spec detailed in @.agent-os/specs/2025-08-02-carousel-sections-sticky-header/spec.md

## Endpoints

### GET /api/catalog/novedades

**Purpose:** Obtiene lista de productos marcados como novedades para la empresa
**Parameters:** 
- `empresaId` (query, number) - ID de la empresa (automático por subdominio en producción)
**Response:** 
```json
{
  "success": true,
  "message": null,
  "productos": [
    {
      "codigo": "string",
      "nombre": "string",
      "descripcion": "string", 
      "descripcion_corta": "string",
      "precio": number,
      "destacado": boolean,
      "imagen_urls": string[],
      "stock": number,
      "tags": string[],
      "marca": string | null,
      "unidad": "string",
      "codigo_barras": "string",
      "codigo_rubro": number,
      "imagen_alt": "string",
      "tipo_producto": "string",
      "lista_precio_id": number,
      "lista_precio_nombre": "string",
      "lista_precio_codigo": "string"
    }
  ],
  "total_productos": number,
  "empresa_nombre": "string",
  "fecha_consulta": "string (ISO date)"
}
```
**Errors:** 
- 404: Empresa no encontrada
- 500: Error interno del servidor

### GET /api/catalog/ofertas

**Purpose:** Obtiene lista de productos marcados como ofertas para la empresa
**Parameters:**
- `empresaId` (query, number) - ID de la empresa (automático por subdominio en producción)
**Response:** Mismo formato que `/api/catalog/novedades`
**Errors:** 
- 404: Empresa no encontrada  
- 500: Error interno del servidor

## Service Layer Integration

### Nuevos Métodos en catalogService.ts

```typescript
// Obtiene productos marcados como novedades
export async function getNovedades(empresaId?: number): Promise<Product[]> {
  const controller = new AbortController()
  
  try {
    const url = buildApiUrl('/api/catalog/novedades', { empresaId })
    const response = await fetchWithTimeout(url, { 
      signal: controller.signal 
    })
    
    if (!response.success) {
      throw new Error(response.message || 'Error al cargar novedades')
    }
    
    return response.productos || []
  } catch (error) {
    console.error('Error fetching novedades:', error)
    return []
  }
}

// Obtiene productos marcados como ofertas  
export async function getOfertas(empresaId?: number): Promise<Product[]> {
  const controller = new AbortController()
  
  try {
    const url = buildApiUrl('/api/catalog/ofertas', { empresaId })
    const response = await fetchWithTimeout(url, {
      signal: controller.signal
    })
    
    if (!response.success) {
      throw new Error(response.message || 'Error al cargar ofertas')
    }
    
    return response.productos || []
  } catch (error) {
    console.error('Error fetching ofertas:', error) 
    return []
  }
}
```

### Cache Implementation

```typescript
// Simple cache con TTL de 5 minutos
const cache = new Map<string, { data: Product[], timestamp: number }>()
const CACHE_TTL = 5 * 60 * 1000 // 5 minutos

function getCachedData(key: string): Product[] | null {
  const cached = cache.get(key)
  if (cached && Date.now() - cached.timestamp < CACHE_TTL) {
    return cached.data
  }
  cache.delete(key)
  return null
}

function setCachedData(key: string, data: Product[]): void {
  cache.set(key, { data, timestamp: Date.now() })
}
```

## Error Handling

- **Network Errors:** Retorno de array vacío con log del error
- **Invalid Response:** Validación de estructura de respuesta antes de procesar
- **Timeout:** AbortController con timeout de 10 segundos
- **Empty Results:** Manejo graceful de respuestas sin productos
- **Fallback UI:** Componentes deben manejar arrays vacíos sin romper la interfaz