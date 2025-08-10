import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { apiService, type Company } from '@/services/api'
import { useTheme } from '@/composables/useTheme'
import { useSeo } from '@/composables/useSeo'

export const useCompanyStore = defineStore('company', () => {
  // State
  const company = ref<Company | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Composables
  const { setThemeFromCompany, setFavicon } = useTheme()
  const { setCompanySeo } = useSeo()

  // Getters
  const isLoaded = computed(() => company.value !== null)
  const companyName = computed(() => company.value?.nombre || '')
  const companyLogo = computed(() => company.value?.logo_url || '')
  const showPrices = computed(() => company.value?.mostrar_precios ?? true)
  const showStock = computed(() => company.value?.mostrar_stock ?? false)
  const allowOrders = computed(() => company.value?.permitir_pedidos ?? true)
  const productsPerPage = computed(() => company.value?.productos_por_pagina ?? 20)
  const whatsappUrl = computed(() => company.value?.url_whatsapp || '')
  const facebookUrl = computed(() => company.value?.url_facebook || '')
  const instagramUrl = computed(() => company.value?.url_instagram || '')
  
  // Feature flags
  const hasFeature = (featureCode: string) => {
    return company.value?.features?.some(feature => feature.codigo === featureCode && feature.habilitado) ?? false
  }
  
  const getFeature = (featureCode: string) => {
    return company.value?.features?.find(feature => feature.codigo === featureCode && feature.habilitado)
  }
  
  const hasWhatsAppOrders = computed(() => hasFeature('pedido_whatsapp'))
  const hasRequiredFields = computed(() => hasFeature('pedido_campos_requeridos'))
  
  const requiredOrderFields = computed(() => {
    const feature = getFeature('pedido_campos_requeridos')
    if (!feature?.valor) return []
    
    try {
      const fields = JSON.parse(feature.valor)
      return Array.isArray(fields) ? fields : []
    } catch (error) {
      console.error('Error parsing required fields:', error)
      return []
    }
  })
  
  // Social media links that exist
  const socialLinks = computed(() => {
    const links = []
    if (whatsappUrl.value) {
      links.push({
        name: 'WhatsApp',
        url: whatsappUrl.value,
        icon: 'whatsapp'
      })
    }
    if (facebookUrl.value) {
      links.push({
        name: 'Facebook',
        url: facebookUrl.value,
        icon: 'facebook'
      })
    }
    if (instagramUrl.value) {
      links.push({
        name: 'Instagram',
        url: instagramUrl.value,
        icon: 'instagram'
      })
    }
    return links
  })

  // Actions
  const fetchCompany = async () => {
    loading.value = true
    error.value = null
    
    try {
      const response = await apiService.getCompany()
      
      if (response.error) {
        error.value = response.error
        return
      }
      
      if (response.data) {
        company.value = response.data
        
        // Apply theme and favicon
        setThemeFromCompany(response.data)
        if (response.data.favicon_url) {
          setFavicon(response.data.favicon_url)
        }
        
        // Update SEO meta tags
        setCompanySeo(response.data)
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Error loading company data'
    } finally {
      loading.value = false
    }
  }

  const updateTitle = (pageTitle?: string) => {
    if (!company.value) return
    
    setCompanySeo(company.value, pageTitle)
  }

  // Initialize if not loaded
  const init = async () => {
    if (!isLoaded.value && !loading.value) {
      await fetchCompany()
    }
  }

  return {
    // State
    company,
    loading,
    error,
    
    // Getters
    isLoaded,
    companyName,
    companyLogo,
    showPrices,
    showStock,
    allowOrders,
    productsPerPage,
    whatsappUrl,
    facebookUrl,
    instagramUrl,
    socialLinks,
    
    // Feature flags
    hasFeature,
    getFeature,
    hasWhatsAppOrders,
    hasRequiredFields,
    requiredOrderFields,
    
    // Actions
    fetchCompany,
    updateTitle,
    init
  }
})