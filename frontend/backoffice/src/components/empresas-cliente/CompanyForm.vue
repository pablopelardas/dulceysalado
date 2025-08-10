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
          Información Básica
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Código" name="codigo" required>
          <UInput 
            v-model="formData.codigo"
            placeholder="Ej: CLI-001"
            :disabled="loading"
            help="Código único para identificar la empresa"
          />
        </UFormField>
        
        <UFormField label="Nombre" name="nombre" required>
          <UInput 
            v-model="formData.nombre"
            placeholder="Nombre de la empresa"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Razón Social" name="razon_social" class="md:col-span-2">
          <UInput 
            v-model="formData.razon_social"
            placeholder="Razón social completa"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="CUIT" name="cuit">
          <UInput 
            v-model="formData.cuit"
            placeholder="XX-XXXXXXXX-X"
            :disabled="loading"
            @input="formatCUIT"
            maxlength="13"
          />
        </UFormField>
        
        <UFormField label="Email" name="email">
          <UInput 
            v-model="formData.email"
            type="email"
            placeholder="contacto@empresa.com"
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Información de contacto -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Información de Contacto
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Teléfono" name="telefono">
          <UInput 
            v-model="formData.telefono"
            placeholder="+54 11 1234-5678"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Dirección" name="direccion" class="md:col-span-2">
          <UInput 
            v-model="formData.direccion"
            placeholder="Dirección completa"
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Configuración del sistema -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Configuración del Sistema
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Dominio Personalizado" name="dominio_personalizado" required>
          <UInput 
            v-model="formData.dominio_personalizado"
            placeholder="mi-empresa"
            :disabled="loading"
            help="Solo letras, números y guiones. Mínimo 3 caracteres"
            @input="formatDomain"
          />
        </UFormField>
        
        <UFormField label="Fecha de Vencimiento" name="fecha_vencimiento">
          <UInput 
            v-model="formData.fecha_vencimiento"
            type="date"
            :disabled="loading"
            help="Opcional - Fecha límite del servicio"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Permisos del sistema -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Permisos y Configuraciones
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div class="space-y-4">
          <h4 class="font-medium text-gray-900 dark:text-gray-100">Gestión de Contenido</h4>
          
          <UFormField name="puede_agregar_productos">
            <UCheckbox 
              v-model="formData.puede_agregar_productos"
              label="Puede agregar productos"
              :disabled="loading"
            />
          </UFormField>
          
          <UFormField name="puede_agregar_categorias">
            <UCheckbox 
              v-model="formData.puede_agregar_categorias"
              label="Puede agregar categorías"
              :disabled="loading"
            />
          </UFormField>
        </div>
        
        <div class="space-y-4">
          <h4 class="font-medium text-gray-900 dark:text-gray-100">Configuración de Catálogo</h4>
          
          <UFormField name="mostrar_precios">
            <UCheckbox 
              v-model="formData.mostrar_precios"
              label="Mostrar precios en catálogo"
              :disabled="loading"
            />
          </UFormField>
          
          <UFormField name="mostrar_stock">
            <UCheckbox 
              v-model="formData.mostrar_stock"
              label="Mostrar stock disponible"
              :disabled="loading"
            />
          </UFormField>
          
          <UFormField name="permitir_pedidos">
            <UCheckbox 
              v-model="formData.permitir_pedidos"
              label="Permitir realizar pedidos"
              :disabled="loading"
            />
          </UFormField>
        </div>
      </div>
      
      <!-- Lista de Precios Predeterminada -->
      <div class="mt-6 pt-6 border-t border-gray-200 dark:border-gray-700">
        <UFormField label="Lista de Precios Predeterminada" name="lista_precio_predeterminada_id">
          <USelect
            v-model="formData.lista_precio_predeterminada_id"
            :items="listasPreciosOptions"
            :disabled="loading || loadingListas"
            placeholder="Selecciona una lista de precios"
            help="Lista de precios que se usará por defecto en el catálogo"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Personalización Visual -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Personalización Visual
          </h3>
          <span class="text-sm text-gray-500 dark:text-gray-400">Opcional</span>
        </div>
      </template>
      
      <div class="space-y-6">
        <!-- Logos -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <UFormField label="Logo URL" name="logo_url">
            <UInput 
              v-model="formData.logo_url"
              placeholder="https://ejemplo.com/logo.png"
              :disabled="loading"
              help="URL del logo de la empresa (PNG, JPG, SVG)"
            />
          </UFormField>
          
          <UFormField label="Favicon URL" name="favicon_url">
            <UInput 
              v-model="formData.favicon_url"
              placeholder="https://ejemplo.com/favicon.ico"
              :disabled="loading"
              help="URL del favicon (ICO, PNG 16x16 o 32x32)"
            />
          </UFormField>
        </div>
        
        <!-- Colores del Tema -->
        <div>
          <h4 class="font-medium text-gray-900 dark:text-gray-100 mb-4">Colores del Tema</h4>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <UFormField label="Color Primario" name="colores_tema.primario">
              <div class="flex gap-2">
                <UInput 
                  v-model="formData.colores_tema!.primario"
                  placeholder="#007bff"
                  :disabled="loading"
                  class="flex-1"
                />
                <input 
                  type="color" 
                  v-model="formData.colores_tema!.primario"
                  :disabled="loading"
                  class="w-12 h-10 rounded border border-gray-300 dark:border-gray-600 cursor-pointer"
                  title="Seleccionar color primario"
                />
              </div>
            </UFormField>
            
            <UFormField label="Color Secundario" name="colores_tema.secundario">
              <div class="flex gap-2">
                <UInput 
                  v-model="formData.colores_tema!.secundario"
                  placeholder="#6c757d"
                  :disabled="loading"
                  class="flex-1"
                />
                <input 
                  type="color" 
                  v-model="formData.colores_tema!.secundario"
                  :disabled="loading"
                  class="w-12 h-10 rounded border border-gray-300 dark:border-gray-600 cursor-pointer"
                  title="Seleccionar color secundario"
                />
              </div>
            </UFormField>
            
            <UFormField label="Color de Acento" name="colores_tema.acento">
              <div class="flex gap-2">
                <UInput 
                  v-model="formData.colores_tema!.acento"
                  placeholder="#28a745"
                  :disabled="loading"
                  class="flex-1"
                />
                <input 
                  type="color" 
                  v-model="formData.colores_tema!.acento"
                  :disabled="loading"
                  class="w-12 h-10 rounded border border-gray-300 dark:border-gray-600 cursor-pointer"
                  title="Seleccionar color de acento"
                />
              </div>
            </UFormField>
          </div>
          
          <!-- Preview de colores -->
          <div class="mt-4 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <h5 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Vista Previa</h5>
            <div class="flex gap-3">
              <div class="flex items-center gap-2">
                <div 
                  class="w-6 h-6 rounded border border-gray-300"
                  :style="{ backgroundColor: formData.colores_tema?.primario }"
                ></div>
                <span class="text-sm text-gray-600 dark:text-gray-400">Primario</span>
              </div>
              <div class="flex items-center gap-2">
                <div 
                  class="w-6 h-6 rounded border border-gray-300"
                  :style="{ backgroundColor: formData.colores_tema?.secundario }"
                ></div>
                <span class="text-sm text-gray-600 dark:text-gray-400">Secundario</span>
              </div>
              <div class="flex items-center gap-2">
                <div 
                  class="w-6 h-6 rounded border border-gray-300"
                  :style="{ backgroundColor: formData.colores_tema?.acento }"
                ></div>
                <span class="text-sm text-gray-600 dark:text-gray-400">Acento</span>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Configuración de catálogo -->
        <div class="mt-6">
          <h4 class="font-medium text-gray-900 dark:text-gray-100 mb-4">Configuración de Catálogo</h4>
          <UFormField label="Productos por página" name="productos_por_pagina">
            <USelect
              v-model="formData.productos_por_pagina"
              :items="productosPorPaginaOptions"
              :disabled="loading"
              help="Cantidad de productos a mostrar por página en el catálogo"
            />
          </UFormField>
        </div>
      </div>
    </UCard>

    <!-- Redes sociales (opcional) -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Redes Sociales
          </h3>
          <span class="text-sm text-gray-500 dark:text-gray-400">Opcional</span>
        </div>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <UFormField label="WhatsApp" name="url_whatsapp">
          <UInput 
            v-model="formData.url_whatsapp"
            placeholder="https://wa.me/..."
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Facebook" name="url_facebook">
          <UInput 
            v-model="formData.url_facebook"
            placeholder="https://facebook.com/..."
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Instagram" name="url_instagram">
          <UInput 
            v-model="formData.url_instagram"
            placeholder="https://instagram.com/..."
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Botones de acción -->
    <div class="flex flex-col sm:flex-row justify-end gap-3 pt-6">
      <UButton
        variant="ghost"
        color="gray"
        @click="$emit('cancel')"
        :disabled="loading"
      >
        Cancelar
      </UButton>
      
      <UButton 
        type="submit"
        color="primary"
        :loading="loading"
        :disabled="!isFormValid"
      >
        {{ mode === 'create' ? 'Crear Empresa' : 'Actualizar Empresa' }}
      </UButton>
    </div>
  </UForm>
