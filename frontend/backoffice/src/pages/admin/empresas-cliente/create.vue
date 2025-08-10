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
          <span class="text-gray-900 dark:text-gray-100">Crear Empresa</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Crear Nueva Empresa Cliente
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Completa los datos para crear una nueva empresa cliente en el sistema
        </p>
      </div>

      <!-- Alerta de información -->
      <UAlert
        v-if="!hasPermission"
        icon="i-heroicons-exclamation-triangle"
        color="warning"
        variant="soft"
        title="Sin permisos"
        description="Solo la empresa principal puede crear empresas cliente"
        class="mb-6"
      />

      <!-- Formulario de creación -->
      <div v-else>
        <CompanyForm
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
import CompanyForm from '~/components/empresas-cliente/CompanyForm.vue'
import type { CreateCompanyRequest } from '~/types/auth'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Crear Empresa Cliente',
  meta: [
    { name: 'description', content: 'Crear una nueva empresa cliente en el sistema' }
  ]
})

// Composables
const { isEmpresaPrincipal } = useAuth()
const { createCompany } = useCompanies()
const toast = useToast()
const router = useRouter()

// Estado reactivo
const creating = ref(false)

// Computed
const hasPermission = computed(() => {
  return isEmpresaPrincipal.value
})

// Métodos
const handleSubmit = async (formData: CreateCompanyRequest) => {
  if (!hasPermission.value) {
    toast.add({
      title: 'Error',
      description: 'Solo la empresa principal puede crear empresas cliente',
      color: 'error'
    })
    return
  }

  creating.value = true
  
  try {
    await createCompany(formData)
    
    // El composable ya maneja el toast de éxito
    await router.push('/admin/empresas-cliente')
  } catch (error) {
    // El composable ya maneja el toast de error
    console.error('Error creando empresa:', error)
  } finally {
    creating.value = false
  }
}

const handleCancel = () => {
  router.push('/admin/empresas-cliente')
}

// Verificar permisos al cargar
onMounted(() => {
  if (!hasPermission.value) {
    toast.add({
      title: 'Acceso denegado',
      description: 'Solo la empresa principal puede crear empresas cliente',
      color: 'warning'
    })
  }
})
</script>