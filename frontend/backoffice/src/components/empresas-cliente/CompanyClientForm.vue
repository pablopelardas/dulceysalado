<template>
  <UForm 
    ref="form"
    :schema="schema" 
    :state="formData"
    @submit="onSubmit"
    @error="onError"
    class="space-y-6"
  >
    <!-- Información básica - Solo lectura -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Información Básica
          </h3>
          <UBadge color="yellow" variant="soft">Solo lectura</UBadge>
        </div>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Código">
          <UInput 
            :model-value="formData.codigo"
            disabled
            help="Este campo es administrado por la empresa principal"
          />
        </UFormField>
        
        <UFormField label="Razón Social">
          <UInput 
            :model-value="formData.razon_social"
            disabled
            help="Este campo es administrado por la empresa principal"
          />
        </UFormField>
        
        <UFormField label="CUIT" class="md:col-span-2">
          <UInput 
            :model-value="formData.cuit"
            disabled
            help="Este campo es administrado por la empresa principal"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Información de contacto - Editable -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Información de Contacto
          </h3>
          <UBadge color="green" variant="soft">Editable</UBadge>
        </div>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Nombre de la Empresa" name="nombre" required>
          <UInput 
            v-model="formData.nombre"
            placeholder="Nombre de la empresa"
            :disabled="loading"
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
        
        <UFormField label="Teléfono" name="telefono">
          <UInput 
            v-model="formData.telefono"
            placeholder="+54 11 1234-5678"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Dirección" name="direccion" class="md:col-span-1">
          <UInput 
            v-model="formData.direccion"
            placeholder="Dirección completa"
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Configuración del sistema (bloqueada) -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Configuración del Sistema
          </h3>
          <UBadge color="yellow" variant="soft">Solo lectura</UBadge>
        </div>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Dominio Personalizado">
          <UInput 
            :model-value="formData.dominio_personalizado"
            disabled
            help="Este campo es administrado por la empresa principal"
          />
        </UFormField>
        
        <UFormField label="Fecha de Vencimiento">
          <UInput 
            :model-value="formData.fecha_vencimiento ? new Date(formData.fecha_vencimiento).toLocaleDateString() : 'Sin vencimiento'"
            disabled
            help="Administrado por la empresa principal"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Permisos del sistema (bloqueados) -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Permisos y Configuraciones
          </h3>
          <UBadge color="yellow" variant="soft">Solo lectura</UBadge>
        </div>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div class="space-y-4">
          <h4 class="font-medium text-gray-900 dark:text-gray-100">Gestión de Contenido</h4>
          
          <UFormField>
            <UCheckbox 
              :model-value="formData.puede_agregar_productos"
              label="Puede agregar productos"
              disabled
            />
          </UFormField>
          
          <UFormField>
            <UCheckbox 
              :model-value="formData.puede_agregar_categorias"
              label="Puede agregar categorías"
              disabled
            />
          </UFormField>
        </div>
        
        <div class="space-y-4">
          <h4 class="font-medium text-gray-900 dark:text-gray-100">Configuración de Catálogo</h4>
          
          <UFormField>
            <UCheckbox 
              :model-value="formData.mostrar_precios"
              label="Mostrar precios en catálogo"
              disabled
            />
          </UFormField>
          
          <UFormField>
            <UCheckbox 
              :model-value="formData.mostrar_stock"
              label="Mostrar stock disponible"
              disabled
            />
          </UFormField>
          
          <UFormField>
            <UCheckbox 
              :model-value="formData.permitir_pedidos"
              label="Permitir realizar pedidos"
              disabled
            />
          </UFormField>
        </div>
      </div>
      
    </UCard>

    <!-- Imágenes Corporativas (editable) -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Imágenes Corporativas
          </h3>
          <UBadge color="green" variant="soft">Editable</UBadge>
        </div>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Logo de la Empresa" name="logo_url">
          <ImageUpload
            v-model="formData.logo_url"
            :company-id="initialData?.id"
            company-image-type="logo"
            :disabled="loading"
            url-label="URL del Logo"
            file-label="Subir Logo"
            url-placeholder="https://ejemplo.com/logo.png"
            url-field-name="logo_url"
            image-alt="Logo de la empresa"
            @image-uploaded="onImageUploaded"
            @image-deleted="onImageDeleted"
            @error="onImageError"
          />
        </UFormField>
        
        <UFormField label="Favicon" name="favicon_url">
          <ImageUpload
            v-model="formData.favicon_url"
            :company-id="initialData?.id"
            company-image-type="favicon"
            :disabled="loading"
            url-label="URL del Favicon"
            file-label="Subir Favicon"
            url-placeholder="https://ejemplo.com/favicon.ico"
            url-field-name="favicon_url"
            image-alt="Favicon"
            @image-uploaded="onImageUploaded"
            @image-deleted="onImageDeleted"
            @error="onImageError"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Personalización Visual (editable) -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Personalización Visual
          </h3>
          <UBadge color="green" variant="soft">Editable</UBadge>
        </div>
      </template>
      
      <div class="space-y-6">        
        <!-- Colores del Tema -->
        <div>
          <h4 class="font-medium text-gray-900 dark:text-gray-100 mb-4">Colores del Tema</h4>
          
          <!-- Selector de paletas predefinidas -->
          <div class="mb-6">
            <UFormField label="Paletas Predefinidas">
              <USelect
                v-model="selectedPalette"
                :items="colorPalettes"
                placeholder="Selecciona una paleta de colores"
                :disabled="loading"
                @change="applyPalette"
                help="Elige una paleta predefinida o personaliza los colores manualmente"
              />
            </UFormField>
            
            <!-- Grid de paletas visuales para selección rápida -->
            <div class="mt-4 grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-6 gap-3">
              <div
                v-for="palette in colorPalettes"
                :key="palette.value"
                class="cursor-pointer p-3 rounded-lg border transition-all hover:shadow-md"
                :class="[
                  selectedPalette === palette.value 
                    ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20' 
                    : 'border-gray-200 dark:border-gray-700 hover:border-gray-300'
                ]"
                @click="selectPalette(palette.value)"
              >
                <div class="text-xs font-medium text-gray-700 dark:text-gray-300 mb-2 text-center">
                  {{ palette.label }}
                </div>
                <div class="flex gap-1 justify-center">
                  <div 
                    class="w-4 h-4 rounded-full border border-gray-300"
                    :style="{ backgroundColor: palette.colors.primario }"
                    :title="`Primario: ${palette.colors.primario}`"
                  ></div>
                  <div 
                    class="w-4 h-4 rounded-full border border-gray-300"
                    :style="{ backgroundColor: palette.colors.secundario }"
                    :title="`Secundario: ${palette.colors.secundario}`"
                  ></div>
                  <div 
                    class="w-4 h-4 rounded-full border border-gray-300"
                    :style="{ backgroundColor: palette.colors.acento }"
                    :title="`Acento: ${palette.colors.acento}`"
                  ></div>
                </div>
              </div>
            </div>
          </div>
          
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <UFormField label="Color Primario" name="colores_tema.primario">
              <div class="flex gap-2">
                <UInput 
                  v-model="formData.colores_tema.primario"
                  placeholder="#007bff"
                  :disabled="loading"
                  class="flex-1"
                />
                <input 
                  type="color" 
                  v-model="formData.colores_tema.primario"
                  :disabled="loading"
                  class="w-12 h-10 rounded border border-gray-300 dark:border-gray-600 cursor-pointer"
                  title="Seleccionar color primario"
                />
              </div>
            </UFormField>
            
            <UFormField label="Color Secundario" name="colores_tema.secundario">
              <div class="flex gap-2">
                <UInput 
                  v-model="formData.colores_tema.secundario"
                  placeholder="#6c757d"
                  :disabled="loading"
                  class="flex-1"
                />
                <input 
                  type="color" 
                  v-model="formData.colores_tema.secundario"
                  :disabled="loading"
                  class="w-12 h-10 rounded border border-gray-300 dark:border-gray-600 cursor-pointer"
                  title="Seleccionar color secundario"
                />
              </div>
            </UFormField>
            
            <UFormField label="Color de Acento" name="colores_tema.acento">
              <div class="flex gap-2">
                <UInput 
                  v-model="formData.colores_tema.acento"
                  placeholder="#28a745"
                  :disabled="loading"
                  class="flex-1"
                />
                <input 
                  type="color" 
                  v-model="formData.colores_tema.acento"
                  :disabled="loading"
                  class="w-12 h-10 rounded border border-gray-300 dark:border-gray-600 cursor-pointer"
                  title="Seleccionar color de acento"
                />
              </div>
            </UFormField>
          </div>
          
          <!-- Preview de colores -->
          <div class="mt-4 p-4 bg-gray-50 dark:bg-gray-800 rounded-lg">
            <h5 class="text-sm font-medium text-gray-700 dark:text-gray-300 mb-4">Vista Previa de Colores</h5>
            
            <!-- Colores individuales -->
            <div class="flex gap-3 mb-6">
              <div class="flex items-center gap-2">
                <div 
                  class="w-6 h-6 rounded border border-gray-300"
                  :style="{ backgroundColor: formData.colores_tema.primario }"
                ></div>
                <span class="text-sm text-gray-600 dark:text-gray-400">Primario</span>
              </div>
              <div class="flex items-center gap-2">
                <div 
                  class="w-6 h-6 rounded border border-gray-300"
                  :style="{ backgroundColor: formData.colores_tema.secundario }"
                ></div>
                <span class="text-sm text-gray-600 dark:text-gray-400">Secundario</span>
              </div>
              <div class="flex items-center gap-2">
                <div 
                  class="w-6 h-6 rounded border border-gray-300"
                  :style="{ backgroundColor: formData.colores_tema.acento }"
                ></div>
                <span class="text-sm text-gray-600 dark:text-gray-400">Acento</span>
              </div>
            </div>

            <!-- Preview de página simulada (basado en el diseño real) -->
            <div class="border border-gray-200 dark:border-gray-600 rounded-lg overflow-hidden relative">
              <!-- Fondo principal con color primario -->
              <div 
                class="p-6 min-h-[300px]"
                :style="{ backgroundColor: formData.colores_tema.primario }"
              >
                <!-- Header con logo y nombre -->
                <div class="flex items-center gap-3 mb-4">
                  <div 
                    class="w-10 h-10 rounded-full flex items-center justify-center text-white font-bold text-lg"
                    :style="{ backgroundColor: formData.colores_tema.acento }"
                  >
                    {{ (formData.nombre || 'Tu Empresa').charAt(0).toUpperCase() }}
                  </div>
                  <div>
                    <div class="font-semibold text-sm text-white">
                      {{ formData.nombre || 'Tu Empresa' }}
                    </div>
                    <div class="text-xs text-white opacity-80">
                      Tu catálogo online confiable
                    </div>
                  </div>
                </div>

                <!-- Barra de búsqueda -->
                <div class="mb-4">
                  <input 
                    type="text" 
                    placeholder="¿Qué estás buscando hoy?"
                    class="w-full px-3 py-2 rounded-lg text-sm bg-white text-gray-700"
                    disabled
                  />
                </div>

                <!-- Pills de categorías -->
                <div class="flex flex-wrap gap-2 mb-4">
                  <span class="px-3 py-1 rounded-full text-xs font-medium bg-white bg-opacity-20 text-white">
                    Todos (128)
                  </span>
                  <span 
                    class="px-3 py-1 rounded-full text-xs font-medium text-white"
                    :style="{ backgroundColor: formData.colores_tema.secundario }"
                  >
                    Categoría 1 (45)
                  </span>
                  <span class="px-3 py-1 rounded-full text-xs font-medium bg-white bg-opacity-20 text-white">
                    Categoría 2 (32)
                  </span>
                </div>

                <!-- Contador de productos -->
                <div class="text-sm font-medium mb-4 text-white">
                  128 productos
                </div>

                <!-- Grid de productos -->
                <div class="grid grid-cols-2 gap-3">
                  <div class="bg-white rounded-lg p-3 shadow-sm">
                    <div class="w-full h-20 bg-gray-100 rounded mb-2 flex items-center justify-center">
                      <svg class="w-8 h-8 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                        <path fill-rule="evenodd" d="M4 3a2 2 0 00-2 2v1.586l8 8 8-8V5a2 2 0 00-2-2H4zm8 9.414L4 4.414V13a2 2 0 002 2h8a2 2 0 002-2V4.414L12 12.414z" clip-rule="evenodd"></path>
                      </svg>
                    </div>
                    <div class="text-xs font-medium text-gray-800 mb-1">PRODUCTO EJEMPLO 1</div>
                    <div class="text-xs font-bold text-gray-900">$1,250</div>
                  </div>
                  
                  <div class="bg-white rounded-lg p-3 shadow-sm">
                    <div class="w-full h-20 bg-gray-100 rounded mb-2 flex items-center justify-center">
                      <svg class="w-8 h-8 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                        <path fill-rule="evenodd" d="M4 3a2 2 0 00-2 2v1.586l8 8 8-8V5a2 2 0 00-2-2H4zm8 9.414L4 4.414V13a2 2 0 002 2h8a2 2 0 002-2V4.414L12 12.414z" clip-rule="evenodd"></path>
                      </svg>
                    </div>
                    <div class="text-xs font-medium text-gray-800 mb-1">PRODUCTO EJEMPLO 2</div>
                    <div class="text-xs font-bold text-gray-900">$890</div>
                  </div>
                </div>
              </div>

              <!-- Botón de WhatsApp flotante -->
              <div class="absolute bottom-3 right-3">
                <div 
                  class="w-8 h-8 rounded-full flex items-center justify-center text-white shadow-lg"
                  :style="{ backgroundColor: formData.colores_tema.acento }"
                >
                  <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893A11.821 11.821 0 0020.885 3.108"/>
                  </svg>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Configuración de catálogo -->
        <div class="mt-6">
          <h4 class="font-medium text-gray-900 dark:text-gray-100 mb-4">Configuración de Catálogo</h4>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <UFormField label="Productos por página" name="productos_por_pagina">
              <USelect
                v-model="formData.productos_por_pagina"
                :items="productosPorPaginaOptions"
                :disabled="loading"
                help="Cantidad de productos a mostrar por página en el catálogo"
              />
            </UFormField>
            
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
        </div>
      </div>
    </UCard>

    <!-- Redes sociales (editable) -->
    <UCard>
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Redes Sociales
          </h3>
          <UBadge color="green" variant="soft">Editable</UBadge>
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
        Actualizar Configuración
      </UButton>
    </div>
  </UForm>
