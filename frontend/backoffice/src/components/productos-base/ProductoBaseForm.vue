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
          Información Básica del Producto
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Código" name="codigo" required>
          <UInput 
            v-model.number="formData.codigo"
            type="number"
            placeholder="Código único del producto"
            :disabled="loading || mode === 'edit'"
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              {{ mode === 'edit' ? 'El código no se puede modificar' : 'Código único e irrepetible' }}
            </span>
          </template>
        </UFormField>
        
        <UFormField label="Descripción" name="descripcion" required>
          <UInput 
            v-model="formData.descripcion"
            placeholder="Descripción del producto"
            :disabled="loading"
          />
          <div v-if="isSyncField('descripcion')" class="flex items-center gap-2 mt-1">
            <UBadge color="orange" variant="soft" size="xs">SYNC</UBadge>
            <span class="text-xs text-orange-600">Campo sincronizado con SIGMA</span>
          </div>
        </UFormField>
        
        <UFormField label="Categoría" name="categoria_id">
          <USelectMenu
            v-model="formData.categoria_id"
            :items="categoriasDisponibles"
            label-key="label"
            value-key="value"
            placeholder="Selecciona una categoría"
            :loading="loadingCategorias"
            :disabled="loading"
            searchable
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              Selecciona la categoría que corresponde al producto
            </span>
          </template>
          <div v-if="isSyncField('codigo_rubro')" class="flex items-center gap-2 mt-1">
            <UBadge color="orange" variant="soft" size="xs">SYNC</UBadge>
            <span class="text-xs text-orange-600">Categoría sincronizada con Gecom</span>
          </div>
        </UFormField>
        
        
        <UFormField label="Stock/Existencia" name="existencia">
          <UInput 
            v-model.number="formData.existencia"
            type="number"
            placeholder="Cantidad en stock"
            :disabled="loading"
          />
          <div v-if="isSyncField('existencia')" class="flex items-center gap-2 mt-1">
            <UBadge color="orange" variant="soft" size="xs">SYNC</UBadge>
            <span class="text-xs text-orange-600">Campo sincronizado con SIGMA</span>
          </div>
        </UFormField>
        
        <UFormField label="Marca" name="marca">
          <UInput 
            v-model="formData.marca"
            placeholder="Marca del producto"
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Descripciones adicionales -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Descripciones Adicionales
        </h3>
      </template>
      
      <div class="space-y-4">
        <UFormField label="Descripción Corta" name="descripcion_corta">
          <UInput 
            v-model="formData.descripcion_corta"
            placeholder="Descripción breve para listados"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Descripción Larga" name="descripcion_larga">
          <UTextarea 
            v-model="formData.descripcion_larga"
            placeholder="Descripción detallada del producto"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Tags" name="tags">
          <UInput 
            v-model="formData.tags"
            placeholder="Etiquetas separadas por comas"
            :disabled="loading"
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              Separa las etiquetas con comas (ej: electrónica, gadget, móvil)
            </span>
          </template>
        </UFormField>
      </div>
    </UCard>

    <!-- Configuración y estado -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Configuración y Estado
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <UFormField label="Visible en catálogo" name="visible">
          <USwitch 
            v-model="formData.visible"
            :disabled="loading"
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              ¿El producto aparece en el catálogo público?
            </span>
          </template>
        </UFormField>
        
        <UFormField label="Producto destacado" name="destacado">
          <USwitch 
            v-model="formData.destacado"
            :disabled="loading"
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              ¿Se muestra como destacado?
            </span>
          </template>
        </UFormField>
        
        <UFormField label="Orden en categoría" name="orden_categoria">
          <UInput 
            v-model.number="formData.orden_categoria"
            type="number"
            placeholder="Orden de aparición"
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Gestión de Precios -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Gestión de Precios
          </h3>
          <div v-if="isSyncField('precios')" class="flex items-center gap-2">
            <UBadge color="orange" variant="soft" size="xs">SYNC</UBadge>
            <span class="text-xs text-orange-600">Precios sincronizados con Gecom</span>
          </div>
        </div>
      </template>
      
      <!-- Modo edición: lista completa de precios -->
      <PreciosList
        v-if="mode === 'edit' && initialData?.id"
        :product-id="initialData.id"
        product-type="base"
        :precios="productoPreciosData"
        :loading="loadingPrecios"
        :error="errorPrecios"
        @precio-updated="handlePrecioUpdated"
        @precio-created="handlePrecioCreated"
        @precio-deleted="handlePrecioDeleted"
        @refetch="fetchProductoPrecios"
      />
      
      <!-- Modo creación: formulario de precios iniciales -->
      <div v-else class="space-y-4">
        <p class="text-sm text-gray-500 dark:text-gray-400">
          Define los precios iniciales para cada lista. Podrás gestionar más precios después de crear el producto.
        </p>
        
        <div v-if="listasDisponibles.length > 0" class="space-y-3">
          <div
            v-for="lista in listasDisponibles"
            :key="lista.id"
            class="flex items-center gap-4 p-4 border border-gray-200 dark:border-gray-700 rounded-lg"
          >
            <div class="flex-1">
              <div class="flex items-center gap-2">
                <span class="font-medium text-gray-900 dark:text-gray-100">
                  {{ lista.nombre }}
                </span>
                <UBadge 
                  v-if="lista.es_predeterminada" 
                  size="xs" 
                  color="blue" 
                  variant="soft"
                >
                  Por defecto
                </UBadge>
              </div>
              <span v-if="lista.codigo" class="text-xs text-gray-500">
                {{ lista.codigo }}
              </span>
            </div>
            
            <div class="w-40">
              <UInput
                v-model.number="preciosIniciales[lista.id]"
                type="number"
                step="0.01"
                min="0"
                placeholder="0.00"
                :disabled="loading"
              >
                <template #leading>
                  <span class="text-gray-500">$</span>
                </template>
              </UInput>
            </div>
          </div>
        </div>
        
        <div v-else class="text-center py-8">
          <UIcon name="i-heroicons-currency-dollar" class="h-12 w-12 text-gray-400 mx-auto mb-2" />
          <p class="text-gray-500 dark:text-gray-400">
            No hay listas de precios disponibles
          </p>
        </div>
      </div>
    </UCard>

    <!-- Información adicional -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Información Adicional
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Código de Barras" name="codigo_barras">
          <UInput 
            v-model="formData.codigo_barras"
            placeholder="Código de barras del producto"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Unidad de Medida" name="unidad_medida">
          <UInput 
            v-model="formData.unidad_medida"
            placeholder="ej: unidad, kg, litro"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Imagen del Producto" name="imagen_url">
          <ImageUpload
            v-model="formData.imagen_url"
            :image-alt="formData.imagen_alt"
            :disabled="loading"
            :product-id="mode === 'edit' ? initialData?.id : undefined"
            product-type="base"
            url-field-name="imagen_url"
            @image-uploaded="onImageUploaded"
            @image-deleted="onImageDeleted"
            @error="onImageError"
          />
        </UFormField>
        
        <UFormField label="Texto alternativo de imagen" name="imagen_alt">
          <UInput 
            v-model="formData.imagen_alt"
            placeholder="Descripción de la imagen"
            :disabled="loading"
          />
        </UFormField>
      </div>
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
        {{ mode === 'create' ? 'Crear Producto' : 'Actualizar Producto' }}
      </UButton>
    </div>
  </UForm>

