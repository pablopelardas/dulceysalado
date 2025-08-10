<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/users" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Usuarios</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Editar Usuario</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Editar Usuario
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Modifica los datos del usuario en el sistema
        </p>
      </div>

      <!-- Loading state -->
      <div v-if="loadingUser" class="flex justify-center py-12">
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
        description="No tienes permisos para editar este usuario"
        class="mb-6"
      />

      <!-- Formulario de edición -->
      <div v-else-if="userData">
        <UserForm
          mode="edit"
          :initial-data="userData"
          :loading="updating"
          @submit="handleSubmit"
          @cancel="handleCancel"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UserForm from '~/components/users/UserForm.vue'
import type { UserFormData } from '~/types/users'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Editar Usuario',
  meta: [
    { name: 'description', content: 'Editar un usuario en el sistema' }
  ]
})

// Composables
const route = useRoute()
const router = useRouter()
const { user: currentUser } = useAuth()
const { fetchUser, updateUser, canEditUser } = useUsers()
const toast = useToast()

// Estado reactivo
const userId = computed(() => Number(route.params.id))
const loadingUser = ref(true)
const updating = ref(false)
const loadError = ref<string | null>(null)
const userData = ref<UserFormData | null>(null)

// Computed
const hasPermission = computed(() => {
  if (!userData.value) return false
  return canEditUser(userData.value as any) // Casting temporal para compatibilidad
})

// Métodos
const loadUser = async () => {
  if (!userId.value || isNaN(userId.value)) {
    loadError.value = 'ID de usuario inválido'
    return
  }

  loadingUser.value = true
  loadError.value = null
  
  try {
    const user = await fetchUser(userId.value)
    
    // Convertir datos del usuario al formato del formulario
    userData.value = {
      nombre: user.nombre,
      apellido: user.apellido,
      email: user.email,
      password: '', // No se pre-llena por seguridad
      confirmPassword: '', // No se pre-llena por seguridad
      rol: user.rol,
      activo: user.activo,
      empresa_id: user.empresa_id,
      puede_gestionar_productos_base: user.puede_gestionar_productos_base || false,
      puede_gestionar_productos_empresa: user.puede_gestionar_productos_empresa || false,
      puede_gestionar_categorias_base: user.puede_gestionar_categorias_base || false,
      puede_gestionar_categorias_empresa: user.puede_gestionar_categorias_empresa || false,
      puede_gestionar_usuarios: user.puede_gestionar_usuarios || false,
      puede_ver_estadisticas: user.puede_ver_estadisticas || false
    }
    
  } catch (error: any) {
    loadError.value = error.message || 'Error al cargar el usuario'
    console.error('Error cargando usuario:', error)
  } finally {
    loadingUser.value = false
  }
}

const handleSubmit = async (formData: UserFormData) => {
  if (!userData.value || !hasPermission.value) {
    toast.add({
      title: 'Error',
      description: 'No tienes permisos para editar este usuario',
      color: 'error'
    })
    return
  }

  updating.value = true
  
  try {
    // Preparar datos para enviar (sin contraseña y sin activo para edición)
    const updateData = {
      nombre: formData.nombre,
      apellido: formData.apellido,
      email: formData.email,
      rol: formData.rol,
      empresa_id: formData.empresa_id,
      puede_gestionar_productos_base: formData.puede_gestionar_productos_base,
      puede_gestionar_productos_empresa: formData.puede_gestionar_productos_empresa,
      puede_gestionar_categorias_base: formData.puede_gestionar_categorias_base,
      puede_gestionar_categorias_empresa: formData.puede_gestionar_categorias_empresa,
      puede_gestionar_usuarios: formData.puede_gestionar_usuarios,
      puede_ver_estadisticas: formData.puede_ver_estadisticas
    }

    await updateUser(userId.value, updateData)
    
    // El composable ya maneja el toast de éxito
    // Redirigir a la lista de usuarios
    await router.push('/users')
    
  } catch (error) {
    // El composable ya maneja el toast de error
    console.error('Error actualizando usuario:', error)
  } finally {
    updating.value = false
  }
}

const handleCancel = () => {
  router.push('/users')
}

// Cargar datos al montar
onMounted(() => {
  loadUser()
})

// Recargar si cambia el ID de la URL
watch(() => route.params.id, () => {
  loadUser()
})
</script>