<template>
  <UCard>
    <template #header>
      <div class="flex items-center justify-between">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Gestión de Imágenes
        </h3>
        <UButton
          color="primary"
          size="sm"
          @click="showUploadModal = true"
        >
          <UIcon name="i-heroicons-plus" class="mr-1" />
          Agregar Imagen
        </UButton>
      </div>
    </template>

    <!-- Lista de imágenes existentes -->
    <div v-if="images.length > 0" class="space-y-4">
      <draggable
        v-model="sortableImages"
        item-key="id"
        handle=".drag-handle"
        @end="onDragEnd"
        class="space-y-3"
      >
        <template #item="{ element: image, index }">
          <div class="flex items-center gap-4 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <!-- Drag handle -->
            <div class="drag-handle cursor-move">
              <UIcon name="i-heroicons-bars-3" class="h-5 w-5 text-gray-400" />
            </div>

            <!-- Preview imagen -->
            <div class="relative">
              <UAvatar
                :src="image.url_medium || image.url_original"
                :alt="image.nombre_archivo"
                size="lg"
                class="rounded-lg"
              />
              <UBadge
                v-if="image.es_principal"
                color="primary"
                variant="solid"
                size="sm"
                class="absolute -top-2 -right-2"
              >
                Principal
              </UBadge>
            </div>

            <!-- Información de la imagen -->
            <div class="flex-1">
              <p class="font-medium text-gray-900 dark:text-gray-100">
                {{ image.nombre_archivo }}
              </p>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                {{ formatFileSize(image.tamaño) }} • {{ image.tipo_mime }}
              </p>
              <p class="text-xs text-gray-400 dark:text-gray-500">
                Orden: {{ image.orden }}
              </p>
            </div>

            <!-- Acciones -->
            <div class="flex items-center gap-2">
              <UButton
                v-if="!image.es_principal"
                variant="soft"
                color="primary"
                size="sm"
                @click="setAsPrincipal(image.id)"
                title="Establecer como principal"
              >
                <UIcon name="i-heroicons-star" />
              </UButton>

              <UButton
                variant="soft"
                color="blue"
                size="sm"
                @click="editImage(image)"
                title="Editar imagen"
              >
                <UIcon name="i-heroicons-pencil" />
              </UButton>

              <UButton
                variant="soft"
                color="red"
                size="sm"
                @click="deleteImage(image.id)"
                title="Eliminar imagen"
              >
                <UIcon name="i-heroicons-trash" />
              </UButton>
            </div>
          </div>
        </template>
      </draggable>
    </div>

    <!-- Estado vacío -->
    <div v-else class="text-center py-8">
      <UIcon name="i-heroicons-photo" class="h-12 w-12 text-gray-400 mx-auto mb-2" />
      <p class="text-gray-600 dark:text-gray-400">No hay imágenes para este producto</p>
      <UButton
        color="primary"
        variant="ghost"
        class="mt-2"
        @click="showUploadModal = true"
      >
        Agregar la primera imagen
      </UButton>
    </div>
  </UCard>

  <!-- Modal de subida de imágenes -->
  <UModal v-model="showUploadModal">
    <template #header>
      <h3 class="text-lg font-semibold">Subir Imágenes</h3>
    </template>
    
    <template #body>
      <div class="space-y-4">
        <!-- Drag & Drop Area -->
        <div
          class="border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg p-6 text-center transition-colors"
          :class="{
            'border-primary-500 bg-primary-50 dark:bg-primary-950': isDragOver,
            'hover:border-gray-400 dark:hover:border-gray-500': !isDragOver
          }"
          @dragover.prevent="isDragOver = true"
          @dragleave.prevent="isDragOver = false"
          @drop.prevent="handleDrop"
        >
          <UIcon name="i-heroicons-cloud-arrow-up" class="h-12 w-12 text-gray-400 mx-auto mb-2" />
          <p class="text-gray-600 dark:text-gray-400 mb-2">
            Arrastra las imágenes aquí o haz clic para seleccionar
          </p>
          <UButton variant="outline" @click="$refs.fileInput?.click()">
            Seleccionar Archivos
          </UButton>
          <input
            ref="fileInput"
            type="file"
            multiple
            accept="image/*"
            class="hidden"
            @change="handleFileSelect"
          />
        </div>

        <!-- Preview de archivos seleccionados -->
        <div v-if="selectedFiles.length > 0" class="space-y-2">
          <h4 class="font-medium">Archivos seleccionados:</h4>
          <div class="space-y-2 max-h-48 overflow-y-auto">
            <div
              v-for="(file, index) in selectedFiles"
              :key="index"
              class="flex items-center gap-3 p-2 bg-gray-50 dark:bg-gray-800 rounded"
            >
              <img
                :src="getFilePreview(file)"
                :alt="file.name"
                class="h-10 w-10 object-cover rounded"
              />
              <div class="flex-1">
                <p class="text-sm font-medium">{{ file.name }}</p>
                <p class="text-xs text-gray-500">{{ formatFileSize(file.size) }}</p>
              </div>
              <UButton
                variant="ghost"
                color="red"
                size="sm"
                @click="removeFile(index)"
              >
                <UIcon name="i-heroicons-x-mark" />
              </UButton>
            </div>
          </div>
        </div>
      </div>
    </template>

    <template #footer>
      <div class="flex justify-end space-x-3">
        <UButton
          variant="ghost"
          color="gray"
          @click="showUploadModal = false"
        >
          Cancelar
        </UButton>
        <UButton
          color="primary"
          :loading="uploading"
          :disabled="selectedFiles.length === 0"
          @click="uploadImages"
        >
          Subir {{ selectedFiles.length }} imagen{{ selectedFiles.length !== 1 ? 'es' : '' }}
        </UButton>
      </div>
    </template>
  </UModal>

  <!-- Modal de edición de imagen -->
  <UModal v-model="showEditModal">
    <template #header>
      <h3 class="text-lg font-semibold">Editar Imagen</h3>
    </template>
    
    <template #body>
      <div v-if="editingImage" class="space-y-4">
        <!-- Preview -->
        <div class="text-center">
          <img
            :src="editingImage.url_medium || editingImage.url_original"
            :alt="editingImage.nombre_archivo"
            class="max-h-48 mx-auto rounded-lg"
          />
        </div>

        <!-- Formulario de edición -->
        <UFormField label="Nombre del archivo">
          <UInput v-model="editingImage.nombre_archivo" />
        </UFormField>

        <UFormField label="Texto alternativo">
          <UInput v-model="editingImage.alt_text" placeholder="Descripción de la imagen" />
        </UFormField>

        <UFormField label="Orden">
          <UInput v-model.number="editingImage.orden" type="number" />
        </UFormField>

        <UFormField label="Imagen principal">
          <USwitch v-model="editingImage.es_principal" />
        </UFormField>
      </div>
    </template>

    <template #footer>
      <div class="flex justify-end space-x-3">
        <UButton
          variant="ghost"
          color="gray"
          @click="showEditModal = false"
        >
          Cancelar
        </UButton>
        <UButton
          color="primary"
          :loading="updating"
          @click="updateImage"
        >
          Guardar Cambios
        </UButton>
      </div>
    </template>
  </UModal>