</template>

<script setup lang="ts">
import { z } from 'zod'
import type { FormSubmitEvent } from '#ui/types'
import ImageUpload from '~/components/ui/ImageUpload.vue'

interface ClientUpdateRequest {
  nombre: string
  email?: string
  telefono?: string
  direccion?: string
  logo_url?: string
  favicon_url?: string
  colores_tema?: {
    primario?: string
    secundario?: string
    acento?: string
  }
  productos_por_pagina?: number
  url_whatsapp?: string
  url_facebook?: string
  url_instagram?: string
  lista_precio_predeterminada_id?: number | null
}

interface Props {
  loading?: boolean
  initialData?: any
}

interface Emits {
  submit: [data: ClientUpdateRequest]
  cancel: []
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  initialData: () => ({})
})

const emit = defineEmits<Emits>()

// Composables para listas de precios
const { listas: listasPrecios, loading: loadingListas, fetchListas } = useListasPrecios()

// Schema de validación (solo campos editables)
const schema = z.object({
  nombre: z.string().min(2, 'El nombre debe tener al menos 2 caracteres'),
  email: z.string().nullable().optional(),
  telefono: z.string().nullable().optional(),
  direccion: z.string().nullable().optional(),
  logo_url: z.string().nullable().optional(),
  favicon_url: z.string().nullable().optional(),
  productos_por_pagina: z.number().optional(),
  url_whatsapp: z.string().nullable().optional(),
  url_facebook: z.string().nullable().optional(),
  url_instagram: z.string().nullable().optional(),
  lista_precio_predeterminada_id: z.number().nullable().optional()
})

