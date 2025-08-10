<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Dashboard</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Configuración</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Configuración de Empresa
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Personaliza la información y apariencia de tu empresa
        </p>
      </div>

      <!-- Estado de carga -->
      <div v-if="loadingCompany" class="flex justify-center py-12">
        <UIcon name="i-heroicons-arrow-path" class="animate-spin h-8 w-8 text-blue-500" />
      </div>

      <!-- Error al cargar -->
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
        v-else-if="!canEdit"
        icon="i-heroicons-exclamation-triangle"
        color="warning"
        variant="soft"
        title="Sin permisos"
        description="No tienes permisos para editar la configuración de la empresa"
        class="mb-6"
      />

      <!-- Formulario de configuración -->
      <div v-else-if="companyData">
        <CompanyClientForm
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
import CompanyClientForm from '~/components/empresas-cliente/CompanyClientForm.vue'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Configuración de Empresa',
  meta: [
    { name: 'description', content: 'Configuración de empresa cliente' }
  ]
})

// Composables
const router = useRouter()
const { user, empresa } = useAuth()
const { updateCompany } = useCompanies()
const api = useApi()
const toast = useToast()

// Estado reactivo
const loadingCompany = ref(true)
const updating = ref(false)
const loadError = ref<string | null>(null)
const companyData = ref<any>(null)

// Computed
const canEdit = computed(() => {
  // Tanto empresa principal como empresas cliente pueden editar su configuración
  return empresa.value !== null
})

// Métodos
const loadCompanyData = async () => {
  if (!empresa.value?.id) {
    loadError.value = 'No se encontró información de la empresa'
    return
  }

  loadingCompany.value = true
  loadError.value = null
  
  try {
    const response = await api.get(`/api/Companies/${empresa.value.id}`)
    companyData.value = response
    
  } catch (error: any) {
    loadError.value = error.message || 'Error al cargar los datos de la empresa'
    console.error('Error cargando empresa:', error)
  } finally {
    loadingCompany.value = false
  }
}

const handleSubmit = async (formData: any) => {
  if (!empresa.value?.id || !canEdit.value) {
    toast.add({
      title: 'Error',
      description: 'No tienes permisos para editar esta empresa',
      color: 'error'
    })
    return
  }

  updating.value = true
  
  try {
    // Preparar datos para enviar (solo campos editables)
    const updateData = {
      company_id: empresa.value.id,
      nombre: formData.nombre,
      email: formData.email,
      telefono: formData.telefono,
      direccion: formData.direccion,
      logo_url: formData.logo_url,
      favicon_url: formData.favicon_url,
      colores_tema: formData.colores_tema,
      productos_por_pagina: formData.productos_por_pagina,
      url_whatsapp: formData.url_whatsapp,
      url_facebook: formData.url_facebook,
      url_instagram: formData.url_instagram,
      lista_precio_predeterminada_id: formData.lista_precio_predeterminada_id,
      requesting_user_id: user.value?.id || 0
    }

    await updateCompany(empresa.value.id, updateData)
    
    toast.add({
      title: 'Configuración actualizada',
      description: 'La configuración de tu empresa ha sido actualizada exitosamente',
      color: 'success'
    })
    
    // Recargar datos actualizados
    await loadCompanyData()
    
  } catch (error: any) {
    console.error('Error actualizando empresa:', error)
    toast.add({
      title: 'Error al actualizar',
      description: error.message || 'No se pudo actualizar la configuración',
      color: 'error'
    })
  } finally {
    updating.value = false
  }
}

const handleCancel = () => {
  router.push('/')
}

// Cargar datos al montar
onMounted(async () => {
  // Verificar permisos antes de cargar
  if (canEdit.value) {
    await loadCompanyData()
  } else {
    loadingCompany.value = false
  }
})
</script>