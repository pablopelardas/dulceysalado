# Especificación: Simplificación y Personalización del Catálogo Dulce y Salado

## 1. Resumen Ejecutivo

### Objetivo
Transformar la aplicación multi-tenant actual en una implementación dedicada y personalizada para "Dulce y Salado", eliminando la lógica de resolución dinámica de empresas y aplicando un diseño exclusivo acorde a la marca.

### Alcance
- Eliminar dependencias de configuración dinámica de empresa
- Hardcodear información de la empresa en el código
- Rediseñar interfaz con nueva paleta de colores y distribución
- Mantener integración con API backend (empresa ID = 1)
- Preparar estructura para futuras features (autenticación, pedidos)

## 2. Cambios Arquitectónicos

### 2.1 Eliminación de Lógica Multi-tenant

**Estado Actual:**
- La aplicación resuelve empresa por subdominios
- Carga configuración desde `/api/catalog/empresa`
- Aplica temas dinámicamente
- Gestiona features habilitadas por empresa

**Estado Objetivo:**
- Configuración de empresa hardcodeada
- Tema fijo personalizado
- Features específicas para este cliente
- Sin resolución de subdominios

### 2.2 Configuración Estática de Empresa

```typescript
// src/config/empresa.config.ts
export const EMPRESA_CONFIG = {
  id: 1,
  codigo: "DULCEYSALADO",
  nombre: "Dulce y Salado",
  telefono: "",
  email: "",
  direccion: "Av. Bernardo Ader 161, B1609 Boulogne, Provincia de Buenos Aires",
  
  // URLs de recursos
  logoUrl: "/assets/logo-dulceysalado.png", // Local
  faviconUrl: "/favicon.ico",
  
  // Redes sociales
  whatsapp: "",
  facebook: "",
  instagram: "",
  
  // Configuración de catálogo
  mostrarPrecios: true,
  mostrarStock: false,
  permitirPedidos: false,
  productosPorPagina: 100,
  
  // Features activas (simplificado)
  features: {
    autenticacion: true,
    pedidosWhatsapp: false,
    clientesMayoristas: false
  }
};
```

## 3. Diseño Visual

### 3.1 Paleta de Colores

```scss
// src/styles/variables.scss
:root {
  // Colores principales
  --color-negro: #000000;
  --color-blanco: #FFFFFF;
  --color-rojo: #E50000;
  --color-gris-oscuro: #1E1E1E;
  --color-gris-medio: #4A4A4A;
  --color-gris-claro: #F5F5F5;
  
  // Aplicación semántica
  --color-primario: var(--color-negro);
  --color-secundario: var(--color-blanco);
  --color-acento: var(--color-rojo);
  --color-fondo: var(--color-blanco);
  --color-fondo-alternativo: var(--color-gris-claro);
  --color-texto: var(--color-negro);
  --color-texto-suave: var(--color-gris-medio);
  
  // Estados
  --color-hover: var(--color-rojo);
  --color-activo: #CC0000;
  --color-deshabilitado: var(--color-gris-medio);
}
```

### 3.2 Tipografía

```scss
// Importar fuentes
@import url('https://fonts.googleapis.com/css2?family=Poppins:wght@400;600;700;800&family=Open+Sans:wght@400;500;600&display=swap');

:root {
  // Familias tipográficas
  --font-heading: 'Poppins', sans-serif;
  --font-body: 'Open Sans', sans-serif;
  
  // Tamaños
  --text-xs: 0.75rem;
  --text-sm: 0.875rem;
  --text-base: 1rem;
  --text-lg: 1.125rem;
  --text-xl: 1.25rem;
  --text-2xl: 1.5rem;
  --text-3xl: 1.875rem;
  --text-4xl: 2.25rem;
  
  // Pesos
  --font-normal: 400;
  --font-medium: 500;
  --font-semibold: 600;
  --font-bold: 700;
  --font-extrabold: 800;
}
```

### 3.3 Estructura de Layout

