<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="flex justify-between items-center mb-8">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
            Gestión de Usuarios
          </h1>
          <p class="mt-2 text-gray-600 dark:text-gray-400">
            Administra los usuarios del sistema
          </p>
        </div>
        
        <ClientOnly>
          <UButton
            v-if="userPermissions.canManageUsuarios"
            to="/users/create"
            color="primary"
            size="lg"
          >
            <UIcon name="i-heroicons-plus" class="mr-2" />
            Nuevo Usuario
          </UButton>
          <template #fallback>
            <USkeleton class="h-11 w-36 rounded" />
          </template>
        </ClientOnly>
      </div>

      <!-- Filtros -->
      <UCard class="mb-6">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <!-- Búsqueda -->
          <UFormField label="Buscar">
            <UInput
              v-model="searchQuery"
              placeholder="Nombre, apellido o email..."
              icon="i-heroicons-magnifying-glass"
              @input="debouncedSearch"
            />
          </UFormField>

          <!-- Filtro por rol -->
          <UFormField label="Rol">
            <USelectMenu
              v-model="roleFilter"
              :items="roleOptions"
              value-key="value"
              placeholder="Todos los roles"
              @change="applyFilters"
            />
          </UFormField>

        </div>

        <!-- Botones de acción -->
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
            {{ pagination.total }} usuario{{ pagination.total !== 1 ? 's' : '' }} encontrado{{ pagination.total !== 1 ? 's' : '' }}
          </div>
        </div>
      </UCard>

      <!-- Tabla de usuarios -->
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
                <!-- Nombre y email -->
                <div class="flex-1">
                  <USkeleton class="h-4 w-32 mb-1" />
                  <USkeleton class="h-3 w-48" />
                </div>
                
                <!-- Rol -->
                <div class="w-20">
                  <USkeleton class="h-6 w-16 rounded-full" />
                </div>
                
                <!-- Estado -->
                <div class="w-16">
                  <USkeleton class="h-6 w-14 rounded-full" />
                </div>
                
                
                <!-- Último login -->
                <div class="w-24">
                  <USkeleton class="h-4 w-20 mb-1" />
                  <USkeleton class="h-3 w-16" />
                </div>
                
                <!-- Permisos -->
                <div class="w-32">
                  <div class="flex space-x-1">
                    <USkeleton class="h-5 w-6 rounded" />
                    <USkeleton class="h-5 w-6 rounded" />
                    <USkeleton class="h-5 w-6 rounded" />
                  </div>
                </div>
                
                <!-- Acciones -->
                <div class="w-24">
                  <div class="flex space-x-1">
                    <USkeleton class="h-8 w-8 rounded" />
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
          <UButton
            color="error"
            variant="ghost"
            class="mt-2"
            @click="fetchUsers()"
          >
            Reintentar
          </UButton>
        </div>

        <div v-else-if="users.length === 0 && !initialLoading" class="text-center py-8">
          <UIcon name="i-heroicons-users" class="h-12 w-12 text-gray-400 mx-auto mb-2" />
          <p class="text-gray-600 dark:text-gray-400">No se encontraron usuarios</p>
          <ClientOnly>
            <UButton
              v-if="userPermissions.canManageUsuarios"
              to="/users/create"
              color="primary"
              variant="ghost"
              class="mt-2"
            >
              Crear el primer usuario
            </UButton>
            <template #fallback>
              <USkeleton class="h-9 w-40 mx-auto mt-2 rounded" />
            </template>
          </ClientOnly>
        </div>

        <ClientOnly>
          <UsersTable
            v-if="!loading && !initialLoading && !error && users.length > 0"
            :users="users"
            :loading="false"
            @edit="editUser"
            @delete="deleteUser"
            @change-password="changePassword"
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
  <UModal :open="showDeleteModal" :overlay="false" title="Modal without overlay">
      <template #header>
        <div class="flex items-center">
          <UIcon name="i-heroicons-exclamation-triangle" class="h-6 w-6 text-red-500 mr-2" />
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">Confirmar eliminación</h3>
        </div>
      </template>
    <template #body>
        <p class="text-gray-600 dark:text-gray-400">
          ¿Estás seguro de que deseas desactivar al usuario 
          <strong class="text-gray-900 dark:text-gray-100">{{ userToDelete?.nombre }} {{ userToDelete?.apellido }}</strong>?
          Esta acción se puede revertir posteriormente.
        </p>
    </template>
    <template #footer>
        <div class="flex justify-end space-x-3">
          <UButton
            variant="ghost"
            color="neutral"
            @click="showDeleteModal = false"
          >
            Cancelar
          </UButton>
          <UButton
            color="error"
            :loading="loading"
            @click="confirmDelete"
          >
            Desactivar Usuario
          </UButton>
        </div>
      </template>
  </UModal>

  <!-- Modal de cambio de contraseña -->
  <ChangePasswordModal
    v-model="showChangePasswordModal"
    :user="userToChangePassword"
    @success="handlePasswordChangeSuccess"
  />
