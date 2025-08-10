<template>
  <div class="flex items-center gap-2 mt-1">
    <UBadge 
      color="orange" 
      variant="soft" 
      size="sm"
      class="flex items-center gap-1"
    >
      <UIcon name="i-heroicons-arrow-path" class="h-3 w-3" />
      SYNC
    </UBadge>
    <UTooltip 
      :text="tooltipText"
      :popper="{ placement: 'top' }"
    >
      <UIcon 
        name="i-heroicons-information-circle" 
        class="h-4 w-4 text-orange-500 cursor-help" 
      />
    </UTooltip>
  </div>
</template>

<script setup lang="ts">
interface Props {
  tooltipText?: string
}

const props = withDefaults(defineProps<Props>(), {
  tooltipText: 'Este campo será sobrescrito automáticamente durante la sincronización con Gecom. Los cambios manuales no se conservarán.'
})
</script>