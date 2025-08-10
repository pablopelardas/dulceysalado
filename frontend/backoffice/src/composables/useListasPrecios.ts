import type { ListaPrecioInfo } from '~/types/productos'

export const useListasPrecios = () => {
  const api = useApi()
  const toast = useToast()

  // Estado reactivo
  const listas = ref<ListaPrecioInfo[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const listaSeleccionada = ref<ListaPrecioInfo | null>(null)

  // Key para localStorage
  const STORAGE_KEY = 'selectedListaPrecio'

  // Helper para extraer mensaje de error de la API
  const getErrorMessage = (err: any): string => {
    if (err?.message) {
      return err.message
    }
    
    if (err?.data?.message) {
      return err.data.message
    }
    
    if (err?.data?.errors) {
      const errors = err.data.errors
      const firstError = Object.values(errors)[0] as string[]
      return firstError[0] || 'Error de validación'
    }
    
    return err?.statusText || err?.message || 'Error desconocido'
  }

  // Cargar listas de precios disponibles
  const fetchListas = async () => {
    loading.value = true
    error.value = null
    
    try {
      const response = await api.get<{ listas: ListaPrecioInfo[] }>('/api/listas-precios')
      listas.value = response.listas || []
      
      // Si no hay lista seleccionada, seleccionar la primera o la predeterminada
      if (!listaSeleccionada.value && listas.value.length > 0) {
        const listaPredeterminada = listas.value.find(lista => lista.es_predeterminada)
        const listaGuardada = getListaGuardada()
        
        if (listaGuardada && listas.value.find(l => l.id === listaGuardada.id)) {
          listaSeleccionada.value = listaGuardada
        } else if (listaPredeterminada) {
          listaSeleccionada.value = listaPredeterminada
        } else {
          listaSeleccionada.value = listas.value[0]
        }
        
        // Guardar la selección
        saveListaSeleccionada(listaSeleccionada.value)
      }
      
      return listas.value
    } catch (err: any) {
      const errorMessage = getErrorMessage(err)
      error.value = errorMessage
      toast.add({
        title: 'Error al cargar listas de precios',
        description: errorMessage,
        color: 'red'
      })
      throw err
    } finally {
      loading.value = false
    }
  }

  // Obtener lista guardada en localStorage
  const getListaGuardada = (): ListaPrecioInfo | null => {
    if (process.client) {
      try {
        const saved = localStorage.getItem(STORAGE_KEY)
        return saved ? JSON.parse(saved) : null
      } catch {
        return null
      }
    }
    return null
  }

  // Guardar lista seleccionada en localStorage
  const saveListaSeleccionada = (lista: ListaPrecioInfo) => {
    if (process.client) {
      try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(lista))
      } catch {
        // Ignorar errores de localStorage
      }
    }
  }

  // Seleccionar una lista de precios
  const selectLista = (lista: ListaPrecioInfo) => {
    listaSeleccionada.value = lista
    saveListaSeleccionada(lista)
  }

  // Obtener lista por ID
  const getListaById = (id: number): ListaPrecioInfo | undefined => {
    return listas.value.find(lista => lista.id === id)
  }

  // Obtener lista predeterminada
  const getListaPredeterminada = (): ListaPrecioInfo | undefined => {
    return listas.value.find(lista => lista.es_predeterminada)
  }

  // Verificar si una lista está seleccionada
  const isListaSeleccionada = (lista: ListaPrecioInfo): boolean => {
    return listaSeleccionada.value?.id === lista.id
  }

  // Computed para obtener el ID de la lista seleccionada
  const listaSeleccionadaId = computed(() => {
    return listaSeleccionada.value?.id || null
  })

  // Computed para verificar si hay listas disponibles
  const hasListas = computed(() => {
    return listas.value.length > 0
  })

  // Inicializar al montar (solo en cliente)
  const init = async () => {
    if (process.client && listas.value.length === 0) {
      await fetchListas()
    }
  }

  // Watcher para cargar lista guardada al inicio
  onMounted(() => {
    if (process.client) {
      const listaGuardada = getListaGuardada()
      if (listaGuardada) {
        listaSeleccionada.value = listaGuardada
      }
    }
  })

  return {
    // Estado
    listas: readonly(listas),
    loading: readonly(loading),
    error: readonly(error),
    listaSeleccionada: readonly(listaSeleccionada),
    listaSeleccionadaId,
    hasListas,
    
    // Acciones
    fetchListas,
    selectLista,
    getListaById,
    getListaPredeterminada,
    isListaSeleccionada,
    init
  }
}