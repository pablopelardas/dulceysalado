<!-- SearchBar.vue - Simplificado -->
<template>
  <div class="relative">
    <div class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
      <MagnifyingGlassIcon class="h-5 w-5 text-gray-500" />
    </div>
    
    <input
      v-model="localValue"
      type="text"
      :placeholder="placeholder"
      :class="[
        'w-full pl-11 pr-11 rounded-lg bg-white border border-gray-300 text-gray-800 placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-all shadow-sm',
        compact ? 'py-2 text-sm' : 'py-3'
      ]"
      @keyup.enter="handleSearch"
      @input="handleInput"
    />
    
    <div class="absolute inset-y-0 right-0 pr-2 flex items-center gap-1">
      <!-- Search button - always visible -->
      <button
        @click="handleSearch"
        :class="localValue ? 'text-gray-600 hover:text-gray-800' : 'text-gray-400'"
        :disabled="!localValue"
        class="p-2 transition-colors cursor-pointer disabled:cursor-not-allowed"
        title="Buscar"
      >
        <MagnifyingGlassIcon class="h-4 w-4" />
      </button>
      <!-- Clear button - always reserve space but only show when there's text -->
      <button
        @click="clearSearch"
        :class="localValue ? 'text-gray-500 hover:text-gray-700' : 'text-transparent pointer-events-none'"
        class="p-2 transition-colors cursor-pointer"
        title="Limpiar búsqueda"
      >
        <XMarkIcon class="h-5 w-5" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { MagnifyingGlassIcon, XMarkIcon } from '@heroicons/vue/24/outline'

// Props
interface Props {
  modelValue: string
  placeholder?: string
  debounceMs?: number
  compact?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: 'Buscar productos...',
  debounceMs: 500,
  compact: false
})

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: string]
  'search': []
  'searchWithScroll': []
}>()

// Local state
const localValue = ref(props.modelValue)
let debounceTimer: number | null = null

// Watch
watch(() => props.modelValue, (newValue) => {
  localValue.value = newValue
})

// Methods

const handleSearch = () => {
  if (debounceTimer) {
    clearTimeout(debounceTimer)
    debounceTimer = null
  }
  
  emit('update:modelValue', localValue.value)
  emit('search')
  emit('searchWithScroll')
}

const handleInput = () => {
  if (debounceTimer) clearTimeout(debounceTimer)
  
  emit('update:modelValue', localValue.value)
  
  debounceTimer = setTimeout(() => {
    // Emit both search and scroll after debounce
    emit('search')
    emit('searchWithScroll')
  }, props.debounceMs)
}

const clearSearch = () => {
  localValue.value = ''
  emit('update:modelValue', '')
  emit('search')
}
</script>