# Spec Tasks

Estas son las tareas a completar para la spec detallada en @.agent-os/specs/2025-08-04-stock-diferencial-empresa/spec.md

> Created: 2025-08-04
> Status: Ready for Implementation

## Tasks

### 1. Composables y Lógica de Negocio
- [x] **Crear composable useStockDiferencial**
  - Función para obtener stock por empresa (GET con empresaId)
  - Función para actualizar stock por empresa (PUT con empresaId)
  - Función para resetear stock diferencial
  - Manejo de estados loading/error
  - Tipos TypeScript apropiados

- [x] **Extender composables existentes**
  - Modificar useProductos para incluir parámetro empresaId
  - Asegurar compatibilidad con funcionalidad existente
  - Agregar métodos específicos para stock diferencial

### 2. Componentes UI
- [x] **Crear EmpresaSelector component**
  - Selector dropdown con lista de empresas
  - Filtrado/búsqueda de empresas
  - Estado reactivo para empresa seleccionada
  - Validaciones de selección requerida

- [x] **Crear StockDiferencial component**
  - Input numérico para edición de stock
  - Indicadores visuales (badge) para stock diferencial
  - Botones para guardar/resetear
  - Feedback visual de cambios pendientes

- [x] **Crear componente StockIndicator**
  - Mostrar diferencia entre stock base vs diferencial
  - Indicadores de color para diferentes estados
  - Tooltip con información detallada

### 3. Modificaciones de Páginas
- [x] **Modificar página productos/[id].vue**
  - Integrar EmpresaSelector en la interfaz
  - Mostrar StockDiferencial cuando hay empresa seleccionada
  - Mantener funcionalidad existente de productos base
  - Agregar navegación de breadcrumbs con empresa

- [x] **Modificar página productos/index.vue (listado)**
  - Agregar selector de empresa en vista de listado
  - Mostrar indicadores de stock diferencial en la tabla
  - Filtros adicionales para productos con stock diferencial

### 4. Integración API
- [ ] **Configurar calls API con empresaId**
  - Modificar fetch calls para incluir parámetro empresaId
  - Manejo de respuestas específicas por empresa
  - Cache inteligente por empresa
  - Error handling específico para stock diferencial

### 5. Estados y Stores
- [ ] **Extender store de productos**
  - Estado para empresa seleccionada globalmente
  - Cache de stock diferencial por empresa
  - Acciones para CRUD de stock diferencial
  - Sincronización de estados entre componentes

### 6. UX/UI Polish
- [ ] **Implementar indicadores visuales**
  - Badges para identificar stock diferencial vs base
  - Estados loading durante operaciones
  - Confirmaciones para cambios importantes
  - Mensajes de éxito/error específicos

- [ ] **Mejorar navegación**
  - Persistir empresa seleccionada en sesión
  - Breadcrumbs con empresa actual
  - Navegación rápida entre empresas

### 7. Testing y Validaciones
- [ ] **Validaciones frontend**
  - Stock debe ser número positivo
  - Empresa debe estar seleccionada para operaciones
  - Validar permisos de usuario para empresa

- [ ] **Testing de integración**
  - Probar flujo completo de edición stock diferencial
  - Verificar que no afecta productos base
  - Testing de cambios entre empresas

### 8. Documentación
- [ ] **Documentar componentes nuevos**
  - Props y eventos de EmpresaSelector
  - Props y eventos de StockDiferencial
  - Ejemplos de uso en README

- [ ] **Actualizar documentación existente**
  - Documentar nuevos parámetros en composables
  - Agregar ejemplos de uso de stock diferencial