# Migración a Sistema de Listas de Precios

## 🎯 Objetivo
Migrar el sistema de precio único por producto a un sistema de listas de precios, creando componentes reutilizables que funcionen tanto para productos-base como productos-empresa.

## 📋 Contexto
- Los productos ya no tienen un campo `precio` único
- Cada producto puede tener múltiples precios según diferentes listas
- Necesitamos mantener la UX existente pero adaptada al nuevo modelo
- Los componentes deben ser reutilizables entre módulos

## 🚀 Plan de Implementación

### Fase 1: Fundamentos y Types
- [ ] **1.1** Actualizar tipos centrales en `src/types/productos.ts`
  - [ ] Agregar `ListaPrecioInfo`
  - [ ] Agregar `ProductoPrecioInfo` 
  - [ ] Agregar `UpsertPrecioProductoCommand`
  - [ ] Agregar `UpdatePrecioProductoRequest`
  - [ ] Agregar `GetPreciosPorProductoQueryResult`
  - [ ] Modificar `ProductoBaseDto` para incluir `precio_seleccionado` y `precios[]`
  - [ ] Actualizar `GetAllProductosBaseQueryResult` para listas disponibles

### Fase 2: Composables Reutilizables
- [ ] **2.1** Crear `src/composables/useListasPrecios.ts`
  - [ ] Gestión de listas disponibles
  - [ ] Cache de listas para performance
  - [ ] Métodos CRUD para listas
  - [ ] Estado reactivo de lista seleccionada

- [ ] **2.2** Crear `src/composables/useProductoPrecios.ts`
  - [ ] Lógica compartida para gestión de precios
  - [ ] `fetchProductoPrecios(id, type?)` - obtener precios de un producto
  - [ ] `updatePrecio(productoId, listaId, precio, type?)` - actualizar precio
  - [ ] `createPrecio(productoId, listaId, precio, type?)` - crear precio
  - [ ] `deletePrecio(productoId, listaId, type?)` - eliminar precio
  - [ ] Manejo de errores centralizado

### Fase 3: Componentes UI Reutilizables
- [ ] **3.1** Crear `src/components/ui/ListaPreciosSelector.vue`
  - [ ] Selector dropdown de listas disponibles
  - [ ] Props: `modelValue`, `listas`, `placeholder?`
  - [ ] Persistencia en localStorage de lista seleccionada
  - [ ] Indicador visual de lista activa

- [ ] **3.2** Crear `src/components/ui/PreciosList.vue`
  - [ ] Tabla/lista de precios de un producto
  - [ ] Props: `productId`, `precios`, `readonly?`, `productType?`
  - [ ] Eventos: `precio-updated`, `precio-created`, `precio-deleted`
  - [ ] Edición inline de precios
  - [ ] Agregar precio en nueva lista

- [ ] **3.3** Crear `src/components/ui/PrecioField.vue`
  - [ ] Campo individual para editar precio
  - [ ] Validación de formato de moneda argentina
  - [ ] Estados: edición, guardando, error
  - [ ] Formateo automático de números

### Fase 4: Helpers y Utilidades
- [ ] **4.1** Crear `src/utils/precios-endpoints.ts`
  - [ ] Factory functions para URLs dinámicas
  - [ ] `getPreciosUrl(type: 'base' | 'empresa', productId: number)`
  - [ ] `getUpdatePrecioUrl(type, productId, listaId)`
  - [ ] `getCreatePrecioUrl(type)`
  - [ ] `getDeletePrecioUrl(type, productId, listaId)`

### Fase 5: Actualización Módulo Productos Base
- [ ] **5.1** Actualizar `src/composables/useProductosBase.ts`
  - [ ] Agregar parámetro `listaPrecioId` opcional a `fetchProductos`
  - [ ] Integrar con `useProductoPrecios` 
  - [ ] Mantener compatibilidad con código existente
  - [ ] Actualizar métodos para nueva estructura de datos

- [ ] **5.2** Actualizar `src/components/productos-base/ProductosBaseTable.vue`
  - [ ] Mostrar `precio_seleccionado` en lugar de `precio`
  - [ ] Indicador de qué lista se está viendo
  - [ ] Tooltip con información de listas disponibles
  - [ ] Manejar productos sin precio en lista seleccionada

- [ ] **5.3** Actualizar `src/components/productos-base/ProductoBaseForm.vue`
  - [ ] Remover campo precio directo del formulario
  - [ ] Integrar componente `PreciosList`
  - [ ] Separar guardado: datos del producto primero, luego precios
  - [ ] Validaciones actualizadas

- [ ] **5.4** Actualizar `src/pages/admin/productos-base/index.vue`
  - [ ] Agregar `ListaPreciosSelector` en sección de filtros
  - [ ] Gestionar estado de lista seleccionada
  - [ ] Pasar lista seleccionada a tabla y componentes
  - [ ] Persistir selección entre navegaciones

- [ ] **5.5** Actualizar páginas de edición y creación
  - [ ] `src/pages/admin/productos-base/create.vue`
  - [ ] `src/pages/admin/productos-base/[id]/edit.vue`
  - [ ] Flujo: crear producto → configurar precios
  - [ ] Manejo de estados de carga

### Fase 6: Actualización Módulo Productos Empresa
- [ ] **6.1** Crear/actualizar `src/composables/useProductosEmpresa.ts`
  - [ ] Reutilizar lógica de `useProductoPrecios`
  - [ ] Adaptar endpoints específicos de empresa
  - [ ] Mantener funcionalidades específicas de empresa

