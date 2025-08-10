<script setup lang="ts">
const route = useRoute()

useHead({
  meta: [{ property: 'og:title', content: `App Name - ${route.meta.title}` }]
})
</script>

<template>
  <div class="flex flex-col min-h-screen bg-gray-100 dark:bg-gray-900 transition-colors duration-200">
    <AppHeader />
    <main class="flex-1 container mx-auto px-4 py-6">
      <slot />
    </main>
    <AppFooter />
  </div>
</template>
