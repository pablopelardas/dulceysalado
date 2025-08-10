<template>
  <UModal v-model:open="isOpen">
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Editar Imagen del Producto
          </h3>
          <UButton
            color="gray"
            variant="ghost"
            icon="i-heroicons-x-mark-20-solid"
            class="-my-1"
            @click="closeModal"
          />
        </div>
      </template>
      <template #body>

        <div class="space-y-4">
          <!-- Información del producto -->
          <div class="flex items-center gap-4 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <UAvatar
              :src="currentImageUrl || undefined"
              :alt="producto?.descripcion || 'Producto'"
              size="lg"
              class="rounded-lg"
            />
            <div class="flex-1">
              <h4 class="font-medium text-gray-900 dark:text-gray-100">
                {{ producto?.descripcion || 'Sin descripción' }}
              </h4>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                Código: {{ producto?.codigo }}
              </p>
            </div>
          </div>
  
          <!-- Componente de carga de imagen -->
          <ImageUpload
            v-model="imageUrl"
            :image-alt="imageAlt"
            :disabled="updating"
            :product-id="producto?.id"
            :product-type="productType"
            url-field-name="imagen_url"
            @image-uploaded="onImageUploaded"
            @image-deleted="onImageDeleted"
            @error="onImageError"
          />
  
          <!-- Campo de texto alternativo -->
          <UFormField label="Texto alternativo de imagen">
            <UInput
              v-model="imageAlt"
              placeholder="Descripción de la imagen"
              :disabled="updating"
            />
            <template #help>
              <span class="text-sm text-gray-500 dark:text-gray-400">
                Descripción para accesibilidad y SEO
              </span>
            </template>
          </UFormField>
        </div>
      </template>

      <template #footer>
        <div class="flex justify-end gap-3">
          <UButton
            color="gray"
            variant="ghost"
            @click="closeModal"
            :disabled="updating"
          >
            Cancelar
          </UButton>
          <UButton
            color="primary"
            :loading="updating"
            @click="saveChanges"
          >
            Guardar Cambios
          </UButton>
        </div>
      </template>
  </UModal>
</template>

<script setup lang="ts">
import ImageUpload from './ImageUpload.vue'
import type { ProductoBaseDto, ProductoEmpresaDtoUpdated } from '~/types/productos'

interface Props {
  modelValue: boolean
  producto?: ProductoBaseDto | ProductoEmpresaDtoUpdated | null
  productType: 'base' | 'empresa'
}

interface Emits {
  'update:modelValue': [value: boolean]
  'image-updated': [productId: number, imageUrl: string | null, imageAlt: string | null]
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const api = useApi()
const toast = useToast()

// Estado reactivo
const isOpen = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const imageUrl = ref('')
const imageAlt = ref('')
const updating = ref(false)

// Computed
const currentImageUrl = computed(() => {
  return imageUrl.value || props.producto?.imagen_url || null
})

// Métodos
const closeModal = () => {
  if (!updating.value) {
    isOpen.value = false
    resetForm()
  }
}

const resetForm = () => {
  imageUrl.value = props.producto?.imagen_url || ''
  imageAlt.value = props.producto?.imagen_alt || ''
}

const onImageUploaded = (url: string) => {
  imageUrl.value = url
}

const onImageDeleted = () => {
  imageUrl.value = ''
}

const onImageError = (error: string) => {
  toast.add({
    title: 'Error con la imagen',
    description: error,
    color: 'red'
  })
}

const saveChanges = async () => {
  if (!props.producto?.id) {
    toast.add({
      title: 'Error',
      description: 'No se puede actualizar: producto no válido',
      color: 'red'
    })
    return
  }

  updating.value = true

  try {
    // Preparar datos para la API
    const updateData = {
      id: props.producto.id,
      imagen_url: imageUrl.value || null,
      imagen_alt: imageAlt.value || null
    }

    // Determinar el endpoint según el tipo de producto
    const endpoint = props.productType === 'empresa' 
      ? `/api/ProductosEmpresa/${props.producto.id}`
      : `/api/ProductosBase/${props.producto.id}`

    // Realizar la actualización
    await api.put(endpoint, updateData)

    // Emitir evento de actualización
    emit('image-updated', props.producto.id, updateData.imagen_url, updateData.imagen_alt)

    // Mostrar éxito
    toast.add({
      title: 'Imagen actualizada',
      description: 'La imagen del producto se ha actualizado correctamente',
      color: 'green'
    })

    // Cerrar modal
    closeModal()

  } catch (error: any) {
    console.error('Error updating image:', error)
    const { parseApiError } = await import('~/utils/errorParser')
    const parsed = parseApiError(error)
    
    toast.add({
      title: 'Error al actualizar imagen',
      description: parsed.message,
      color: 'red'
    })
  } finally {
    updating.value = false
  }
}

// Watchers
watch(() => props.producto, (newProducto) => {
  if (newProducto) {
    resetForm()
  }
}, { immediate: true })

watch(isOpen, (newOpen) => {
  if (newOpen && props.producto) {
    resetForm()
  }
})
</script>