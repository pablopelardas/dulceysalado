<template>
  <div class="space-y-4">
    <!-- Toggle entre URL y Upload -->
    <div class="flex items-center gap-4">
      <URadioGroup 
        v-model="imageSourceType" 
        :items="sourceOptions"
        class="flex gap-4"
      />
    </div>

    <!-- URL Input -->
    <div v-if="imageSourceType === 'url'" class="space-y-2">
      <UFormField :label="urlLabel" :name="urlFieldName">
        <UInput 
          v-model="imageUrl"
          :placeholder="urlPlaceholder"
          :disabled="disabled"
          @update:model-value="onUrlChange"
        />
      </UFormField>
    </div>

    <!-- File Upload -->
    <div v-else class="space-y-2">
      <UFormField :label="fileLabel">
        <UInput 
          ref="fileInput"
          type="file" 
          accept="image/*"
          :disabled="disabled || uploading"
          @change="onFileSelect"
        />
        <template #help>
          <span class="text-sm text-gray-500 dark:text-gray-400">
            Formatos soportados: JPG, PNG, GIF. Tamaño máximo: 5MB
          </span>
        </template>
      </UFormField>
      
      <!-- Progress bar durante upload -->
      <div v-if="uploading" class="space-y-2">
        <div class="flex items-center gap-2">
          <UIcon name="i-heroicons-arrow-path" class="animate-spin h-4 w-4" />
          <span class="text-sm">Subiendo imagen...</span>
        </div>
        <UProgress :value="uploadProgress" />
      </div>
    </div>

    <!-- Preview de imagen -->
    <div v-if="currentImageUrl && !imageError" class="space-y-3">
      <div class="flex items-center justify-between">
        <span class="text-sm font-medium text-gray-700 dark:text-gray-300">Vista previa</span>
        <div class="flex items-center gap-2">
          <UButton
            variant="ghost"
            size="xs"
            color="blue"
            @click="showImageModal = true"
          >
            Ver en grande
          </UButton>
          <UButton
            v-if="canDelete"
            variant="ghost"
            size="xs"
            color="red"
            :loading="deleting"
            @click="deleteImage"
          >
            Eliminar
          </UButton>
        </div>
      </div>
      
      <div class="flex items-center gap-3">
        <img 
          :src="currentImageUrl" 
          :alt="imageAlt || 'Vista previa'"
          class="h-20 w-20 object-cover rounded-lg border cursor-pointer hover:opacity-75 transition-opacity"
          @click="showImageModal = true"
          @error="onImageError"
        />
        <div class="flex-1 text-sm text-gray-600 dark:text-gray-400">
          <p v-if="imageSourceType === 'upload' && !uploading">
            ✓ Imagen subida al servidor
          </p>
          <p v-else-if="imageSourceType === 'url'">
            🔗 Imagen desde URL externa
          </p>
        </div>
      </div>
    </div>

    <!-- Error state -->
    <div v-if="imageError" class="p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
      <div class="flex items-center gap-2">
        <UIcon name="i-heroicons-exclamation-triangle" class="h-4 w-4 text-red-500" />
        <span class="text-sm text-red-600 dark:text-red-400">{{ errorMessage }}</span>
      </div>
    </div>
  </div>

  <!-- Modal para mostrar imagen en grande -->
  <UModal v-model:open="showImageModal">
    <template #header>
      <div class="flex items-center justify-between">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Vista previa de imagen
        </h3>
        <UButton
          variant="ghost"
          size="xs"
          color="gray"
          icon="i-heroicons-x-mark"
          @click="showImageModal = false"
        />
      </div>
    </template>
    <template #body>
      <div class="flex justify-center p-4">
        <img 
          :src="currentImageUrl" 
          :alt="imageAlt || 'Imagen'"
          class="max-w-full max-h-96 object-contain rounded-lg shadow-lg"
        />
      </div>
      <div v-if="imageAlt" class="text-center text-sm text-gray-600 dark:text-gray-400 mt-2">
        {{ imageAlt }}
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
interface Props {
  modelValue?: string | null
  imageAlt?: string
  disabled?: boolean
  productId?: number // Para uploads al servidor
  productType?: 'base' | 'empresa' // Tipo de producto para construir URLs
  companyId?: number // Para uploads de empresas
  companyImageType?: 'logo' | 'favicon' // Tipo de imagen de empresa
  urlLabel?: string
  fileLabel?: string 
  urlPlaceholder?: string
  urlFieldName?: string
  canDelete?: boolean
}

