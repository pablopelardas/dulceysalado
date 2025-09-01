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
          Información Básica del Cliente
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Código" name="codigo" required>
          <UInput 
            v-model="formData.codigo"
            placeholder="Código único del cliente"
            :disabled="loading || mode === 'edit'"
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              {{ mode === 'edit' ? 'El código no se puede modificar' : 'Código único e irrepetible' }}
            </span>
          </template>
        </UFormField>
        
        <UFormField label="Nombre" name="nombre" required>
          <UInput 
            v-model="formData.nombre"
            placeholder="Nombre completo del cliente"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Email" name="email" required>
          <UInput 
            v-model="formData.email"
            type="email"
            placeholder="email@ejemplo.com"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Teléfono" name="telefono">
          <PhoneInput 
            v-model="formData.telefono"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="CUIT" name="cuit">
          <UInput 
            v-model="formData.cuit"
            placeholder="CUIT del cliente"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Tipo de IVA" name="tipo_iva">
          <USelectMenu
            v-model="formData.tipo_iva"
            :items="tiposIva"
            placeholder="Seleccionar tipo de IVA"
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Información de domicilio -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Información de Domicilio
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Dirección" name="direccion">
          <UInput 
            v-model="formData.direccion"
            placeholder="Dirección completa"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Altura" name="altura">
          <UInput 
            v-model="formData.altura"
            placeholder="Número de altura"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Localidad" name="localidad">
          <UInput 
            v-model="formData.localidad"
            placeholder="Localidad"
            :disabled="loading"
          />
        </UFormField>
        
        <UFormField label="Provincia" name="provincia">
          <UInput 
            v-model="formData.provincia"
            placeholder="Provincia"
            :disabled="loading"
          />
        </UFormField>
      </div>
    </UCard>

    <!-- Configuración comercial -->
    <UCard>
      <template #header>
        <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
          Configuración Comercial
        </h3>
      </template>
      
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Lista de Precios" name="lista_precio_id">
          <USelectMenu
            v-model="formData.lista_precio_id"
            :items="listasPrecios"
            value-key="id"
            label-key="nombre"
            placeholder="Seleccionar lista de precios"
            :loading="loadingListas"
            :disabled="loading"
            searchable
            nullable
          >
            <template #label>
              <span v-if="formData.lista_precio_id">
                {{ listasPrecios.find(l => l.id === formData.lista_precio_id)?.nombre }}
              </span>
              <span v-else class="text-gray-500">Seleccionar lista de precios</span>
            </template>
          </USelectMenu>
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              Lista de precios que verá este cliente
            </span>
          </template>
        </UFormField>

        <UFormField v-if="mode === 'edit'" label="Estado del Cliente" name="activo">
          <UCheckbox
            v-model="formData.activo"
            label="Cliente activo"
            :disabled="loading"
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              {{ formData.activo ? 'El cliente está activo y puede usar credenciales' : 'El cliente está inactivo' }}
            </span>
          </template>
        </UFormField>
      </div>
    </UCard>

    <!-- Credenciales de acceso -->
    <UCard v-if="mode === 'create'">
      <template #header>
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-gray-100">
            Credenciales de Acceso
          </h3>
          <USwitch
            v-model="createCredentials"
            color="primary"
          />
        </div>
        <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
          {{ createCredentials ? 'Se crearán credenciales de acceso para este cliente' : 'El cliente se creará sin credenciales' }}
        </p>
      </template>
      
      <div v-if="createCredentials" class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <UFormField label="Usuario" name="username" :required="createCredentials">
          <UInput 
            v-model="formData.username"
            placeholder="Nombre de usuario"
            :disabled="loading"
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              Usuario para iniciar sesión
            </span>
          </template>
        </UFormField>
        
        <UFormField label="Contraseña" name="password" :required="createCredentials">
          <UInput 
            v-model="formData.password"
            type="password"
            placeholder="Contraseña"
            :disabled="loading"
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              Contraseña segura para el acceso
            </span>
          </template>
        </UFormField>
        
        <UFormField>
          <UCheckbox
            v-model="formData.is_active"
            label="Usuario activo"
            :disabled="loading"
          />
          <template #help>
            <span class="text-sm text-gray-500 dark:text-gray-400">
              {{ formData.is_active ? 'El usuario podrá iniciar sesión' : 'El usuario estará deshabilitado' }}
            </span>
          </template>
        </UFormField>
      </div>
    </UCard>

    <!-- Botones de acción -->
    <div class="flex flex-col sm:flex-row gap-4 justify-end pt-6 border-t border-gray-200 dark:border-gray-700">
      <UButton
        variant="ghost"
        color="gray"
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
        {{ mode === 'create' ? 'Crear Cliente' : 'Actualizar Cliente' }}
      </UButton>
    </div>
  </UForm>