</template>

<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'
import type { CreateCompanyRequest, UpdateCompanyRequest } from '~/types/auth'

interface Props {
  mode: 'create' | 'edit'
  loading?: boolean
  initialData?: Partial<CreateCompanyRequest>
}

interface Emits {
  submit: [data: CreateCompanyRequest | UpdateCompanyRequest]
  cancel: []
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  initialData: () => ({})
})

const emit = defineEmits<Emits>()

// Composables para listas de precios
const { listas: listasPrecios, loading: loadingListas, fetchListas } = useListasPrecios()

// Schema de validación
const schema = z.object({
  codigo: z.string().min(2, 'El código debe tener al menos 2 caracteres'),
  nombre: z.string().min(2, 'El nombre debe tener al menos 2 caracteres'),
  razon_social: z.string().optional(),
  cuit: z.string()
    .optional()
    .refine((val) => {
      if (!val || val === '') return true
      return /^\d{2}-\d{8}-\d{1}$/.test(val)
    }, 'CUIT debe tener formato XX-XXXXXXXX-X'),
  telefono: z.string().optional(),
  email: z.union([
    z.string().email('Email inválido'),
    z.literal('')
  ]).optional(),
  direccion: z.string().optional(),
  dominio_personalizado: z.string()
    .min(3, 'El dominio debe tener al menos 3 caracteres')
    .regex(/^[a-z0-9]([a-z0-9-]*[a-z0-9])?$/, 'Formato de dominio inválido. Solo letras, números y guiones'),
  fecha_vencimiento: z.string().optional(),
  logo_url: z.union([
    z.string().url('URL inválida'),
    z.literal('')
  ]).optional(),
  favicon_url: z.union([
    z.string().url('URL inválida'),
    z.literal('')
  ]).optional(),
  colores_tema: z.object({
    primario: z.union([
      z.string().regex(/^#[0-9A-Fa-f]{6}$/, 'Color debe ser formato hexadecimal #RRGGBB'),
      z.literal('')
    ]).optional(),
    secundario: z.union([
      z.string().regex(/^#[0-9A-Fa-f]{6}$/, 'Color debe ser formato hexadecimal #RRGGBB'),
      z.literal('')
    ]).optional(),
    acento: z.union([
      z.string().regex(/^#[0-9A-Fa-f]{6}$/, 'Color debe ser formato hexadecimal #RRGGBB'),
      z.literal('')
    ]).optional()
  }).optional(),
  puede_agregar_productos: z.boolean(),
  puede_agregar_categorias: z.boolean(),
  mostrar_precios: z.boolean(),
  mostrar_stock: z.boolean(),
  permitir_pedidos: z.boolean(),
  productos_por_pagina: z.number(),
  url_whatsapp: z.union([
    z.string().url('URL inválida'),
    z.literal('')
  ]).optional(),
  url_facebook: z.union([
    z.string().url('URL inválida'),
    z.literal('')
  ]).optional(),
  url_instagram: z.union([
    z.string().url('URL inválida'),
    z.literal('')
  ]).optional(),
  lista_precio_predeterminada_id: z.number().nullable().optional()
})

// Estado del formulario
const formData = reactive<CreateCompanyRequest>({
  codigo: '',
  nombre: '',
  razon_social: '',
  cuit: '',
  telefono: '',
  email: '',
  direccion: '',
  dominio_personalizado: '',
  fecha_vencimiento: '',
  logo_url: '',
  favicon_url: '',
  colores_tema: {
    primario: '#007bff',
    secundario: '#6c757d',
    acento: '#28a745'
  },
  puede_agregar_productos: false,
  puede_agregar_categorias: false,
  mostrar_precios: true,
  mostrar_stock: false,
  permitir_pedidos: false,
  productos_por_pagina: 20,
  url_whatsapp: '',
  url_facebook: '',
  url_instagram: '',
  lista_precio_predeterminada_id: null,
  requesting_user_id: 0,
  ...props.initialData
})

// Opciones para select
const productosPorPaginaOptions = [
  { label: '6 productos', value: 6 },
  { label: '12 productos', value: 12 },
  { label: '20 productos', value: 20 },
  { label: '30 productos', value: 30 },
  { label: '50 productos', value: 50 },
  { label: '100 productos', value: 100 }
]

// Opciones para listas de precios
const listasPreciosOptions = computed(() => {
  const options = listasPrecios.value.map(lista => ({
    label: lista.nombre,
    value: lista.id
  }))
  
  // Agregar opción para "Sin lista predeterminada"
  return [
    { label: 'Sin lista predeterminada', value: null },
    ...options
  ]
})

// Computed
const isFormValid = computed(() => {
  const isBasicValid = formData.codigo.trim() !== '' && 
                      formData.nombre.trim() !== '' && 
                      formData.dominio_personalizado.trim() !== ''
  
  // Validar dominio personalizado
  const isDomainValid = formData.dominio_personalizado.length >= 3 && 
                       /^[a-z0-9]([a-z0-9-]*[a-z0-9])?$/.test(formData.dominio_personalizado)
  
  // Validar CUIT si está presente
  const isCuitValid = !formData.cuit || formData.cuit === '' || 
                     /^\d{2}-\d{8}-\d{1}$/.test(formData.cuit)
  
  return isBasicValid && isDomainValid && isCuitValid
})

// Referencia al formulario
const form = ref()

// Métodos
const onSubmit = async (event: FormSubmitEvent<Record<string, any>>) => {
  const cleanData = { ...event.data }

  // Limpiar campos vacíos opcionales
  Object.keys(cleanData).forEach(key => {
    if (typeof cleanData[key as keyof typeof cleanData] === 'string' &&
        cleanData[key as keyof typeof cleanData] === '') {
      delete cleanData[key as keyof typeof cleanData]
    }
  })

  // Asegurarse de incluir requesting_user_id si está en formData
  if ('requesting_user_id' in formData) {
    cleanData.requesting_user_id = formData.requesting_user_id
  }

  // Si es modo edición, asegurar company_id y requesting_user_id
  if (props.mode === 'edit' && props.initialData && 'company_id' in props.initialData) {
    cleanData.company_id = props.initialData.company_id
    if ('requesting_user_id' in props.initialData) {
      cleanData.requesting_user_id = props.initialData.requesting_user_id
    }
  }

  emit('submit', cleanData as CreateCompanyRequest | UpdateCompanyRequest)
}

const onError = (errors: any) => {
  console.warn('Errores de validación:', errors)
}

// Funciones de formateo
const formatCUIT = (event: Event) => {
  const input = event.target as HTMLInputElement
  let value = input.value.replace(/\D/g, '') // Solo números
  
  if (value.length >= 2) {
    value = value.substring(0, 2) + '-' + value.substring(2)
  }
  if (value.length >= 12) {
    value = value.substring(0, 11) + '-' + value.substring(11, 12)
  }
  
  formData.cuit = value.substring(0, 13) // XX-XXXXXXXX-X (13 caracteres máximo)
}

const formatDomain = (event: Event) => {
  const input = event.target as HTMLInputElement
  let value = input.value
    .toLowerCase()
    .replace(/[^a-z0-9-]/g, '') // Solo letras, números y guiones
    .replace(/^-+|-+$/g, '') // Remover guiones al inicio y final
    .replace(/-+/g, '-') // Convertir múltiples guiones en uno solo
  
  formData.dominio_personalizado = value
}

// Inicializar datos si se está editando
onMounted(async () => {
  // Cargar listas de precios
  await fetchListas()
  
  if (props.mode === 'edit' && props.initialData) {
    Object.assign(formData, props.initialData)
  }
})
</script>