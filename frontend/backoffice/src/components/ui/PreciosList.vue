<template>
  <div class="space-y-4">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h3 class="text-lg font-medium text-gray-900 dark:text-gray-100">
          Precios por Lista
        </h3>
        <p class="text-sm text-gray-500 dark:text-gray-400">
          Configura precios específicos para cada lista disponible
        </p>
      </div>
      
      <UButton
        v-if="!readonly && canAddPrice"
        variant="outline"
        size="sm"
        icon="i-heroicons-plus"
        @click="showAddModal = true"
      >
        Agregar Precio
      </UButton>
    </div>

    <!-- Loading state -->
    <div v-if="loading" class="space-y-3">
      <USkeleton class="h-16 w-full" v-for="i in 3" :key="i" />
    </div>

    <!-- Error state -->
    <div v-else-if="error" class="text-center py-8">
      <UIcon name="i-heroicons-exclamation-triangle" class="h-12 w-12 text-red-500 mx-auto mb-4" />
      <p class="text-red-600 dark:text-red-400 mb-4">{{ error }}</p>
      <UButton @click="refetch" color="red" variant="outline">
        Reintentar
      </UButton>
    </div>

    <!-- Lista de precios -->
    <div v-else-if="displayPrecios.length > 0" class="space-y-4">
      <div
        v-for="precio in displayPrecios"
        :key="precio.lista_precio_id"
        class="flex items-start justify-between p-4 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
      >
        <div class="flex items-center gap-3 flex-1">
          <div class="flex-shrink-0">
            <UIcon name="i-heroicons-currency-dollar" class="h-5 w-5 text-gray-400" />
          </div>
          
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2">
              <h4 class="font-medium text-gray-900 dark:text-gray-100 truncate">
                {{ precio.lista_precio_nombre }}
              </h4>
              <UBadge 
                v-if="isListaPredeterminada(precio.lista_precio_id)" 
                size="xs" 
                color="blue" 
                variant="soft"
              >
                Por defecto
              </UBadge>
            </div>
            <p v-if="precio.lista_precio_codigo" class="text-xs text-gray-500">
              {{ precio.lista_precio_codigo }}
            </p>
            <p v-if="precio.ultima_actualizacion" class="text-xs text-gray-500">
              Actualizado: {{ formatDate(precio.ultima_actualizacion) }}
            </p>
          </div>
        </div>

        <div class="flex items-center gap-3">
          <!-- Campo de precio -->
          <div class="w-48">
            <PrecioField
              :model-value="precio.precio"
              :readonly="readonly"
              :lista="getListaInfo(precio.lista_precio_id)"
              @save="(newPrecio) => handleUpdatePrecio(precio.lista_precio_id, newPrecio)"
              @error="handlePrecioError"
              @update:model-value="(value) => updateLocalPrecio(precio.lista_precio_id, value)"
            />
          </div>

          <!-- Botón eliminar -->
          <UButton
            v-if="!readonly"
            variant="ghost"
            size="xs"
            color="red"
            icon="i-heroicons-trash"
            :loading="deletingPrice === precio.lista_precio_id"
            @click="confirmDeletePrecio(precio)"
            title="Eliminar precio"
          />
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-else class="text-center py-8">
      <UIcon name="i-heroicons-currency-dollar" class="h-12 w-12 text-gray-400 mx-auto mb-4" />
      <p class="text-gray-500 dark:text-gray-400 mb-4">
        No hay precios configurados para este producto
      </p>
      <UButton
        v-if="!readonly && canAddPrice"
        @click="showAddModal = true"
        color="primary"
        variant="outline"
      >
        Agregar Primer Precio
      </UButton>
    </div>

    <!-- Modal para agregar precio -->
    <UModal v-model:open="showAddModal">
        <template #header>
          <h3 class="text-lg font-semibold">Agregar Precio</h3>
        </template>
        <template #body>
          <div class="space-y-4 flex gap-4">
            <UFormField label="Lista de Precios" required>
              <USelectMenu
                v-model="newPrecio.listaId"
                :items="listasDisponibles"
                placeholder="Selecciona una lista"
                label-key="nombre"
                value-key="id"
              >
                <template #item="{ item }">
                  <div class="flex items-center justify-between w-full">
                    <span>{{ item.nombre }}</span>
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
              </USelectMenu>
            </UFormField>
  
            <UFormField label="Precio" required>
              <UInput
                v-model="newPrecio.precioInput"
                placeholder="0.00"
                @input="validateNewPrecio"
              >
                <template #leading>
                  <span class="text-gray-500">$</span>
                </template>
              </UInput>
              <template v-if="newPrecioError" #help>
                <span class="text-red-500">{{ newPrecioError }}</span>
              </template>
            </UFormField>
          </div>
        </template>

        <template #footer>
          <div class="flex items-center justify-end gap-3">
            <UButton
              variant="ghost"
              @click="closeAddModal"
            >
              Cancelar
            </UButton>
            <UButton
              color="primary"
              :disabled="!canSaveNewPrecio"
              :loading="savingNewPrice"
              @click="handleAddPrecio"
            >
              Agregar Precio
            </UButton>
          </div>
        </template>
    </UModal>

    <!-- Modal de confirmación para eliminar -->
    <UModal v-model:open="showDeleteModal">
        <template #header>
          <h3 class="text-lg font-semibold text-red-600">Eliminar Precio</h3>
        </template>

        <template #body>
          <div class="space-y-4">
            <p class="text-gray-600 dark:text-gray-400">
              ¿Estás seguro de que deseas eliminar el precio de 
              <strong>{{ precioToDelete?.lista_precio_nombre }}</strong>?
            </p>
            <p class="text-sm text-gray-500">
              Esta acción no se puede deshacer.
            </p>
          </div>
        </template>
        <template #footer>
          <div class="flex items-center justify-end gap-3">
            <UButton
              variant="ghost"
              @click="showDeleteModal = false"
            >
              Cancelar
            </UButton>
            <UButton
              color="red"
              :loading="deletingPrice !== null"
              @click="handleDeletePrecio"
            >
              Eliminar
            </UButton>
          </div>
        </template>
    </UModal>
  </div>
