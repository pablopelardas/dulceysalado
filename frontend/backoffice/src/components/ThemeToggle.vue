<template>
  <ClientOnly>
    <UDropdownMenu :items="themeOptions" :ui="{ content: 'w-48' }">
      <UButton
        variant="ghost"
        color="gray"
        :icon="currentThemeIcon"
        size="sm"
        class="rounded-full p-2 cursor-pointer"
        :title="`Tema actual: ${currentThemeLabel}`"
      />
    </UDropdownMenu>
    
    <template #fallback>
      <UButton
        variant="ghost"
        color="gray"
        icon="i-heroicons-computer-desktop"
        size="sm"
        class="rounded-full p-2"
        title="Tema"
      />
    </template>
  </ClientOnly>
</template>

<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

const colorMode = useColorMode()

// Opciones del menú de temas usando el formato correcto
const themeOptions: DropdownMenuItem[][] = [
  [
    {
      label: 'Claro',
      icon: 'i-heroicons-sun',
      class: 'cursor-pointer',
      onSelect: () => setTheme('light')
    },
    {
      label: 'Oscuro',
      icon: 'i-heroicons-moon',
      class: 'cursor-pointer',
      onSelect: () => setTheme('dark')
    },
    {
      label: 'Sistema',
      icon: 'i-heroicons-computer-desktop',
      class: 'cursor-pointer',
      onSelect: () => setTheme('system')
    }
  ]
]

// Computed para el icono actual
const currentThemeIcon = computed(() => {
  switch (colorMode.preference) {
    case 'light':
      return 'i-heroicons-sun'
    case 'dark':
      return 'i-heroicons-moon'
    case 'system':
    default:
      return 'i-heroicons-computer-desktop'
  }
})

// Computed para el label actual
const currentThemeLabel = computed(() => {
  switch (colorMode.preference) {
    case 'light':
      return 'Claro'
    case 'dark':
      return 'Oscuro'
    case 'system':
    default:
      return 'Sistema'
  }
})

// Función para cambiar tema  
const setTheme = (theme: 'light' | 'dark' | 'system') => {
  colorMode.preference = theme
}

// Debug para colorMode
watch(() => colorMode.preference, (newVal) => {
}, { immediate: true })
</script>