// Estado del formulario
const formData = reactive({
  // Campos editables
  nombre: '',
  email: '',
  telefono: '',
  direccion: '',
  logo_url: '',
  favicon_url: '',
  colores_tema: {
    primario: '#007bff',
    secundario: '#6c757d',
    acento: '#28a745'
  },
  url_whatsapp: '',
  url_facebook: '',
  url_instagram: '',
  lista_precio_predeterminada_id: null as number | null,
  
  // Campos solo lectura
  codigo: '',
  razon_social: '',
  cuit: '',
  dominio_personalizado: '',
  fecha_vencimiento: '',
  puede_agregar_productos: false,
  puede_agregar_categorias: false,
  mostrar_precios: true,
  mostrar_stock: false,
  permitir_pedidos: false,
  productos_por_pagina: 20
})

// Inicializar datos después de la creación
const initializeFormData = () => {
  if (props.initialData) {
    Object.keys(props.initialData).forEach(key => {
      const value = props.initialData[key]
      // Convertir null a string vacío para campos de texto
      if (
        value === null &&
        Object.prototype.hasOwnProperty.call(formData, key) &&
        typeof (formData as any)[key] === 'string'
      ) {
        (formData as any)[key] = ''
      } else if (value !== undefined) {
        (formData as any)[key] = value
      }
    })
  }
}