</template>

<script setup lang="ts">
import { z } from 'zod'
import PhoneInput from '~/components/ui/PhoneInput.vue'
import type { CreateClienteCommand, UpdateClienteCommand, ClienteDto } from '~/types/clientes'
import type { ListaPrecioInfo } from '~/types/productos'

interface Props {
  mode: 'create' | 'edit'
  loading?: boolean
  cliente?: ClienteDto
}

interface Emits {
  submit: [data: CreateClienteCommand | UpdateClienteCommand]
  cancel: []
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Composables
const { listas: listasPrecios, init: initListasPrecios } = useListasPrecios()

// Estado
const form = ref()
const loadingListas = ref(false)
const createCredentials = ref(false)

// Opciones para selects
const tiposIva = [
  { label: 'Responsable Inscripto', value: 'responsable_inscripto' },
  { label: 'Consumidor Final', value: 'consumidor_final' },
  { label: 'Exento', value: 'exento' },
  { label: 'Monotributo', value: 'monotributo' }
]

// Esquema de validación
const baseSchema = z.object({
  codigo: z.string().min(1, 'El código es requerido'),
  nombre: z.string().min(1, 'El nombre es requerido'),
  email: z.string().email('Email inválido'),
  telefono: z.string().optional(),
  cuit: z.string().optional(),
  tipo_iva: z.string().optional(),
  direccion: z.string().optional(),
  altura: z.string().optional(),
  localidad: z.string().optional(),
  provincia: z.string().optional(),
  lista_precio_id: z.number().nullable().optional()
})

const createSchema = baseSchema.extend({
  username: z.string().optional(),
  password: z.string().optional(),
  is_active: z.boolean().optional()
}).refine((data) => {
  // Si createCredentials es true, username y password son requeridos
  if (createCredentials.value) {
    return data.username && data.username.length > 0 && data.password && data.password.length > 0
  }
  return true
}, {
  message: 'Usuario y contraseña son requeridos cuando se crean credenciales',
  path: ['username']
})

const updateSchema = baseSchema.extend({
  activo: z.boolean().optional()
})

const schema = computed(() => {
  return props.mode === 'create' ? createSchema : updateSchema
})

// Datos del formulario
const formData = reactive({
  codigo: props.cliente?.codigo || '',
  nombre: props.cliente?.nombre || '',
  email: props.cliente?.email || '',
  telefono: props.cliente?.telefono || '',
  cuit: props.cliente?.cuit || '',
  tipo_iva: props.cliente?.tipo_iva || '',
  direccion: props.cliente?.direccion || '',
  altura: props.cliente?.altura || '',
  localidad: props.cliente?.localidad || '',
  provincia: props.cliente?.provincia || '',
  lista_precio_id: props.cliente?.lista_precio?.id || null,
  activo: props.cliente?.activo ?? true,
  // Campos de credenciales (solo para crear)
  username: '',
  password: '',
  is_active: true
})

// Métodos
const onSubmit = async (event: any) => {
  try {
    const data = { ...event.data }
    
    if (props.mode === 'create') {
      // Si no se crean credenciales, remover campos de credenciales
      if (!createCredentials.value) {
        delete data.username
        delete data.password
        delete data.is_active
      }
      emit('submit', data as CreateClienteCommand)
    } else {
      // Para actualización, remover campos no necesarios
      delete data.username
      delete data.password
      delete data.is_active
      emit('submit', { ...data, id: props.cliente!.id } as UpdateClienteCommand)
    }
  } catch (error) {
    console.error('Error en onSubmit:', error)
  }
}

const onError = (event: any) => {
  console.error('Errores de validación:', event.errors)
}

const handleCancel = () => {
  emit('cancel')
}

// Cargar listas de precios al montar el componente
onMounted(async () => {
  try {
    loadingListas.value = true
    await initListasPrecios()
  } catch (error) {
    console.error('Error cargando listas de precios:', error)
  } finally {
    loadingListas.value = false
  }
})
</script>