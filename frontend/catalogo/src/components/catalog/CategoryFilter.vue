<template>
  <div class="relative">
    <select
      v-model="localValue"
      @change="handleChange"
      class="block w-full pl-3 pr-10 py-2 text-base border border-gray-300 focus:outline-none focus:ring-[--theme-primary] focus:border-[--theme-primary] rounded-md bg-white"
    >
      <option :value="null">Todas las categorías</option>
      <option
        v-for="category in categories"
        :key="category.id"
        :value="category.codigo_rubro"
      >
        {{ category.nombre }} ({{ category.product_count }})
      </option>
    </select>
    
    <div class="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
      <ChevronDownIcon class="h-5 w-5 text-gray-400" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { ChevronDownIcon } from '@heroicons/vue/24/outline'
import type { Category } from '@/services/api'

// Props
interface Props {
  modelValue: number | null
  categories: Category[]
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  'update:modelValue': [value: number | null]
  'change': []
}>()

// Local state
const localValue = ref<number | null>(props.modelValue)

// Watch for prop changes
watch(() => props.modelValue, (newValue) => {
  localValue.value = newValue
})

// Methods
const handleChange = () => {
  emit('update:modelValue', localValue.value)
  emit('change')
}
</script>