// Opciones para select (solo lectura)
const productosPorPaginaOptions = [
  { label: '6 productos', value: 6 },
  { label: '12 productos', value: 12 },
  { label: '20 productos', value: 20 },
  { label: '30 productos', value: 30 },
  { label: '50 productos', value: 50 },
  { label: '100 productos', value: 100 }
]

// Opciones para listas de precios
const listasPreciosOptions = computed(() => [
  { label: 'Ninguna (usar predeterminada del sistema)', value: null },
  ...listasPrecios.value.map(lista => ({
    label: `${lista.codigo || 'Sin código'} - ${lista.nombre || 'Sin nombre'}`,
    value: lista.id
  }))
])

// Paletas de colores predefinidas (optimizadas para texto blanco)
const colorPalettes = [
  {
    label: '🌿 Nature Fresh',
    value: 'nature-fresh',
    colors: {
      primario: '#2D5016', // Verde oscuro natural
      secundario: '#8B7355', // Marrón tierra
      acento: '#D97706'      // Naranja cálido
    }
  },
  {
    label: '💻 Tech Blue',
    value: 'tech-blue',
    colors: {
      primario: '#1E3A8A', // Azul marino profundo
      secundario: '#1D4ED8', // Azul royal
      acento: '#F59E0B'      // Amarillo dorado
    }
  },
  {
    label: '🧊 Cool Minimal',
    value: 'cool-minimal',
    colors: {
      primario: '#374151', // Gris azulado oscuro
      secundario: '#6B7280', // Gris medio
      acento: '#0891B2'      // Cyan oscuro
    }
  },
  {
    label: '🍧 Sweet Berry',
    value: 'sweet-berry',
    colors: {
      primario: '#7C2D92', // Púrpura oscuro
      secundario: '#BE185D', // Rosa intenso
      acento: '#DC2626'      // Rojo vibrante
    }
  },
  {
    label: '🌇 Sunset Pro',
    value: 'sunset-pro',
    colors: {
      primario: '#C2410C', // Naranja quemado
      secundario: '#DC2626', // Rojo coral
      acento: '#16A34A'      // Verde esmeralda
    }
  },
  {
    label: '🌊 Deep Ocean',
    value: 'deep-ocean',
    colors: {
      primario: '#0C4A6E', // Azul océano profundo
      secundario: '#0891B2', // Turquesa
      acento: '#EA580C'      // Naranja coral
    }
  },
  {
    label: '🍃 Forest Deep',
    value: 'forest-deep',
    colors: {
      primario: '#14532D', // Verde bosque
      secundario: '#166534', // Verde pino
      acento: '#DC2626'      // Rojo contraste
    }
  },
  {
    label: '🌸 Elegant Rose',
    value: 'elegant-rose',
    colors: {
      primario: '#9F1239', // Rosa elegante
      secundario: '#BE185D', // Magenta
      acento: '#7C3AED'      // Violeta
    }
  },
  {
    label: '🔥 Fire Power',
    value: 'fire-power',
    colors: {
      primario: '#991B1B', // Rojo fuego
      secundario: '#C2410C', // Naranja intenso
      acento: '#FBBF24'      // Amarillo dorado
    }
  },
  {
    label: '🌙 Midnight',
    value: 'midnight',
    colors: {
      primario: '#1F2937', // Gris muy oscuro
      secundario: '#374151', // Gris carbón
      acento: '#10B981'      // Verde esmeralda
    }
  },
  {
    label: '👑 Royal Purple',
    value: 'royal-purple',
    colors: {
      primario: '#581C87', // Púrpura real
      secundario: '#7C3AED', // Violeta
      acento: '#F59E0B'      // Dorado
    }
  },
  {
    label: '🌟 Golden Hour',
    value: 'golden-hour',
    colors: {
      primario: '#92400E', // Marrón dorado
      secundario: '#D97706', // Ámbar
      acento: '#DC2626'      // Rojo rubí
    }
  }
]