</template>

<script setup lang="ts">
import draggable from 'vuedraggable'

// Mock interface para imágenes (se reemplazaría con la interfaz real de la API)
interface ProductoImagen {
  id: number
  nombre_archivo: string
  url_original: string
  url_thumbnail: string
  url_medium: string
  orden: number
  es_principal: boolean
  tamaño: number
  tipo_mime: string
  alt_text?: string
}

interface Props {
  productoId: number
  images?: ProductoImagen[]
}

interface Emits {
  'update:images': [images: ProductoImagen[]]
  'image-uploaded': [image: ProductoImagen]
  'image-deleted': [imageId: number]
  'image-updated': [image: ProductoImagen]
}

const props = withDefaults(defineProps<Props>(), {
  images: () => []
})

const emit = defineEmits<Emits>()

// Composables
const toast = useToast()

// Estado reactivo
const sortableImages = ref([...props.images])
const showUploadModal = ref(false)
const showEditModal = ref(false)
const isDragOver = ref(false)
const selectedFiles = ref<File[]>([])
const uploading = ref(false)
const updating = ref(false)
const editingImage = ref<ProductoImagen | null>(null)

// Computed
const images = computed(() => sortableImages.value)

// Métodos
const onDragEnd = () => {
  // Actualizar el orden de las imágenes
  sortableImages.value.forEach((image, index) => {
    image.orden = index + 1
  })
  
  // Aquí se haría la llamada a la API para actualizar el orden
  updateImagesOrder()
}

