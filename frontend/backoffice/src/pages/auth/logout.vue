<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50">
    <div class="max-w-md w-full text-center">
      <div class="mx-auto h-12 w-12 flex items-center justify-center rounded-full bg-red-100 mb-4">
        <UIcon name="i-heroicons-arrow-right-on-rectangle" class="h-6 w-6 text-red-600" />
      </div>
      
      <h2 class="text-2xl font-bold text-gray-900 mb-2">
        Cerrando sesión...
      </h2>
      
      <p class="text-gray-600 mb-6">
        Tu sesión se está cerrando de forma segura
      </p>
      
      <UButton 
        to="/login"
        icon="i-heroicons-arrow-left"
        variant="outline"
      >
        Volver al login
      </UButton>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: false
})

// Ejecutar logout al montar el componente
const { logout } = useAuth()

onMounted(() => {
  logout()
})
</script>