<script setup lang="ts">
import { onMounted } from 'vue'
import { RouterView } from 'vue-router'
import { applyTheme } from '@/utils/theme'

onMounted(() => {
  // Aplicar tema de la empresa
  applyTheme()
})
</script>

<template>
  <!-- Main App Content -->
  <router-view v-slot="{ Component }">
    <transition name="page" mode="out-in">
      <component :is="Component" />
    </transition>
  </router-view>
</template>

<style scoped>
/* Page Transitions */
.page-enter-active,
.page-leave-active {
  transition: all 0.3s ease;
}

.page-enter-from,
.page-leave-to {
  opacity: 0;
  transform: translateY(20px);
}
</style>
