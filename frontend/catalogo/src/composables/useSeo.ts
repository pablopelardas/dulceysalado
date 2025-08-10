import { computed } from 'vue'
import type { Company } from '@/services/api'

interface SeoData {
  title: string
  description: string
  image?: string
  url?: string
}

export function useSeo() {
  const updateMetaTags = (seoData: SeoData) => {
    // Update page title
    document.title = seoData.title

    // Update or create meta description
    let metaDescription = document.querySelector('meta[name="description"]')
    if (!metaDescription) {
      metaDescription = document.createElement('meta')
      metaDescription.setAttribute('name', 'description')
      document.head.appendChild(metaDescription)
    }
    metaDescription.setAttribute('content', seoData.description)

    // Update or create Open Graph meta tags
    updateMetaProperty('og:title', seoData.title)
    updateMetaProperty('og:description', seoData.description)
    updateMetaProperty('og:type', 'website')
    
    if (seoData.image) {
      updateMetaProperty('og:image', seoData.image)
    }
    
    if (seoData.url) {
      updateMetaProperty('og:url', seoData.url)
    }

    // Update Twitter Card meta tags
    updateMetaProperty('twitter:card', 'summary_large_image')
    updateMetaProperty('twitter:title', seoData.title)
    updateMetaProperty('twitter:description', seoData.description)
    
    if (seoData.image) {
      updateMetaProperty('twitter:image', seoData.image)
    }
  }

  const updateMetaProperty = (property: string, content: string) => {
    let metaTag = document.querySelector(`meta[property="${property}"]`)
    if (!metaTag) {
      metaTag = document.createElement('meta')
      metaTag.setAttribute('property', property)
      document.head.appendChild(metaTag)
    }
    metaTag.setAttribute('content', content)
  }

  const setCompanySeo = (company: Company, pageTitle?: string) => {
    const title = pageTitle 
      ? `${pageTitle} - ${company.nombre}`
      : `${company.nombre} - Catálogo Online`
    
    const description = company.descripcion 
      ? `${company.descripcion} - Catálogo online de ${company.nombre}. Encuentra todos nuestros productos disponibles las 24 horas.`
      : `Catálogo online de ${company.nombre}. Encuentra todos nuestros productos disponibles las 24 horas. Los precios mostrados están sujetos a cambios.`

    const seoData: SeoData = {
      title,
      description,
      image: company.logo_url,
      url: window.location.href
    }

    updateMetaTags(seoData)
  }

  const setProductSeo = (productName: string, company: Company, productImage?: string) => {
    const title = `${productName} - ${company.nombre}`
    const description = `${productName} en ${company.nombre}. Consulta disponibilidad y precios en nuestro catálogo online.`
    
    const seoData: SeoData = {
      title,
      description,
      image: productImage || company.logo_url,
      url: window.location.href
    }

    updateMetaTags(seoData)
  }

  const setCategorySeo = (categoryName: string, company: Company) => {
    const title = `${categoryName} - ${company.nombre}`
    const description = `Productos de ${categoryName} en ${company.nombre}. Explora nuestra selección y encuentra lo que necesitas.`
    
    const seoData: SeoData = {
      title,
      description,
      image: company.logo_url,
      url: window.location.href
    }

    updateMetaTags(seoData)
  }

  return {
    updateMetaTags,
    setCompanySeo,
    setProductSeo,
    setCategorySeo
  }
}