<template>
  <UForm 
    ref="form"
    :schema="schema" 
    :state="formData"
    @submit="onSubmit"
    @error="onError"
    class="space-y-6"
  >
    <!-- Información básica -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Información de la Categoría
        </h3>
      </template>
      
      <div class="space-y-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <UFormField label="Código de Rubro" name="codigo_rubro" required>
            <UInput 
              v-model.number="formData.codigo_rubro"
              type="number"
              min="1"
              placeholder="Código numérico único"
              :disabled="loading"
            />
            <template #help>
              <span class="text-sm text-gray-500 dark:text-gray-400">
                Código numérico único para la categoría
              </span>
            </template>
          </UFormField>
          
          <UFormField label="Nombre" name="nombre" required>
            <UInput 
              v-model="formData.nombre"
              placeholder="Nombre de la categoría"
              :disabled="loading"
            />
            <template #help>
              <span class="text-sm text-gray-500 dark:text-gray-400">
                Nombre único e identificativo para la categoría
              </span>
            </template>
          </UFormField>
        </div>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <UFormField label="Icono" name="icono">
            <UInput 
              v-model="formData.icono"
              placeholder=""
              :disabled="loading"
            />
            <template #help>
              <span class="text-sm text-gray-500 dark:text-gray-400">
                Emoji o texto para representar la categoría
              </span>
            </template>
          </UFormField>
          
          <UFormField label="Orden" name="orden">
            <UInput 
              v-model.number="formData.orden"
              type="number"
              min="0"
              max="9999"
              placeholder="0"
              :disabled="loading"
            />
            <template #help>
              <span class="text-sm text-gray-500 dark:text-gray-400">
                Orden de aparición (0-9999)
              </span>
            </template>
          </UFormField>
        </div>
      </div>
    </UCard>

    <!-- Descripción detallada -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Descripción
        </h3>
      </template>
      
      <UFormField label="Descripción" name="descripcion">
        <UTextarea 
          v-model="formData.descripcion"
          placeholder="Descripción opcional de la categoría (máximo 500 caracteres)"
          :disabled="loading"
          class="w-full"
          :rows="4"
        />
        <template #help>
          <span class="text-sm text-gray-500 dark:text-gray-400">
            Descripción detallada que ayude a identificar el contenido de esta categoría
          </span>
        </template>
      </UFormField>
    </UCard>

    <!-- Configuración -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Configuración
        </h3>
      </template>
      
      <UFormField label="Visibilidad" name="visible">
        <div class="flex items-center gap-4">
          <USwitch 
            v-model="formData.visible"
            :disabled="loading"
          />
          <span class="text-sm text-gray-600 dark:text-gray-400">
            {{ formData.visible ? 'Categoría visible en el catálogo' : 'Categoría oculta del catálogo' }}
          </span>
        </div>
        <template #help>
          <span class="text-sm text-gray-500 dark:text-gray-400">
            Las categorías ocultas no aparecen en el catálogo público
          </span>
        </template>
      </UFormField>
    </UCard>

    <!-- Botones de acción -->
    <div class="flex justify-end space-x-3 pt-6">
      <UButton
        variant="ghost"
        color="gray"
        @click="onCancel"
        :disabled="loading"
      >
        Cancelar
      </UButton>
      <UButton
        type="submit"
        color="primary"
        :loading="loading"
      >
        {{ mode === 'create' ? 'Crear Categoría' : 'Actualizar Categoría' }}
      </UButton>
    </div>
  </UForm>
</template>

<script setup lang="ts">
import { z } from 'zod'
import type { CategoryBaseDto, CreateCategoryBaseCommand, UpdateCategoryBaseCommand } from '~/types/categorias'

interface Props {
  mode: 'create' | 'edit'
  initialData?: CategoryBaseDto
  loading?: boolean
}

interface Emits {
  submit: [data: CreateCategoryBaseCommand | UpdateCategoryBaseCommand]
  cancel: []
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const { user } = useAuth()

// Schema de validación
const schema = z.object({
  empresa_id: z.number({ required_error: 'El ID de empresa es requerido' })
    .min(1, 'El ID de empresa debe ser mayor a 0'),
  codigo_rubro: z.number({ required_error: 'El código de rubro es requerido' })
    .min(1, 'El código de rubro debe ser mayor a 0'),
  nombre: z.string({ required_error: 'El nombre es requerido' })
    .min(2, 'El nombre debe tener al menos 2 caracteres')
    .max(100, 'El nombre no puede exceder 100 caracteres'),
  descripcion: z.string().max(500, 'La descripción no puede exceder 500 caracteres').nullable().optional(),
  icono: z.string().max(10, 'El icono no puede exceder 10 caracteres').nullable().optional(),
  visible: z.boolean(),
  orden: z.number().min(0, 'El orden no puede ser negativo').max(9999, 'El orden no puede exceder 9999')
})

// Estado del formulario
const formData = reactive({
  empresa_id: props.initialData?.created_by_empresa_id || user.value?.empresa?.id || 0,
  codigo_rubro: props.initialData?.codigo_rubro || 0,
  nombre: props.initialData?.nombre || '',
  descripcion: props.initialData?.descripcion || '',
  icono: props.initialData?.icono || null,
  visible: props.initialData?.visible ?? true,
  orden: props.initialData?.orden || 0
})

// Métodos
const onSubmit = async (_: any) => {
  try {
    // Limpiar campos vacíos
    const cleanData = { ...formData }
    if (cleanData.descripcion === '') {
      cleanData.descripcion = null
    }
    if (cleanData.icono === '') {
      cleanData.icono = null
    }
    
    if (props.mode === 'create') {
      emit('submit', cleanData as CreateCategoryBaseCommand)
    } else {
      const updateData = { ...cleanData, id: props.initialData!.id }
      emit('submit', updateData as UpdateCategoryBaseCommand)
    }
  } catch (error) {
    console.error('Error en validación:', error)
  }
}

const onError = (event: any) => {
  console.error('Errores de validación:', event)
}

const onCancel = () => {
  emit('cancel')
}

// Actualizar datos cuando cambian las props
watch(() => props.initialData, (newData) => {
  if (newData) {
    Object.assign(formData, {
      empresa_id: newData.created_by_empresa_id || user.value?.empresa?.id || 0,
      codigo_rubro: newData.codigo_rubro || 0,
      nombre: newData.nombre || '',
      descripcion: newData.descripcion || '',
      icono: newData.icono || null,
      visible: newData.visible ?? true,
      orden: newData.orden || 0
    })
  }
}, { immediate: true })
</script>