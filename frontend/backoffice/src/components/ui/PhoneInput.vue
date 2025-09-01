<template>
  <div class="relative">
    <label v-if="label" :for="inputId" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
      {{ label }} {{ required ? '*' : '' }}
    </label>
    
    <div class="relative flex">
      <!-- Prefijo +54 -->
      <div class="inline-flex items-center px-3 rounded-l-lg border border-r-0 border-gray-300 dark:border-gray-600 bg-gray-50 dark:bg-gray-800 text-gray-700 dark:text-gray-300 text-sm font-medium">
        +54
      </div>
      
      <!-- Input del teléfono -->
      <UInput
        :id="inputId"
        :model-value="internalValue"
        @update:model-value="handleInput"
        type="tel"
        :required="required"
        :disabled="disabled"
        :placeholder="placeholder"
        class="flex-1 rounded-l-none"
      />
    </div>
    
    <p v-if="helpText" class="mt-1 text-xs text-gray-500 dark:text-gray-400">
      {{ helpText }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  modelValue?: string
  label?: string
  placeholder?: string
  required?: boolean
  disabled?: boolean
  helpText?: string
  id?: string
}

interface Emits {
  (e: 'update:modelValue', value: string): void
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  placeholder: '11 1234-5678',
  helpText: 'Formato: 11 1234-5678 (sin el +54)',
  id: undefined
})

const emit = defineEmits<Emits>()

// ID único para el input
const inputId = computed(() => props.id || `phone-${Math.random().toString(36).substr(2, 9)}`)

// Valor interno (sin el +54)
const internalValue = computed(() => {
  if (!props.modelValue) return ''
  
  // Si el valor ya tiene +54, lo removemos para mostrarlo sin prefijo
  if (props.modelValue.startsWith('+54')) {
    return props.modelValue.substring(3).trim()
  }
  
  return props.modelValue
})

// Manejar cambios en el input
const handleInput = (value: string) => {
  let cleanValue = value.trim()
  
  // Remover cualquier +54 que el usuario haya escrito
  if (cleanValue.startsWith('+54')) {
    cleanValue = cleanValue.substring(3).trim()
  }
  
  // Si hay valor, agregamos el prefijo +54
  const fullValue = cleanValue ? `+54${cleanValue}` : ''
  
  emit('update:modelValue', fullValue)
}
</script>