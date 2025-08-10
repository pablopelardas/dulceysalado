<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <!-- Header -->
      <div class="mb-8">
        <nav class="flex items-center space-x-2 text-sm text-gray-500 dark:text-gray-400 mb-4">
          <NuxtLink to="/users" class="hover:text-gray-700 dark:hover:text-gray-300 transition-colors">Usuarios</NuxtLink>
          <UIcon name="i-heroicons-chevron-right" class="h-4 w-4" />
          <span class="text-gray-900 dark:text-gray-100">Crear Usuario</span>
        </nav>
        
        <h1 class="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Crear Nuevo Usuario
        </h1>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Completa los datos para crear un nuevo usuario en el sistema
        </p>
      </div>

      <!-- Alerta de información -->
      <UAlert
        v-if="!hasPermission"
        icon="i-heroicons-exclamation-triangle"
        color="warning"
        variant="soft"
        title="Sin permisos"
        description="No tienes permisos para crear usuarios"
        class="mb-6"
      />

      <!-- Formulario de creación -->
      <div v-else>
        <UserForm
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
import UserForm from '~/components/users/UserForm.vue'
import type { UserFormData } from '~/types/users'

// Configuración de página
definePageMeta({
  middleware: ['auth', 'permissions'],
  layout: 'default'
})

useHead({
  title: 'Crear Usuario',
  meta: [
    { name: 'description', content: 'Crear un nuevo usuario en el sistema' }
  ]
})

// Composables
const { user: currentUser } = useAuth()
const { createUser } = useUsers()
const toast = useToast()
const router = useRouter()

// Estado reactivo
const creating = ref(false)

// Computed
const hasPermission = computed(() => {
  return currentUser.value?.puede_gestionar_usuarios || false
})

// Métodos
const handleSubmit = async (formData: UserFormData) => {
  if (!hasPermission.value) {
    toast.add({
      title: 'Error',
      description: 'No tienes permisos para crear usuarios',
      color: 'error'
    })
    return
  }

  creating.value = true
  
  try {
    // Preparar datos para enviar
    const createData = {
      nombre: formData.nombre,
      apellido: formData.apellido,
      email: formData.email,
      password: formData.password,
      rol: formData.rol,
      activo: formData.activo,
      empresa_id: formData.empresa_id, // Incluir empresa_id si está presente
      puede_gestionar_productos_base: formData.puede_gestionar_productos_base,
      puede_gestionar_productos_empresa: formData.puede_gestionar_productos_empresa,
      puede_gestionar_categorias_base: formData.puede_gestionar_categorias_base,
      puede_gestionar_categorias_empresa: formData.puede_gestionar_categorias_empresa,
      puede_gestionar_usuarios: formData.puede_gestionar_usuarios,
      puede_ver_estadisticas: formData.puede_ver_estadisticas
    }

    await createUser(createData)
    
    // El composable ya maneja el toast de éxito
    // Solo redirigir a la lista de usuarios
    await router.push('/users')
    
  } catch (error) {
    // El composable ya maneja el toast de error
    console.error('Error creando usuario:', error)
  } finally {
    creating.value = false
  }
}

const handleCancel = () => {
  router.push('/users')
}

// Verificar permisos al cargar
onMounted(() => {
  if (!hasPermission.value) {
    toast.add({
      title: 'Acceso denegado',
      description: 'No tienes permisos para crear usuarios',
      color: 'warning'
    })
  }
})
</script>