// Estado para la paleta seleccionada
const selectedPalette = ref('')

// Computed
const isFormValid = computed(() => {
  return formData.nombre.trim() !== ''
})

// Referencia al formulario
const form = ref()

// Métodos
const applyPalette = () => {
  if (selectedPalette.value) {
    const palette = colorPalettes.find(p => p.value === selectedPalette.value)
    if (palette) {
      formData.colores_tema.primario = palette.colors.primario
      formData.colores_tema.secundario = palette.colors.secundario
      formData.colores_tema.acento = palette.colors.acento
    }
  }
}

const selectPalette = (paletteValue: string) => {
  selectedPalette.value = paletteValue
  applyPalette()
}


const onSubmit = async (event: FormSubmitEvent<any>) => {
  const cleanData = { ...event.data }
  
  // Validar URLs si están presentes y no son null/vacías
  const urlFields = ['logo_url', 'favicon_url', 'url_whatsapp', 'url_facebook', 'url_instagram']
  for (const field of urlFields) {
    const value = cleanData[field]
    if (value && typeof value === 'string' && value.trim() !== '') {
      try {
        new URL(value)
      } catch {
        console.error(`URL inválida en ${field}:`, value)
        return // No emitir si hay URLs inválidas
      }
    }
  }
  
  // Validar email si está presente y no es null/vacío
  if (cleanData.email && typeof cleanData.email === 'string' && cleanData.email.trim() !== '') {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailRegex.test(cleanData.email)) {
      console.error('Email inválido:', cleanData.email)
      return
    }
  }
  
  // Limpiar campos vacíos, null o undefined
  Object.keys(cleanData).forEach(key => {
    const value = cleanData[key as keyof typeof cleanData]
    if (value === null || value === undefined || 
        (typeof value === 'string' && value.trim() === '')) {
      delete cleanData[key as keyof typeof cleanData]
    }
  })
  
  emit('submit', cleanData)
}

const onError = (errors: any) => {
  console.warn('Errores de validación:', errors)
}

// Event handlers para ImageUpload
const onImageUploaded = (url: string) => {
  const toast = useToast()
  toast.add({
    title: 'Imagen subida',
    description: 'La imagen se ha subido correctamente',
    color: 'green'
  })
}

const onImageDeleted = () => {
  const toast = useToast()
  toast.add({
    title: 'Imagen eliminada',
    description: 'La imagen se ha eliminado correctamente',
    color: 'green'
  })
}

const onImageError = (error: string) => {
  const toast = useToast()
  toast.add({
    title: 'Error con la imagen',
    description: error,
    color: 'red'
  })
}

// Inicializar datos
onMounted(async () => {
  initializeFormData()
  // Cargar listas de precios disponibles
  try {
    await fetchListas()
  } catch (error) {
    console.warn('Error loading price lists:', error)
  }
})

// También actualizar cuando cambien los props
watch(() => props.initialData, () => {
  initializeFormData()
}, { deep: true })
</script>