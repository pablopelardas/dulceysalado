<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/admin/productos-base" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Productos Base</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Crear Producto</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Crear Nuevo Producto Base
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Completa los datos para crear un nuevo producto en el catálogo base
        </p>
      </div>

      <!-- Alerta de información -->
      <UAlert
        v-if="!hasPermission"
        icon="i-heroicons-exclamation-triangle"
        color="orange"
        variant="soft"
        title="Sin permisos"
        description="No tienes permisos para crear productos base"
        class="mb-6"
      />

      <!-- Formulario de creación -->
      <div v-else>
        <ProductosBaseProductoBaseForm
          mode="create"
          :loading="creating"
          @submit="handleSubmit"
          @cancel="handleCancel"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { CreateProductoBaseCommand } from '~/types/productos'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default',
  requiresEmpresaPrincipal: true
})

useHead({
  title: 'Crear Producto Base',
  meta: [
    { name: 'description', content: 'Crear un nuevo producto en el catálogo base' }
  ]
})

// Composables
const { userPermissions } = useAuth()
const { createProducto } = useProductosBase()
const { upsertPrecio } = useProductoPrecios()
const toast = useToast()
const router = useRouter()

// Estado reactivo
const creating = ref(false)

// Computed
const hasPermission = computed(() => {
  return userPermissions.value?.canManageProductosBase || false
})

// Métodos
const handleSubmit = async (formData: any) => {
  if (!hasPermission.value) {
    toast.add({
      title: 'Error',
      description: 'No tienes permisos para crear productos base',
      color: 'red'
    })
    return
  }

  creating.value = true
  
  try {
    // Extraer precios iniciales
    const { _preciosIniciales, ...productoData } = formData
    
    // Crear el producto
    const resultado = await createProducto(productoData as CreateProductoBaseCommand)
    
    // Si se creó exitosamente y hay precios iniciales, guardarlos
    if (resultado && resultado.id && _preciosIniciales) {
      const preciosPromises = Object.entries(_preciosIniciales)
        .filter(([_, precio]) => precio && precio > 0)
        .map(([listaId, precio]) => 
          upsertPrecio(resultado.id, Number(listaId), Number(precio), 'base')
        )
      
      if (preciosPromises.length > 0) {
        try {
          await Promise.all(preciosPromises)
          toast.add({
            title: 'Precios guardados',
            description: 'Los precios iniciales se han guardado correctamente',
            color: 'green'
          })
        } catch (precioError) {
          console.error('Error al guardar precios:', precioError)
          toast.add({
            title: 'Advertencia',
            description: 'El producto se creó pero algunos precios no se pudieron guardar',
            color: 'orange'
          })
        }
      }
    }
    
    // Redirigir a la lista de productos
    await router.push('/admin/productos-base')
    
  } catch (error) {
    // El composable ya maneja el toast de error
    console.error('Error creando producto:', error)
  } finally {
    creating.value = false
  }
}

const handleCancel = () => {
  router.push('/admin/productos-base')
}

// Verificar permisos al cargar
onMounted(() => {
  if (!hasPermission.value) {
    toast.add({
      title: 'Acceso denegado',
      description: 'No tienes permisos para crear productos base',
      color: 'orange'
    })
  }
})
</script>