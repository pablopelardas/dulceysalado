# Technical Specification

Esta es la especificación técnica para la spec detallada en @.agent-os/specs/2025-08-04-stock-diferencial-empresa/spec.md

> Created: 2025-08-04
> Version: 1.0.0

## Technical Requirements

### Framework y Tecnologías
- **Frontend:** Nuxt 3 + Vue 3 + TypeScript
- **Estado:** Pinia stores existentes
- **API:** Endpoints existentes con parámetro empresaId
- **Componentes:** Nuxt UI existente

### Arquitectura Actual
- Sistema Hub-and-Spoke
- Empresa principal gestiona productos base
- Stock diferencial por empresa ya implementado en backend
- API endpoints: GET/PUT con parámetro empresaId

## Approach

### 1. Composables
```typescript
// composables/useStockDiferencial.ts
interface StockDiferencialOptions {
  empresaId: string
  productoId: string
}

export const useStockDiferencial = () => {
  const getStockByEmpresa = async (options: StockDiferencialOptions)
  const updateStockByEmpresa = async (options: StockDiferencialOptions & { stock: number })
  const resetStockDiferencial = async (options: StockDiferencialOptions)
}
```

### 2. Componentes
```vue
<!-- components/productos/EmpresaSelector.vue -->
<template>
  <USelectMenu
    v-model="selectedEmpresa"
    :options="empresas"
    placeholder="Seleccionar empresa"
  />
</template>

<!-- components/productos/StockDiferencial.vue -->
<template>
  <div class="stock-diferencial">
    <UBadge v-if="isDiferencial" color="blue">Stock Diferencial</UBadge>
    <UInput 
      v-model="stockValue"
      type="number"
      @blur="updateStock"
    />
  </div>
</template>
```

### 3. Modificaciones de Páginas
```vue
<!-- pages/productos/[id].vue - Modificar página existente -->
<template>
  <div>
    <EmpresaSelector v-model="selectedEmpresa" />
    <StockDiferencial 
      :producto-id="route.params.id"
      :empresa-id="selectedEmpresa?.id"
    />
  </div>
</template>
```

### 4. Integración API
```typescript
// API calls usando endpoints existentes
GET /api/productos/{id}/stock?empresaId={empresaId}
PUT /api/productos/{id}/stock?empresaId={empresaId}
```

### 5. Estados y Reactividad
- Estado reactivo para empresa seleccionada
- Carga automática de stock al cambiar empresa
- Indicadores de loading durante operaciones
- Manejo de errores específicos

### 6. UI/UX Considerations
- Selector de empresa prominente en la interface
- Indicadores visuales claros para stock diferencial vs base
- Feedback inmediato en cambios de stock
- Validaciones frontend antes de envío

## External Dependencies

### APIs Existentes
- **GET** `/api/productos/{id}/stock?empresaId={empresaId}` - Obtener stock específico por empresa
- **PUT** `/api/productos/{id}/stock?empresaId={empresaId}` - Actualizar stock específico por empresa
- **GET** `/api/empresas` - Listar empresas disponibles (asumido existente)

### Composables Existentes
- `useProductos()` - Extender para incluir stock diferencial
- `useEmpresas()` - Para obtener lista de empresas

### Stores Existentes
- Productos store - Modificar para manejar stock por empresa
- Empresas store - Para gestión de empresas seleccionables

### Componentes UI
- NuxtUI components (USelectMenu, UInput, UBadge, etc.)
- Componentes de productos existentes para extender

### Validaciones
- Validación de stock numérico positivo
- Validación de empresa seleccionada antes de operaciones
- Manejo de permisos existentes del sistema