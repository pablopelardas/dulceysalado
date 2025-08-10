<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="flex justify-between items-center mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
            Gestión de Empresas Cliente
          </h1>
          <p class="mt-2 text-gray-600 dark:text-gray-400">
            Administra las empresas cliente del sistema
          </p>
        </div>
        <UButton color="primary" size="lg" @click="navigateTo('/admin/empresas-cliente/create')">
          <UIcon name="i-heroicons-plus" class="mr-2" />
          Nueva Empresa
        </UButton>
      </div>

      <!-- Filtros -->
      <UCard class="mb-6">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <!-- Búsqueda -->
          <UFormField label="Buscar">
            <UInput
              v-model="searchQuery"
              placeholder="Nombre o código..."
              icon="i-heroicons-magnifying-glass"
              @input="debouncedSearch"
            />
          </UFormField>

          <!-- Espacio para futuros filtros -->
          <div></div>
         
        </div>
        <div class="flex justify-between items-center mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
          <UButton
            variant="ghost"
            class="cursor-pointer"
            color="blue"
            @click="clearAllFilters"
          >
            Limpiar Filtros
          </UButton>
          <div class="text-sm text-gray-500 dark:text-gray-400">
            {{ pagination.total }} empresa{{ pagination.total !== 1 ? 's' : '' }} encontrada{{ pagination.total !== 1 ? 's' : '' }}
          </div>
        </div>
      </UCard>

      <!-- Tabla de empresas cliente -->
      <UCard>
        <div v-if="loading || initialLoading" class="space-y-4">
          <!-- Header skeleton -->
          <div class="flex justify-between items-center p-4 border-b border-gray-200 dark:border-gray-700">
            <div class="flex space-x-4">
              <USkeleton class="h-4 w-20" />
              <USkeleton class="h-4 w-16" />
              <USkeleton class="h-4 w-24" />
              <USkeleton class="h-4 w-32" />
              <USkeleton class="h-4 w-20" />
              <USkeleton class="h-4 w-28" />
              <USkeleton class="h-4 w-24" />
            </div>
          </div>
          
          <!-- Rows skeleton -->
          <div class="space-y-3 p-4">
            <div v-for="n in 5" :key="n" class="flex justify-between items-center py-3 border-b border-gray-100 dark:border-gray-800">
              <div class="flex items-center space-x-4 flex-1">
                <!-- Código y nombre -->
                <div class="flex-1">
                  <USkeleton class="h-4 w-20 mb-1" />
                  <USkeleton class="h-5 w-48" />
                </div>
                
                <!-- Razón social -->
                <div class="w-32">
                  <USkeleton class="h-4 w-28" />
                </div>
                
                <!-- Estado -->
                <div class="w-16">
                  <USkeleton class="h-6 w-14 rounded-full" />
                </div>
                
                <!-- Vencimiento -->
                <div class="w-24">
                  <USkeleton class="h-4 w-20 mb-1" />
                  <USkeleton class="h-3 w-16" />
                </div>
                
                <!-- Permisos -->
                <div class="w-32">
                  <div class="flex space-x-1">
                    <USkeleton class="h-5 w-6 rounded" />
                    <USkeleton class="h-5 w-6 rounded" />
                  </div>
                </div>
                
                <!-- Acciones -->
                <div class="w-24">
                  <div class="flex space-x-1">
                    <USkeleton class="h-8 w-8 rounded" />
                    <USkeleton class="h-8 w-8 rounded" />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div v-else-if="error && !initialLoading" class="text-center py-8">
          <UIcon name="i-heroicons-exclamation-triangle" class="h-12 w-12 text-red-500 mx-auto mb-2" />
          <p class="text-red-600 dark:text-red-400">{{ error }}</p>
          <UButton color="error" variant="ghost" class="mt-2" @click="fetchCompanies()">
            Reintentar
          </UButton>
        </div>
        <div v-else-if="companies.length === 0 && !initialLoading" class="text-center py-8">
          <UIcon name="i-heroicons-building-office" class="h-12 w-12 text-gray-400 mx-auto mb-2" />
          <p class="text-gray-600 dark:text-gray-400">No se encontraron empresas cliente</p>
          <UButton color="primary" variant="ghost" class="mt-2" @click="navigateTo('/admin/empresas-cliente/create')">
            Crear la primera empresa
          </UButton>
        </div>
        <!-- Tabla de empresas -->
        <ClientOnly>
          <EmpresasClienteTable
            v-if="!loading && !initialLoading && !error && companies.length > 0"
            :empresas="companies as Empresa[]"
            :loading="false"
            @edit="editEmpresa"
            @delete="deleteEmpresa"
          />
        </ClientOnly>
        <!-- Paginación -->
        <div v-if="pagination.pages > 1" class="flex justify-center mt-6">
          <UPagination
            v-model="currentPage"
            :page-count="pagination.pages"
            :total="pagination.total"
            @update:model-value="changePage"
          />
        </div>
      </UCard>
    </div>
  </div>
</template>

<script setup lang="ts">
import EmpresasClienteTable from '~/components/empresas-cliente/EmpresasClienteTable.vue'
import type { Empresa } from '~/types/auth'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Gestión de Empresas Cliente',
  meta: [
    { name: 'description', content: 'Administra las empresas cliente del sistema' }
  ]
})

// Composables
const { 
  companies,
  loading, 
  error, 
  pagination, 
  fetchCompanies, 
  deleteCompany: deleteCompanyAction, 
  changePage: changePageAction, 
  applyFilters: applyFiltersAction, 
  clearFilters 
} = useCompanies()

// Estado reactivo
const searchQuery = ref('')
const currentPage = ref(1)
const initialLoading = ref(true)

// Métodos
const applyFilters = async () => {
  await applyFiltersAction({
    search: searchQuery.value,
    page: 1
  })
  currentPage.value = 1
}

const clearAllFilters = async () => {
  searchQuery.value = ''
  currentPage.value = 1
  await clearFilters()
}

const changePage = async (page: number) => {
  currentPage.value = page
  await changePageAction(page)
}

const editEmpresa = (empresa: Empresa) => {
  navigateTo(`/admin/empresas-cliente/${empresa.id}/edit`)
}

const deleteEmpresa = async (empresa: Empresa) => {
  const confirmed = confirm(`¿Estás seguro de que quieres desactivar la empresa "${empresa.nombre}"?`)
  
  if (!confirmed) {
    return
  }
  
  try {
    await deleteCompanyAction(empresa.id)
  } catch (error) {
    // Error ya manejado en el composable
  }
}

// Búsqueda debounced
const debouncedSearch = () => {
  applyFilters()
}

// Cargar datos iniciales
onMounted(async () => {
  try {
    await fetchCompanies()
  } finally {
    initialLoading.value = false
  }
})
</script>
