<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/admin/empresas-cliente" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">
            Empresas Cliente
          </NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Editar Empresa</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Editar Empresa Cliente
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Modifica los datos de la empresa cliente en el sistema
        </p>
      </div>

      <!-- Loading state -->
      <div v-if="loadingCompany" class="flex justify-center py-12">
        <UIcon name="i-heroicons-arrow-path" class="animate-spin h-8 w-8 text-blue-500" />
      </div>

      <!-- Error state -->
      <UAlert
        v-else-if="loadError"
        icon="i-heroicons-exclamation-triangle"
        color="error"
        variant="soft"
        :title="loadError"
        class="mb-6"
      />

      <!-- Sin permisos -->
      <UAlert
        v-else-if="!hasPermission"
        icon="i-heroicons-exclamation-triangle"
        color="warning"
        variant="soft"
        title="Sin permisos"
        description="Solo la empresa principal puede editar empresas cliente"
        class="mb-6"
      />

      <!-- Formulario de edición -->
      <div v-else-if="companyData">
        <CompanyForm
          mode="edit"
          :initial-data="companyData"
          :loading="updating"
          @submit="handleSubmit"
          @cancel="handleCancel"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import CompanyForm from '~/components/empresas-cliente/CompanyForm.vue'
import type { CreateCompanyRequest, UpdateCompanyRequest } from '~/types/auth'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Editar Empresa Cliente',
  meta: [
    { name: 'description', content: 'Editar una empresa cliente en el sistema' }
  ]
})

// Composables
const route = useRoute()
const router = useRouter()
const { isEmpresaPrincipal } = useAuth()
const { fetchCompany, updateCompany, canEditCompany } = useCompanies()
const toast = useToast()

// Estado reactivo
const companyId = computed(() => Number(route.params.id))
const loadingCompany = ref(true)
const updating = ref(false)
const loadError = ref<string | null>(null)
const companyData = ref<CreateCompanyRequest | null>(null)

// Computed
const hasPermission = computed(() => {
  return isEmpresaPrincipal.value && companyData.value
})

// Métodos
const loadCompany = async () => {
  if (!companyId.value || isNaN(companyId.value)) {
    loadError.value = 'ID de empresa inválido'
    return
  }

  loadingCompany.value = true
  loadError.value = null
  
  try {
    const company = await fetchCompany(companyId.value)
    
    // Convertir datos de la empresa al formato del formulario
    companyData.value = {
      codigo: company.codigo,
      nombre: company.nombre,
      razon_social: company.razon_social || '',
      cuit: company.cuit || '',
      telefono: company.telefono || '',
      email: company.email || '',
      direccion: company.direccion || '',
      dominio_personalizado: company.dominio_personalizado,
      fecha_vencimiento: company.fecha_vencimiento || '',
      logo_url: company.logo_url || '',
      favicon_url: company.favicon_url || '',
      colores_tema: company.colores_tema || {
        primario: '#007bff',
        secundario: '#6c757d',
        acento: '#28a745'
      },
      puede_agregar_productos: company.puede_agregar_productos,
      puede_agregar_categorias: company.puede_agregar_categorias,
      mostrar_precios: company.mostrar_precios,
      mostrar_stock: company.mostrar_stock,
      permitir_pedidos: company.permitir_pedidos,
      productos_por_pagina: company.productos_por_pagina,
      url_whatsapp: company.url_whatsapp || '',
      url_facebook: company.url_facebook || '',
      url_instagram: company.url_instagram || '',
      lista_precio_predeterminada_id: company.lista_precio_predeterminada_id || null,
      requesting_user_id: 0 // Se actualiza en el submit
    }
    
  } catch (error: any) {
    loadError.value = error.message || 'Error al cargar la empresa'
    console.error('Error cargando empresa:', error)
  } finally {
    loadingCompany.value = false
  }
}

const handleSubmit = async (formData: CreateCompanyRequest) => {
  if (!companyData.value || !hasPermission.value) {
    toast.add({
      title: 'Error',
      description: 'Solo la empresa principal puede editar empresas cliente',
      color: 'error'
    })
    return
  }

  updating.value = true
  
  try {
    // Preparar datos para actualización
    const updateData: UpdateCompanyRequest = {
      codigo: formData.codigo,
      nombre: formData.nombre,
      razon_social: formData.razon_social,
      cuit: formData.cuit,
      telefono: formData.telefono,
      email: formData.email,
      direccion: formData.direccion,
      dominio_personalizado: formData.dominio_personalizado,
      fecha_vencimiento: formData.fecha_vencimiento,
      logo_url: formData.logo_url,
      favicon_url: formData.favicon_url,
      colores_tema: formData.colores_tema,
      puede_agregar_productos: formData.puede_agregar_productos,
      puede_agregar_categorias: formData.puede_agregar_categorias,
      mostrar_precios: formData.mostrar_precios,
      mostrar_stock: formData.mostrar_stock,
      permitir_pedidos: formData.permitir_pedidos,
      productos_por_pagina: formData.productos_por_pagina,
      url_whatsapp: formData.url_whatsapp,
      url_facebook: formData.url_facebook,
      url_instagram: formData.url_instagram,
      lista_precio_predeterminada_id: formData.lista_precio_predeterminada_id,
      company_id: companyId.value,
      requesting_user_id: 0 // Se actualiza en el composable
    }

    await updateCompany(companyId.value, updateData)
    
    // El composable ya maneja el toast de éxito
    // Redirigir a la lista de empresas
    await router.push('/admin/empresas-cliente')
    
  } catch (error) {
    // El composable ya maneja el toast de error
    console.error('Error actualizando empresa:', error)
  } finally {
    updating.value = false
  }
}

const handleCancel = () => {
  router.push('/admin/empresas-cliente')
}

// Cargar datos al montar
onMounted(() => {
  loadCompany()
})

// Recargar si cambia el ID de la URL
watch(() => route.params.id, () => {
  loadCompany()
})
</script>