#### Header
```vue
<!-- src/components/layout/AppHeader.vue -->
<template>
  <header class="header-main">
    <div class="container">
      <div class="header-content">
        <!-- Logo sin restricciones de tamaño -->
        <div class="logo-container">
          <img src="/assets/logo-dulceysalado.png" alt="Dulce y Salado" />
        </div>
        
        <!-- Navegación desktop -->
        <nav class="nav-desktop">
          <RouterLink to="/">Inicio</RouterLink>
          <RouterLink to="/catalogo">Catálogo</RouterLink>
          <RouterLink to="/ofertas">Ofertas</RouterLink>
          <RouterLink to="/contacto">Contacto</RouterLink>
        </nav>
        
        <!-- Menú móvil -->
        <button class="menu-toggle">
          <span></span>
        </button>
      </div>
    </div>
  </header>
</template>
```

#### Hero Section
```vue
<!-- src/components/home/HeroSection.vue -->
<template>
  <section class="hero">
    <div class="hero-content">
      <h1>La mejor selección Dulce & Salado directo a tu negocio</h1>
      <p>Descubrí nuestra variedad de productos para tu comercio</p>
      <button class="btn-hero">Ver Catálogo</button>
    </div>
    <div class="hero-image">
      <!-- Imagen dividida dulce/salado -->
    </div>
  </section>
</template>
```

## 4. Cambios en Componentes

### 4.1 Eliminación de CompanyStore

**Archivos a eliminar:**
- `src/stores/companyStore.ts`
- `src/composables/useTheme.ts` (versión dinámica)

**Archivos a modificar:**
- `src/App.vue` - Eliminar carga de empresa
- `src/services/api.ts` - Simplificar, siempre usar empresaId=1

### 4.2 Nuevo ThemeProvider

```typescript
// src/providers/ThemeProvider.ts
export class ThemeProvider {
  static apply() {
    // Aplicar tema fijo de Dulce y Salado
    document.documentElement.style.setProperty('--color-primario', '#000000');
    document.documentElement.style.setProperty('--color-acento', '#E50000');
    // ... resto de variables
  }
}
```

### 4.3 Simplificación del CatalogService

```typescript
// src/services/catalogService.ts
export class CatalogService {
  private static readonly EMPRESA_ID = 1;
  
  static async getProducts(params: ProductParams) {
    return api.get('/catalog', {
      params: {
        ...params,
        empresaId: this.EMPRESA_ID
      }
    });
  }
  
  static async getCategories() {
    return api.get('/catalog/categorias', {
      params: {
        empresaId: this.EMPRESA_ID
      }
    });
  }
  
  // ... resto de métodos
}
```

## 5. Nuevos Componentes UI

### 5.1 CategoryGrid
```vue
<!-- src/components/catalog/CategoryGrid.vue -->
<template>
  <div class="category-grid">
    <div v-for="category in categories" :key="category.id" class="category-card">
      <div class="category-image">
        <img :src="category.imagen_url || defaultImage" :alt="category.nombre" />
      </div>
      <h3>{{ category.nombre }}</h3>
      <RouterLink :to="`/catalogo?categoria=${category.id}`">
        Ver productos
      </RouterLink>
    </div>
  </div>
</template>
```

### 5.2 ProductCard Mejorado
```vue
<!-- src/components/catalog/ProductCard.vue -->
<template>
  <article class="product-card">
    <div class="product-image">
      <img :src="product.imagen_url" :alt="product.nombre" />
      <span v-if="product.destacado" class="badge-destacado">Destacado</span>
    </div>
    
    <div class="product-info">
      <h3>{{ product.nombre }}</h3>
      <p class="product-code">Código: {{ product.codigo }}</p>
      
      <div v-if="product.precio" class="product-price">
        <span class="price">${{ formatPrice(product.precio) }}</span>
      </div>
      
      <button class="btn-add-cart">
        Agregar al pedido
      </button>
    </div>
  </article>
</template>
```

