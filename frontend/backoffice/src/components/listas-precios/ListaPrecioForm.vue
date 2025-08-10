<template>
  <UForm :schema="schema" :state="formData" @submit="handleSubmit">
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          {{ mode === 'create' ? 'Nueva Lista de Precios' : 'Editar Lista de Precios' }}
        </h3>
      </template>

      <!-- Información básica -->
      <div class="space-y-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <!-- Código -->
          <UFormField label="Código" name="codigo" required>
            <UInput
              v-model="formData.codigo"
              placeholder="Ej: LISTA01"
              :disabled="loading"
            />
          </UFormField>

          <!-- Nombre -->
          <UFormField label="Nombre" name="nombre" required>
            <UInput
              v-model="formData.nombre"
              placeholder="Ej: Lista Minorista"
              :disabled="loading"
            />
          </UFormField>
        </div>
      </div>

      <template #footer>
        <div class="flex justify-end gap-3">
          <UButton
            color="gray"
            variant="ghost"
            @click="handleCancel"
            :disabled="loading"
          >
            Cancelar
          </UButton>
          <UButton
            type="submit"
            color="primary"
            :loading="loading"
            :disabled="loading"
          >
            {{ mode === 'create' ? 'Crear Lista' : 'Guardar Cambios' }}
          </UButton>
        </div>
      </template>
    </UCard>
  </UForm>
</template>

<script setup lang="ts">
import { z } from 'zod'
import type { ListaPrecioDto } from '~/types/listas-precios'

interface Props {
  mode: 'create' | 'edit'
  initialData?: ListaPrecioDto
  loading?: boolean
}

interface Emits {
  submit: [data: any]
  cancel: []
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Schema de validación
const schema = z.object({
  codigo: z.string({ required_error: 'El código es requerido' })
    .min(1, 'El código debe tener al menos 1 caracter')
    .max(50, 'El código no puede tener más de 50 caracteres'),
  nombre: z.string({ required_error: 'El nombre es requerido' })
    .min(3, 'El nombre debe tener al menos 3 caracteres')
    .max(100, 'El nombre no puede tener más de 100 caracteres')
})

// Estado del formulario
const formData = reactive({
  codigo: props.initialData?.codigo || '',
  nombre: props.initialData?.nombre || ''
})

// Actualizar datos cuando cambian las props
watch(() => props.initialData, (newData) => {
  if (newData) {
    Object.assign(formData, {
      codigo: newData.codigo || '',
      nombre: newData.nombre || ''
    })
  }
}, { immediate: true })

// Handlers
const handleSubmit = () => {
  emit('submit', formData)
}

const handleCancel = () => {
  emit('cancel')
}
</script>