</template>

<script setup lang="ts">
import type { 
  ProductType, 
  PrecioListaDto, 
  ListaPrecioInfo 
} from '~/types/productos'
import PrecioField from './PrecioField.vue'

interface Props {
  productId: number
  productType?: ProductType
  precios?: PrecioListaDto[]
  readonly?: boolean
  loading?: boolean
  error?: string | null
}

interface Emits {
  'precio-updated': [listaId: number, precio: number]
  'precio-created': [listaId: number, precio: number]
  'precio-deleted': [listaId: number]
  'refetch': []
}

const props = withDefaults(defineProps<Props>(), {
  productType: 'base',
  precios: () => [],
  readonly: false,
  loading: false
})

const emit = defineEmits<Emits>()

// Composables
const { upsertPrecio, deletePrecio, parsePrecio, validatePrecio } = useProductoPrecios()
const { listas, fetchListas } = useListasPrecios()

// Estado reactivo
const showAddModal = ref(false)
const showDeleteModal = ref(false)
const savingNewPrice = ref(false)
const deletingPrice = ref<number | null>(null)
const precioToDelete = ref<PrecioListaDto | null>(null)

// Estado para nuevo precio
const newPrecio = reactive({
  listaId: undefined as number | undefined,
  precioInput: '',
  precio: 0
})
const newPrecioError = ref('')

// Computed
const displayPrecios = computed(() => {
  return props.precios || []
})

const listasConPrecio = computed(() => {
  return displayPrecios.value.map(p => p.lista_precio_id)
})

const listasDisponibles = computed(() => {
  const disponibles = listas.value.filter(lista => 
    !listasConPrecio.value.includes(lista.id)
  )
  return disponibles
})

const canAddPrice = computed(() => {
  return listasDisponibles.value.length > 0
})

const canSaveNewPrecio = computed(() => {
  return newPrecio.listaId && 
         newPrecio.precioInput.trim() && 
         !newPrecioError.value &&
         validatePrecio(parsePrecio(newPrecio.precioInput))
})

// Métodos
const getListaInfo = (listaId: number): ListaPrecioInfo | undefined => {
  return listas.value.find(lista => lista.id === listaId)
}

const isListaPredeterminada = (listaId: number): boolean => {
  const lista = getListaInfo(listaId)
  return lista?.es_predeterminada || false
}

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('es-AR')
}

const validateNewPrecio = () => {
  if (!newPrecio.precioInput.trim()) {
    newPrecioError.value = ''
    return
  }

  const precio = parsePrecio(newPrecio.precioInput)
  
  if (!validatePrecio(precio)) {
    newPrecioError.value = 'Precio inválido'
    return
  }
  
  if (precio < 0) {
    newPrecioError.value = 'El precio no puede ser negativo'
    return
  }
  
  newPrecioError.value = ''
  newPrecio.precio = precio
}

const handleUpdatePrecio = async (listaId: number, precio: number) => {
  try {
    await upsertPrecio(props.productId, listaId, precio, props.productType)
    emit('precio-updated', listaId, precio)
  } catch (error) {
    // Error ya manejado en el composable
    throw error
  }
}

const handleAddPrecio = async () => {
  if (!canSaveNewPrecio.value) return

  savingNewPrice.value = true
  try {
    // Parsear el precio correctamente antes de enviarlo
    const precioParseado = parsePrecio(newPrecio.precioInput)
    
    await upsertPrecio(
      props.productId, 
      newPrecio.listaId!, 
      precioParseado, 
      props.productType
    )
    
    emit('precio-created', newPrecio.listaId!, precioParseado)
    closeAddModal()
  } catch (error) {
    // Error ya manejado en el composable
  } finally {
    savingNewPrice.value = false
  }
}

const confirmDeletePrecio = (precio: PrecioListaDto) => {
  precioToDelete.value = precio
  showDeleteModal.value = true
}

const handleDeletePrecio = async () => {
  if (!precioToDelete.value) return

  deletingPrice.value = precioToDelete.value.lista_precio_id
  try {
    await deletePrecio(
      props.productId, 
      precioToDelete.value.lista_precio_id, 
      props.productType
    )
    
    emit('precio-deleted', precioToDelete.value.lista_precio_id)
    showDeleteModal.value = false
    precioToDelete.value = null
  } catch (error) {
    // Error ya manejado en el composable
  } finally {
    deletingPrice.value = null
  }
}

const closeAddModal = () => {
  showAddModal.value = false
  newPrecio.listaId = undefined
  newPrecio.precioInput = ''
  newPrecio.precio = 0
  newPrecioError.value = ''
}

const handlePrecioError = (error: string) => {
  console.error('Error en precio:', error)
}

const refetch = () => {
  emit('refetch')
}

// Actualizar precio localmente (para mantener sincronizado el estado)
const updateLocalPrecio = () => {
  // Este método está vacío porque PrecioField maneja su propio estado interno
  // y solo emite el evento 'save' cuando el usuario confirma el cambio
}

// Cargar listas cuando el componente se monta
onMounted(async () => {
  if (listas.value.length === 0) {
    try {
      await fetchListas()
    } catch (error) {
      console.error('Error al cargar listas:', error)
    }
  }
})
</script>