</template>

<script setup lang="ts">
import { z } from 'zod'
import type { ProductoBaseDto, CreateProductoBaseCommand, UpdateProductoBaseCommand, PrecioListaDto } from '~/types/productos'
import type { CategoryBaseDto } from '~/types/categorias'
import ImageUpload from '../ui/ImageUpload.vue'
import PreciosList from '../ui/PreciosList.vue'

interface Props {
  mode: 'create' | 'edit'
  initialData?: ProductoBaseDto
  loading?: boolean
}

interface Emits {
  submit: [data: CreateProductoBaseCommand | UpdateProductoBaseCommand]
  cancel: []
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const { fetchCategories, getCategoryOptions } = useCategoriesBase()

// Estado para categorías
const categoriasDisponibles = ref<{ label: string; value: number; codigo_rubro: number }[]>([])
const loadingCategorias = ref(false)

// Composables para precios
const { fetchProductoPrecios, upsertPrecio } = useProductoPrecios()
const { listas: todasLasListas, fetchListas } = useListasPrecios()

// Función helper para verificar si un campo es sincronizado
const isSyncField = (fieldName: string): boolean => {
  const syncFields = [
    'descripcion',
    'codigo_rubro',
    'precio',
    'precios',
    'existencia',
    'grupo1',
    'grupo2',
    'grupo3',
    'fechaAlta',
    'fechaModi',
    'imputable',
    'disponible',
    'codigoUbicacion'
  ]
  return syncFields.includes(fieldName.toLowerCase()) || syncFields.includes(fieldName)
}

// Schema de validación
const schema = z.object({
  codigo: z.number({ required_error: 'El código es requerido' })
    .min(1, 'El código debe ser mayor a 0'),
  descripcion: z.string({ required_error: 'La descripción es requerida' })
    .min(1, 'La descripción no puede estar vacía'),
  categoria_id: z.number({ required_error: 'Debes seleccionar una categoría' }),
  codigo_rubro: z.number().nullable().optional(),
  existencia: z.number().min(0, 'El stock no puede ser negativo').nullable().optional(),
  visible: z.boolean().optional(),
  destacado: z.boolean().optional(),
  orden_categoria: z.number().nullable().optional(),
  imagen_url: z.string().url('Debe ser una URL válida').nullable().optional().or(z.literal('')),
  imagen_alt: z.string().nullable().optional(),
  descripcion_corta: z.string().nullable().optional(),
  descripcion_larga: z.string().nullable().optional(),
  tags: z.string().nullable().optional(),
  codigo_barras: z.string().nullable().optional(),
  marca: z.string().nullable().optional(),
  unidad_medida: z.string().nullable().optional()
})

// Estado reactivo para toast
const toast = useToast()

// Estado para gestión de precios
const productoPreciosData = ref<PrecioListaDto[]>([])
const loadingPrecios = ref(false)
const errorPrecios = ref<string | null>(null)

// Estado para precios iniciales en modo create
const preciosIniciales = ref<Record<number, number>>({})
const listasDisponibles = ref<any[]>([])

// Helper para encontrar categoría por código de rubro
const findCategoryByCodigoRubro = (codigoRubro: number) => {
  if (!codigoRubro || categoriasDisponibles.value.length === 0) return null
  const category = categoriasDisponibles.value.find(cat => cat.codigo_rubro === codigoRubro)
  return category?.value || null
}

// Estado del formulario
const formData = reactive({
  codigo: props.initialData?.codigo || 0,
  descripcion: props.initialData?.descripcion || '',
  categoria_id: props.initialData?.codigo_rubro ? findCategoryByCodigoRubro(props.initialData.codigo_rubro) : null,
  codigo_rubro: props.initialData?.codigo_rubro || null,
  existencia: props.initialData?.existencia || null,
  visible: props.initialData?.visible ?? true,
  destacado: props.initialData?.destacado ?? false,
  orden_categoria: props.initialData?.orden_categoria || null,
  imagen_url: props.initialData?.imagen_url || '',
  imagen_alt: props.initialData?.imagen_alt || '',
  descripcion_corta: props.initialData?.descripcion_corta || '',
  descripcion_larga: props.initialData?.descripcion_larga || '',
  tags: props.initialData?.tags || '',
  codigo_barras: props.initialData?.codigo_barras || '',
  marca: props.initialData?.marca || '',
  unidad_medida: props.initialData?.unidad_medida || ''
})

// Métodos
const onSubmit = async (_: any) => {
  try {
    // Limpiar campos vacíos, pero preservar campos de imagen
    const cleanData = { ...formData }
    
    // Asignar codigo_rubro desde la categoría seleccionada
    if (cleanData.categoria_id) {
      const selectedCategory = categoriasDisponibles.value.find(cat => cat.value === cleanData.categoria_id)
      if (selectedCategory) {
        cleanData.codigo_rubro = selectedCategory.codigo_rubro
      }
    }
    
    Object.keys(cleanData).forEach(key => {
      const typedKey = key as keyof typeof cleanData
      // No convertir a null los campos de imagen si están vacíos
      if (cleanData[typedKey] === '' && !['imagen_url', 'imagen_alt'].includes(key)) {
        ;(cleanData as any)[typedKey] = null
      }
    })
    
    if (props.mode === 'create') {
      // Incluir precios iniciales en los datos
      const dataWithPrices = {
        ...cleanData,
        _preciosIniciales: preciosIniciales.value
      }
      emit('submit', dataWithPrices as any)
    } else {
      const updateData = { ...cleanData, id: props.initialData!.id }
      emit('submit', updateData as UpdateProductoBaseCommand)
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

const onImageUploaded = (url: string) => {
  formData.imagen_url = url
}

const onImageDeleted = () => {
  // Limpiar completamente el campo de imagen
  formData.imagen_url = ''
  
  // No mostrar toast aquí ya que el componente ImageUpload ya lo hace
}

const onImageError = (error: string) => {
  toast.add({
    title: 'Error con la imagen',
    description: error,
    color: 'red'
  })
}

// Actualizar datos cuando cambian las props
watch(() => props.initialData, (newData) => {
  if (newData) {
    Object.assign(formData, {
      codigo: newData.codigo || 0,
      descripcion: newData.descripcion || '',
      categoria_id: newData.codigo_rubro ? findCategoryByCodigoRubro(newData.codigo_rubro) : null,
      codigo_rubro: newData.codigo_rubro || null,
      existencia: newData.existencia || null,
      visible: newData.visible ?? true,
      destacado: newData.destacado ?? false,
      orden_categoria: newData.orden_categoria || null,
      imagen_url: newData.imagen_url || '',
      imagen_alt: newData.imagen_alt || '',
      descripcion_corta: newData.descripcion_corta || '',
      descripcion_larga: newData.descripcion_larga || '',
      tags: newData.tags || '',
      codigo_barras: newData.codigo_barras || '',
      marca: newData.marca || '',
      unidad_medida: newData.unidad_medida || ''
    })
  }
}, { immediate: true })

// Cargar precios cuando estamos en modo edición
const fetchProductoPreciosData = async () => {
  if (props.mode === 'edit' && props.initialData?.id) {
    loadingPrecios.value = true
    errorPrecios.value = null
    try {
      const precios = await fetchProductoPrecios(props.initialData.id, 'base')
      productoPreciosData.value = precios
    } catch (error) {
      errorPrecios.value = 'Error al cargar precios'
      console.error('Error loading precios:', error)
    } finally {
      loadingPrecios.value = false
    }
  }
}

// Handlers para eventos de precios
const handlePrecioUpdated = (listaId: number, precio: number) => {
  const index = productoPreciosData.value.findIndex(p => p.lista_precio_id === listaId)
  if (index !== -1) {
    productoPreciosData.value[index] = {
      ...productoPreciosData.value[index],
      precio,
      ultima_actualizacion: new Date().toISOString()
    }
  }
}

const handlePrecioCreated = (listaId: number, precio: number) => {
  // Refrescar precios después de crear
  fetchProductoPreciosData()
}

const handlePrecioDeleted = (listaId: number) => {
  productoPreciosData.value = productoPreciosData.value.filter(p => p.lista_precio_id !== listaId)
}

// Cargar precios al inicializar en modo edición
watch(() => [props.mode, props.initialData?.id], () => {
  if (props.mode === 'edit' && props.initialData?.id) {
    fetchProductoPreciosData()
  }
}, { immediate: true })

// Cargar categorías disponibles
const loadCategorias = async () => {
  loadingCategorias.value = true
  try {
    await fetchCategories({ visibleOnly: true })
    categoriasDisponibles.value = getCategoryOptions()
  } catch (error) {
    console.error('Error loading categorías:', error)
  } finally {
    loadingCategorias.value = false
  }
}

// Cargar listas disponibles al montar el componente
onMounted(async () => {
  try {
    await Promise.all([
      fetchListas(),
      loadCategorias()
    ])
    listasDisponibles.value = todasLasListas.value
  } catch (error) {
    console.error('Error loading data:', error)
  }
})

// Computed para obtener las listas disponibles reactivamente
watchEffect(() => {
  listasDisponibles.value = todasLasListas.value
})

// Watcher para actualizar categoria_id cuando las categorías estén cargadas
watch([() => categoriasDisponibles.value, () => props.initialData?.codigo_rubro], ([categorias, codigoRubro]) => {
  if (categorias.length > 0 && codigoRubro && !formData.categoria_id) {
    formData.categoria_id = findCategoryByCodigoRubro(codigoRubro)
  }
}, { immediate: true })
</script>