const setAsPrincipal = async (imageId: number) => {
  try {
    // Actualizar localmente
    sortableImages.value.forEach(img => {
      img.es_principal = img.id === imageId
    })
    
    // Aquí se haría la llamada a la API
    toast.add({
      title: 'Imagen principal actualizada',
      color: 'green'
    })
    
    emit('update:images', sortableImages.value)
  } catch (error) {
    toast.add({
      title: 'Error al actualizar imagen principal',
      color: 'red'
    })
  }
}

const editImage = (image: ProductoImagen) => {
  editingImage.value = { ...image }
  showEditModal.value = true
}

const updateImage = async () => {
  if (!editingImage.value) return
  
  updating.value = true
  try {
    // Actualizar en la lista local
    const index = sortableImages.value.findIndex(img => img.id === editingImage.value!.id)
    if (index !== -1) {
      sortableImages.value[index] = { ...editingImage.value }
    }
    
    // Aquí se haría la llamada a la API
    toast.add({
      title: 'Imagen actualizada',
      color: 'green'
    })
    
    emit('image-updated', editingImage.value)
    showEditModal.value = false
  } catch (error) {
    toast.add({
      title: 'Error al actualizar imagen',
      color: 'red'
    })
  } finally {
    updating.value = false
  }
}

const deleteImage = async (imageId: number) => {
  try {
    // Eliminar de la lista local
    const index = sortableImages.value.findIndex(img => img.id === imageId)
    if (index !== -1) {
      sortableImages.value.splice(index, 1)
    }
    
    // Aquí se haría la llamada a la API
    toast.add({
      title: 'Imagen eliminada',
      color: 'green'
    })
    
    emit('image-deleted', imageId)
  } catch (error) {
    toast.add({
      title: 'Error al eliminar imagen',
      color: 'red'
    })
  }
}

const handleDrop = (event: DragEvent) => {
  isDragOver.value = false
  const files = Array.from(event.dataTransfer?.files || [])
  addFiles(files)
}

const handleFileSelect = (event: Event) => {
  const target = event.target as HTMLInputElement
  const files = Array.from(target.files || [])
  addFiles(files)
}

const addFiles = (files: File[]) => {
  const imageFiles = files.filter(file => file.type.startsWith('image/'))
  selectedFiles.value.push(...imageFiles)
  
  if (imageFiles.length !== files.length) {
    toast.add({
      title: 'Algunos archivos no son imágenes',
      description: 'Solo se pueden subir archivos de imagen',
      color: 'orange'
    })
  }
}

const removeFile = (index: number) => {
  selectedFiles.value.splice(index, 1)
}

const getFilePreview = (file: File) => {
  return URL.createObjectURL(file)
}

const uploadImages = async () => {
  if (selectedFiles.value.length === 0) return
  
  uploading.value = true
  try {
    // Simular upload (aquí se haría la llamada real a la API)
    for (const file of selectedFiles.value) {
      const newImage: ProductoImagen = {
        id: Date.now() + Math.random(),
        nombre_archivo: file.name,
        url_original: URL.createObjectURL(file),
        url_thumbnail: URL.createObjectURL(file),
        url_medium: URL.createObjectURL(file),
        orden: sortableImages.value.length + 1,
        es_principal: sortableImages.value.length === 0,
        tamaño: file.size,
        tipo_mime: file.type
      }
      
      sortableImages.value.push(newImage)
      emit('image-uploaded', newImage)
    }
    
    toast.add({
      title: `${selectedFiles.value.length} imagen${selectedFiles.value.length !== 1 ? 'es' : ''} subida${selectedFiles.value.length !== 1 ? 's' : ''}`,
      color: 'green'
    })
    
    selectedFiles.value = []
    showUploadModal.value = false
    emit('update:images', sortableImages.value)
  } catch (error) {
    toast.add({
      title: 'Error al subir imágenes',
      color: 'red'
    })
  } finally {
    uploading.value = false
  }
}

const updateImagesOrder = async () => {
  try {
    // Aquí se haría la llamada a la API para actualizar el orden
    emit('update:images', sortableImages.value)
  } catch (error) {
    toast.add({
      title: 'Error al actualizar orden de imágenes',
      color: 'red'
    })
  }
}

const formatFileSize = (bytes: number) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

// Watchers
watch(() => props.images, (newImages) => {
  sortableImages.value = [...newImages]
}, { deep: true })
</script>