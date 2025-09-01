<!-- PhoneInput.vue - Componente de input de teléfono con prefijo +54 -->
<template>
  <div class="relative">
    <label v-if="label" :for="inputId" class="block text-sm font-medium text-gray-700 mb-2">
      {{ label }} {{ required ? '*' : '' }}
    </label>
    
    <div class="relative flex">
      <!-- Prefijo +54 -->
      <div class="inline-flex items-center px-3 rounded-l-lg border border-r-0 border-gray-300 bg-gray-50 text-gray-700 text-sm font-medium">
        +54
      </div>
      
      <!-- Input del teléfono -->
      <input
        :id="inputId"
        :value="internalValue"
        @input="handleInput"
        type="tel"
        :required="required"
        :disabled="disabled"
        class="flex-1 px-3 py-2 border border-gray-300 rounded-r-lg shadow-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-colors duration-200 disabled:bg-gray-100 disabled:cursor-not-allowed"
        :placeholder="placeholder"
      />
    </div>
    
    <p v-if="helpText" class="mt-1 text-xs text-gray-500">
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
const handleInput = (event: Event) => {
  const target = event.target as HTMLInputElement
  let value = target.value.trim()
  
  // Remover cualquier +54 que el usuario haya escrito
  if (value.startsWith('+54')) {
    value = value.substring(3).trim()
  }
  
  // Si hay valor, agregamos el prefijo +54
  const fullValue = value ? `+54${value}` : ''
  
  emit('update:modelValue', fullValue)
}
</script>