### 5.3 WhatsApp Flotante
```vue
<!-- src/components/common/WhatsAppButton.vue -->
<template>
  <a 
    href="https://wa.me/541100000000" 
    target="_blank"
    class="whatsapp-float"
    aria-label="Contactar por WhatsApp"
  >
    <svg><!-- Ícono WhatsApp --></svg>
  </a>
</template>

<style scoped>
.whatsapp-float {
  position: fixed;
  bottom: 20px;
  right: 20px;
  width: 60px;
  height: 60px;
  background: #25D366;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  z-index: 1000;
  transition: transform 0.3s;
}

.whatsapp-float:hover {
  transform: scale(1.1);
}
</style>
```

## 6. Estructura de Archivos Propuesta

```
src/
├── assets/
│   ├── logo-dulceysalado.png
│   ├── hero-image.jpg
│   └── category-defaults/
├── components/
│   ├── layout/
│   │   ├── AppHeader.vue
│   │   ├── AppFooter.vue
│   │   └── MobileMenu.vue
│   ├── home/
│   │   ├── HeroSection.vue
│   │   ├── CategoryShowcase.vue
│   │   └── FeaturedProducts.vue
│   ├── catalog/
│   │   ├── ProductGrid.vue
│   │   ├── ProductCard.vue
│   │   ├── ProductFilters.vue
│   │   └── CategoryGrid.vue
│   └── common/
│       ├── WhatsAppButton.vue
│       ├── LoadingSpinner.vue
│       └── Pagination.vue
├── config/
│   └── empresa.config.ts
├── services/
│   ├── api.ts (simplificado)
│   └── catalogService.ts
├── stores/
│   └── catalogStore.ts (sin companyStore)
├── styles/
│   ├── main.scss
│   ├── variables.scss
│   └── components/
├── views/
│   ├── HomeView.vue
│   ├── CatalogView.vue
│   ├── ProductDetailView.vue
│   └── ContactView.vue
└── router/
    └── index.ts
```

## 7. Plan de Migración

### Fase 1: Preparación (1-2 días)
1. Crear branch de desarrollo
2. Backup del código actual
3. Configurar nuevos assets (logo, imágenes)

### Fase 2: Simplificación Backend (2-3 días)
1. Eliminar CompanyStore
2. Hardcodear configuración de empresa
3. Simplificar servicios API
4. Eliminar lógica de subdominios

### Fase 3: Rediseño UI (3-4 días)
1. Implementar nueva paleta de colores
2. Actualizar tipografía
3. Crear nuevo header/navegación
4. Implementar hero section
5. Rediseñar cards de productos
6. Agregar botón WhatsApp flotante

### Fase 4: Testing y Ajustes (1-2 días)
1. Testing responsive
2. Optimización de rendimiento
3. Ajustes finales de diseño
4. Preparación para deploy

### Fase 5: Preparación para Nuevas Features (Opcional)
1. Estructura para autenticación
2. Base para sistema de pedidos
3. Preparación para carrito de compras

## 8. Consideraciones Técnicas

### Performance
- Optimizar imágenes locales
- Implementar lazy loading
- Minimizar requests a API
- Cache de categorías

### SEO
- Meta tags específicos para Dulce y Salado
- Structured data para productos
- Sitemap estático
- URLs amigables

### Accesibilidad
- Contraste adecuado con nueva paleta
- Navegación por teclado
- Alt text en imágenes
- ARIA labels donde corresponda

## 9. Métricas de Éxito

- Tiempo de carga < 2 segundos
- Score Lighthouse > 90
- Funcionamiento correcto en móviles
- Navegación intuitiva
- Diseño coherente con la marca

## 10. Próximos Pasos

1. Revisar y aprobar especificación
2. Comenzar con Fase 1 del plan de migración
3. Crear tareas específicas en el sistema de tracking
4. Definir cronograma detallado
5. Iniciar desarrollo

---

**Fecha de creación:** 2025-08-10
**Versión:** 1.0
**Estado:** Pendiente de aprobación