interface Emits {
  'update:modelValue': [value: string | null]
  'image-uploaded': [url: string]
  'image-deleted': []
  'error': [error: string]
}

const props = withDefaults(defineProps<Props>(), {
  productType: 'base',
  urlLabel: 'URL de Imagen',
  fileLabel: 'Subir Imagen',
  urlPlaceholder: 'https://ejemplo.com/imagen.jpg',
  urlFieldName: 'imagen_url',
  canDelete: true
})

const emit = defineEmits<Emits>()

// Estado reactivo
const imageSourceType = ref<'url' | 'upload'>('url')
const imageUrl = ref('')
const showImageModal = ref(false)
const imageError = ref(false)
const errorMessage = ref('')
const uploading = ref(false)
const deleting = ref(false)
const uploadProgress = ref(0)
const fileInput = ref()

// Opciones para el radio group
const sourceOptions = [
  { value: 'url', label: 'URL Externa', description: 'Ingresa la URL de una imagen desde internet' },
  { value: 'upload', label: 'Subir Archivo', description: 'Sube una imagen desde tu computadora' }
]

// Métodos para URLs dinámicas
const getUploadUrl = (): string => {
  // Para imágenes de empresa (logo/favicon)
  if (props.companyId && props.companyImageType) {
    return `/api/companies/${props.companyId}/upload-${props.companyImageType}`
  }
  // Para productos
  if (props.productType === 'empresa') {
    return `/api/ProductosEmpresa/${props.productId}/upload-image`
  }
  return `/api/ProductosBase/${props.productId}/upload-image`
}

const getDeleteUrl = (): string => {
  // Para imágenes de empresa (logo/favicon)
  if (props.companyId && props.companyImageType) {
    return `/api/companies/${props.companyId}/${props.companyImageType}`
  }
  // Para productos
  if (props.productType === 'empresa') {
    return `/api/ProductosEmpresa/${props.productId}/image`
  }
  return `/api/ProductosBase/${props.productId}/image`
}

const getDefaultImageUrl = (): string => {
  // Para imágenes de empresa, no hay URL por defecto
  if (props.companyId && props.companyImageType) {
    return ''
  }
  // Para productos
  if (props.productType === 'empresa') {
    return `${window.location.origin}/api/ProductosEmpresa/${props.productId}/image`
  }
  return `${window.location.origin}/api/ProductosBase/${props.productId}/image`
}

// Computed
const currentImageUrl = computed(() => {
  // Para URL externa, priorizar el valor local si se está editando
  if (imageSourceType.value === 'url') {
    return imageUrl.value || props.modelValue
  }
  // Para uploads, usar el modelValue del padre
  return props.modelValue || imageUrl.value
})

// Métodos
const onUrlChange = (newValue?: string) => {
  // Usar el valor del parámetro si existe, sino usar el reactive value
  const currentValue = newValue !== undefined ? newValue : imageUrl.value
  // Si el campo está vacío, enviar null, sino enviar el valor
  const valueToEmit = currentValue.trim() === '' ? null : currentValue
  emit('update:modelValue', valueToEmit)
  resetError()
}

const onFileSelect = async (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  
  if (!file) return
  
  // Validaciones
  if (!file.type.startsWith('image/')) {
    showError('Por favor selecciona un archivo de imagen válido')
    return
  }
  
  if (file.size > 5 * 1024 * 1024) { // 5MB
    showError('El archivo es demasiado grande. Tamaño máximo: 5MB')
    return
  }
  
  if (!props.productId && !props.companyId) {
    showError('No se puede subir la imagen: ID de producto o empresa requerido')
    return
  }
  
  await uploadFile(file)
}