</template>

<script setup lang="ts">
import type { Usuario } from '~/types/auth'
import ChangePasswordModal from '~/components/users/ChangePasswordModal.vue'


// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Gestión de Usuarios',
  meta: [
    { name: 'description', content: 'Administra los usuarios del sistema' }
  ]
})

// Composables
const { users, loading, error, pagination, fetchUsers, deleteUser: deleteUserAction, changePage: changePageAction, applyFilters: applyFiltersAction, clearFilters } = useUsers()
const { userPermissions, user: currentUser } = useAuth()
const api = useApi()

// Estado reactivo
const searchQuery = ref('')
const roleFilter = ref<'admin' | 'editor' | 'viewer' | 'all'>('all')
const currentPage = ref(1)
const initialLoading = ref(true) // Para distinguir carga inicial de carga de datos

// Modales
const showDeleteModal = ref(false)
const userToDelete = ref<Usuario | null>(null)

// Opciones para selects
const roleOptions = [
  { label: 'Todos los roles', value: 'all' },
  { label: 'Administrador', value: 'admin' },
  { label: 'Editor', value: 'editor' },
  { label: 'Viewer', value: 'viewer' }
]



// Importar composable para debounce

// Búsqueda debounced
const debouncedSearch = () => {
  applyFilters()
}

// Métodos
const applyFilters = async () => {
  await applyFiltersAction({
    search: searchQuery.value,
    rol: (roleFilter.value && roleFilter.value !== 'all') ? roleFilter.value : undefined,
    page: 1
  })
  currentPage.value = 1
}

const clearAllFilters = async () => {
  searchQuery.value = ''
  roleFilter.value = 'all'
  currentPage.value = 1
  await clearFilters()
}

const changePage = async (page: number) => {
  currentPage.value = page
  await changePageAction(page)
}

const editUser = (user: Usuario) => {
  navigateTo(`/users/${user.id}/edit`)
}

const deleteUser = (user: Usuario) => {
  userToDelete.value = user
  showDeleteModal.value = true
}

const confirmDelete = async () => {
  if (userToDelete.value) {
    try {
      await deleteUserAction(userToDelete.value.id)
      showDeleteModal.value = false
      userToDelete.value = null
    } catch (error) {
      // Error ya manejado en el composable
    }
  }
}

// Estado para modal de cambio de contraseña
const showChangePasswordModal = ref(false)
const userToChangePassword = ref<Usuario | null>(null)

const changePassword = (user: Usuario) => {
  userToChangePassword.value = user
  showChangePasswordModal.value = true
}

const handlePasswordChangeSuccess = () => {
  // Refrescar la lista de usuarios después del cambio exitoso
  fetchUsers()
}

// Debug watcher para el modal
watch(showChangePasswordModal, (newValue) => {
})

// Cargar datos iniciales
onMounted(async () => {
  try {
    await fetchUsers()
  } finally {
    // Marcar que la carga inicial ha terminado
    initialLoading.value = false
  }
})

</script>