- [ ] **6.2** Actualizar componentes productos-empresa
  - [ ] Reutilizar componentes UI creados
  - [ ] Adaptar props específicas (empresaId)
  - [ ] Misma UX que productos-base

- [ ] **6.3** Actualizar páginas productos-empresa
  - [ ] Integrar nuevos componentes
  - [ ] Selector de listas de precios
  - [ ] Gestión de precios por empresa

### Fase 7: Testing y Validación
- [ ] **7.1** Testing de componentes reutilizables
  - [ ] Verificar props y eventos funcionan correctamente
  - [ ] Validar en ambos contextos (base y empresa)
  - [ ] Testing de edge cases (productos sin precios)

- [ ] **7.2** Testing de flujos completos
  - [ ] Crear producto con precios
  - [ ] Editar precios existentes
  - [ ] Cambiar entre listas de precios
  - [ ] Eliminar precios de listas específicas

- [ ] **7.3** Testing de performance
  - [ ] Verificar cache de listas funciona
  - [ ] Optimizar llamadas API
  - [ ] Estados de carga apropiados

### Fase 8: Polish y UX
- [ ] **8.1** Refinamiento de UX
  - [ ] Transiciones suaves entre listas
  - [ ] Feedback visual para cambios de precio
  - [ ] Estados de error claros
  - [ ] Loading states consistentes

- [ ] **8.2** Documentación
  - [ ] Comentarios en componentes reutilizables
  - [ ] Documentar props y eventos
  - [ ] Ejemplos de uso

## 🔄 Flujo de Trabajo Actualizado

### Listado de Productos (Ambos Módulos):
1. **Usuario selecciona lista de precios** en selector (persiste en localStorage)
2. **Tabla muestra productos** con precio de lista seleccionada
3. **Productos sin precio** en esa lista muestran "-"
4. **Indicador visual** de qué lista está activa

### Edición de Producto:
1. **Formulario de datos** del producto (sin campo precio)
2. **Sección separada** con componente `PreciosList`
3. **Guardado secuencial**: producto primero, luego precios modificados
4. **Feedback individual** por cada precio actualizado

### Creación de Producto:
1. **Crear producto base** primero (campos obligatorios)
2. **Redirigir a edición** para configurar precios
3. **Permitir configurar precios** inmediatamente después de creación

## 🎨 Estructura de Componentes Reutilizables

```
src/components/ui/
├── ListaPreciosSelector.vue    # Selector de lista activa
├── PreciosList.vue            # Gestión completa de precios
├── PrecioField.vue            # Campo individual de precio
└── ImageUpload.vue            # Ya existe, reutilizable

src/composables/
├── useListasPrecios.ts        # Gestión de listas disponibles
├── useProductoPrecios.ts      # Lógica común de precios
├── useProductosBase.ts        # Adaptado para listas
└── useProductosEmpresa.ts     # Reutiliza lógica de precios

src/utils/
└── precios-endpoints.ts       # Factory de URLs dinámicas
```

## 📊 Endpoints API Utilizados

### Listas de Precios:
- `GET /api/listas-precios` - Obtener todas las listas

### Precios por Producto:
- `GET /api/productos-precios/producto/{id}` - Todos los precios
- `GET /api/productos-precios/producto/{id}/lista/{listaId}` - Precio específico
- `PUT /api/productos-precios/producto/{id}/lista/{listaId}` - Actualizar precio
- `POST /api/productos-precios` - Crear nuevo precio
- `DELETE /api/productos-precios/producto/{id}/lista/{listaId}` - Eliminar precio

### Productos (Actualizados):
- `GET /api/ProductosBase?listaPrecioId={id}` - Con precios de lista específica
- `GET /api/ProductosEmpresa?listaPrecioId={id}` - Con precios de lista específica

## ⚠️ Consideraciones Técnicas

### Compatibilidad:
- Mantener funcionamiento durante migración
- Fallback para productos sin precios configurados
- Migración gradual de datos existentes

### Performance:
- Cache de listas de precios (localStorage + memory)
- Lazy loading de precios por producto
- Debounce en campos de edición de precios

### UX:
- Estados de carga claros
- Feedback inmediato en cambios
- Consistencia entre módulos
- Manejo de errores informativo

## 🚨 Rollback Strategy

Si necesitamos revertir:
1. Los endpoints antiguos siguen funcionando
2. Los tipos antiguos se mantienen como fallback
3. Feature flag para habilitar/deshabilitar nuevo sistema
4. Migración de datos reversible

## 📅 Timeline Estimado

- **Fase 1-2**: 2-3 días (tipos y composables)
- **Fase 3**: 2-3 días (componentes UI)
- **Fase 4-5**: 3-4 días (productos-base)
- **Fase 6**: 2-3 días (productos-empresa)
- **Fase 7-8**: 2-3 días (testing y polish)

**Total estimado**: 11-16 días de desarrollo

## ✅ Criterios de Éxito

1. **Funcionalidad**: Todos los CRUDs funcionan con listas de precios
2. **Reutilización**: Componentes funcionan en ambos módulos
3. **UX**: Experiencia fluida y consistente
4. **Performance**: Sin degradación notable
5. **Mantenibilidad**: Código limpio y documentado

## 📝 Notas

- Priorizar reutilización sobre optimización prematura
- Mantener simplicidad en cada cambio
- Testing continuo en ambos módulos
- Documentar decisiones técnicas importantes