const uploadFile = async (file: File) => {
  uploading.value = true
  uploadProgress.value = 0
  resetError()
  
  try {
    const formData = new FormData()
    
    // Determinar el nombre del campo según el tipo
    let fieldName = 'image' // default para productos
    if (props.companyId && props.companyImageType) {
      fieldName = props.companyImageType // 'logo' o 'favicon'
    }
    
    formData.append(fieldName, file)
    
    // Simular progreso
    const progressInterval = setInterval(() => {
      if (uploadProgress.value < 90) {
        uploadProgress.value += 10
      }
    }, 100)
    
    const api = useApi()
    const response = await api.post(getUploadUrl(), formData) as any
    
    clearInterval(progressInterval)
    uploadProgress.value = 100
    
    // Obtener la URL de la imagen según el tipo de respuesta
    let imageUrl = ''
    if (props.companyId && props.companyImageType) {
      // Para empresas, la API devuelve logo_url o favicon_url
      imageUrl = response?.logo_url || response?.favicon_url
    } else {
      // Para productos, usar la estructura anterior
      imageUrl = response?.imagen_url || response?.url || getDefaultImageUrl()
    }
    
    emit('update:modelValue', imageUrl)
    emit('image-uploaded', imageUrl)
    
    // Toast de éxito
    const toast = useToast()
    toast.add({
      title: 'Imagen subida',
      description: 'La imagen se ha subido correctamente',
      color: 'green'
    })
    
  } catch (error: any) {
    const { parseApiError } = await import('~/utils/errorParser')
    const parsed = parseApiError(error)
    console.error('Error al subir imagen:', error)
    console.error('Error parseado:', parsed)
    showError(parsed.message)
    emit('error', parsed.message)
  } finally {
    uploading.value = false
    uploadProgress.value = 0
    
    // Limpiar input
    if (fileInput.value) {
      fileInput.value.value = ''
    }
  }
}

const deleteImage = async () => {
  if ((!props.productId && !props.companyId) || !currentImageUrl.value) return
  
  deleting.value = true
  resetError()
  
  try {
    const api = useApi()
    await api.delete(getDeleteUrl())
    
    // Limpiar estado local primero
    imageUrl.value = ''
    
    // Emitir cambios al componente padre
    emit('update:modelValue', null)
    emit('image-deleted')
    
    // Toast de éxito
    const toast = useToast()
    toast.add({
      title: 'Imagen eliminada',
      description: 'La imagen se ha eliminado correctamente',
      color: 'green'
    })
    
  } catch (error: any) {
    const { parseApiError } = await import('~/utils/errorParser')
    const parsed = parseApiError(error)
    console.error('Error al eliminar imagen:', error)
    console.error('Error parseado:', parsed)
    showError(parsed.message)
    emit('error', parsed.message)
  } finally {
    deleting.value = false
  }
}

const onImageError = () => {
  showError('Error al cargar la imagen')
}

const showError = (message: string) => {
  imageError.value = true
  errorMessage.value = message
}

const resetError = () => {
  imageError.value = false
  errorMessage.value = ''
}

// Watchers
watch(() => props.modelValue, (newValue) => {
  if (!newValue) {
    // Si no hay valor, limpiar todo
    imageUrl.value = ''
    imageSourceType.value = 'url'
  } else if (newValue.startsWith('http')) {
    // Es una URL externa
    imageUrl.value = newValue
    imageSourceType.value = 'url'
  } else {
    // Es una imagen subida al servidor
    imageSourceType.value = 'upload'
    // No modificamos imageUrl.value para uploads
  }
  resetError()
}, { immediate: true })

watch(imageSourceType, (newType) => {
  if (newType === 'url') {
    // Cuando cambiamos a URL, usar el valor actual del modelo
    imageUrl.value = props.modelValue || ''
  } else {
    // Cuando cambiamos a upload, limpiar el input local
    imageUrl.value = ''
  }
  resetError()
})
</script>