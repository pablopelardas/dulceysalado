<script setup lang="ts">
useHead({
  titleTemplate: (titleChunk) => {
    return titleChunk ? `${titleChunk} - DistriCatalogo Admin Panel` : 'DistriCatalogo Admin Panel';
  }
})

// Configurar color mode dinámico
const colorMode = useColorMode()
// Permitir que el usuario elija su preferencia
// Por defecto será 'system' (detecta automáticamente)
</script>

<template>
  <div>
    <UApp>
      <NuxtLayout>
        <NuxtPage />
      </NuxtLayout>
    </UApp>
  </div>
</template>

<style>
</style>