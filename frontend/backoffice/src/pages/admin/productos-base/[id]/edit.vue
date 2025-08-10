<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/admin/productos-base" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Productos Base</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Editar Producto</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Editar Producto Base
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Modifica los datos del producto en el catálogo base
        </p>
      </div>

      <!-- Loading state -->
      <div v-if="loadingProducto" class="flex justify-center py-12">
        <UIcon name="i-heroicons-arrow-path" class="animate-spin h-8 w-8 text-blue-500" />
      </div>

      <!-- Error state -->
      <UAlert
        v-else-if="loadError"
        icon="i-heroicons-exclamation-triangle"
        color="red"
        variant="soft"
        :title="loadError"
        class="mb-6"
      />

      <!-- Sin permisos -->
      <UAlert
        v-else-if="!hasPermission"
        icon="i-heroicons-exclamation-triangle"
        color="orange"
        variant="soft"
        title="Sin permisos"
        description="No tienes permisos para editar productos base"
        class="mb-6"
      />

      <!-- Formulario de edición -->
      <div v-else-if="productoData">
        <ProductosBaseProductoBaseForm
          mode="edit"
          :initial-data="productoData"
          :loading="updating"
          :empresa-id="empresaId"
          @submit="handleSubmit"
          @cancel="handleCancel"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ProductosBaseProductoBaseForm } from '#components'
import type { UpdateProductoBaseCommand, ProductoBaseDto } from '~/types/productos'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default',
  requiresEmpresaPrincipal: true
})

useHead({
  title: 'Editar Producto Base',
  meta: [
    { name: 'description', content: 'Editar un producto en el catálogo base' }
  ]
})

// Composables
const route = useRoute()
const router = useRouter()
const { userPermissions } = useAuth()
const { fetchProducto, updateProducto } = useProductosBase()
const toast = useToast()

// Estado reactivo
const productoId = computed(() => Number(route.params.id))
const loadingProducto = ref(true)
const updating = ref(false)
const loadError = ref<string | null>(null)
const productoData = ref<ProductoBaseDto | null>(null)

// Estado para stock diferencial por empresa
const empresaId = computed(() => {
  const queryEmpresaId = route.query.empresaId as string
  return queryEmpresaId ? parseInt(queryEmpresaId) : null
})

// Computed
const hasPermission = computed(() => {
  return userPermissions.value?.canManageProductosBase || false
})

// Métodos
const loadProducto = async () => {
  if (!productoId.value || isNaN(productoId.value)) {
    loadError.value = 'ID de producto inválido'
    return
  }

  loadingProducto.value = true
  loadError.value = null
  
  try {
    const producto = await fetchProducto(productoId.value)
    productoData.value = producto
    
  } catch (error: any) {
    loadError.value = error.message || 'Error al cargar el producto'
    console.error('Error cargando producto:', error)
  } finally {
    loadingProducto.value = false
  }
}

const handleSubmit = async (formData: UpdateProductoBaseCommand) => {
  if (!productoData.value || !hasPermission.value) {
    toast.add({
      title: 'Error',
      description: 'No tienes permisos para editar este producto',
      color: 'red'
    })
    return
  }

  updating.value = true
  
  try {
    // Incluir empresaId si está presente para stock diferencial
    await updateProducto(productoId.value, formData, empresaId.value || undefined)
    
    // El composable ya maneja el toast de éxito
    // Redirigir a la lista de productos
    await router.push('/admin/productos-base')
    
  } catch (error) {
    // El composable ya maneja el toast de error
    console.error('Error actualizando producto:', error)
  } finally {
    updating.value = false
  }
}

const handleCancel = () => {
  router.push('/admin/productos-base')
}

// Cargar datos al montar
onMounted(() => {
  loadProducto()
})

// Recargar si cambia el ID de la URL
watch(() => route.params.id, () => {
  loadProducto()
})
</script>