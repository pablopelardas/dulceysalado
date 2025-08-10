<template>
  <UFormField :label="label">
    <USelectMenu
      v-model="selectedListaId"
      :items="listas"
      :placeholder="placeholder"
      :disabled="disabled || loading"
      :loading="loading"
      label-key="nombre"
      value-key="id"
      :search-input="{
        placeholder: 'Buscar lista...'
      }"
      class="w-full"
    >
      <template #default>
        <div v-if="selectedLista" class="flex items-center gap-2">
          <UIcon 
            name="i-heroicons-currency-dollar" 
            class="h-4 w-4 text-gray-500" 
          />
          <span class="truncate">{{ selectedLista.nombre }}</span>
          <UBadge 
            v-if="selectedLista.es_predeterminada" 
            size="xs" 
            color="blue" 
            variant="soft"
          >
            Por defecto
          </UBadge>
        </div>
        <div v-else class="flex items-center gap-2 text-gray-500">
          <UIcon name="i-heroicons-currency-dollar" class="h-4 w-4" />
          <span>{{ placeholder }}</span>
        </div>
      </template>

      <template #item="{ item }">
        <div class="flex items-center justify-between w-full">
          <div class="flex items-center gap-2">
            <UIcon 
              name="i-heroicons-currency-dollar" 
              class="h-4 w-4 text-gray-400" 
            />
            <div class="flex flex-col">
              <span class="font-medium">{{ item.nombre }}</span>
              <span v-if="item.codigo" class="text-xs text-gray-500">
                {{ item.codigo }}
              </span>
            </div>
          </div>
          <UBadge 
            v-if="item.es_predeterminada" 
            size="xs" 
            color="blue" 
            variant="soft"
          >
            Por defecto
          </UBadge>
        </div>
      </template>

      <template #empty="{ searchTerm }">
        <div class="text-center p-4">
          <UIcon 
            name="i-heroicons-magnifying-glass" 
            class="h-8 w-8 text-gray-400 mx-auto mb-2" 
          />
          <p class="text-sm text-gray-500">
            No se encontraron listas que coincidan con "{{ searchTerm }}"
          </p>
        </div>
      </template>

      <template v-if="listas.length === 0" #empty>
        <div class="text-center p-4">
          <UIcon 
            name="i-heroicons-currency-dollar" 
            class="h-8 w-8 text-gray-400 mx-auto mb-2" 
          />
          <p class="text-sm text-gray-500">
            No hay listas de precios disponibles
          </p>
        </div>
      </template>
    </USelectMenu>
  </UFormField>
</template>

<script setup lang="ts">
import type { ListaPrecioInfo } from '~/types/productos'

interface Props {
  modelValue?: ListaPrecioInfo | null
  listas?: ListaPrecioInfo[]
  label?: string
  placeholder?: string
  disabled?: boolean
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  label: 'Lista de Precios',
  placeholder: 'Selecciona una lista de precios',
  listas: () => []
})

const emit = defineEmits(['update:modelValue', 'change'])

// Estado reactivo - manejar la conversión entre ID y objeto
const selectedListaId = computed({
  get: () => props.modelValue?.id || undefined,
  set: (id: number | undefined) => {
    const lista = id ? props.listas.find(l => l.id === id) || null : null
    emit('update:modelValue', lista)
    emit('change', lista)
  }
})

// Computed para el objeto seleccionado
const selectedLista = computed(() => props.modelValue)

// Watcher para establecer lista por defecto si no hay selección
watch(() => props.listas, (newListas) => {
  if (newListas && newListas.length > 0 && !props.modelValue) {
    // Buscar lista predeterminada
    const listaPredeterminada = newListas.find(lista => lista.es_predeterminada)
    if (listaPredeterminada) {
      selectedListaId.value = listaPredeterminada.id
    } else {
      // Si no hay predeterminada, seleccionar la primera
      selectedListaId.value = newListas[0].id
    }
  }
}, { immediate: true })
</script>