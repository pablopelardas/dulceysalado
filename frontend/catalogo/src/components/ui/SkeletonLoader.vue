<template>
  <div 
    :class="[
      'animate-pulse bg-white/20 rounded-lg',
      customClass
    ]"
    :style="{ width, height }"
  >
    <slot />
  </div>
</template>

<script setup lang="ts">
interface Props {
  width?: string
  height?: string
  customClass?: string
}

withDefaults(defineProps<Props>(), {
  width: '100%',
  height: '1rem',
  customClass: ''
})
</script>