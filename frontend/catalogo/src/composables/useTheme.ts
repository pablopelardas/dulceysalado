import { ref, computed } from 'vue'

interface ThemeColors {
  primary: string
  secondary: string
  accent: string
  text: string
  background: string
}

interface CompanyData {
  colores_tema?: string
  favicon_url?: string
}

const themeColors = ref<ThemeColors>({
  primary: '#3b82f6',
  secondary: '#64748b', 
  accent: '#f1f5f9',
  text: '#1e293b',
  background: '#ffffff'
})

export function useTheme() {
  
  // Function to parse colors from company configuration
  const parseCompanyColors = (coloresTheme: string): ThemeColors | null => {
    if (!coloresTheme || typeof coloresTheme !== 'string') {
      return null
    }
    
    try {
      // Try to parse as JSON first (new format)
      const colorObj = JSON.parse(coloresTheme)
      if (colorObj && typeof colorObj === 'object') {
        return {
          primary: colorObj.primario || colorObj.primary || '#3b82f6',
          secondary: colorObj.secundario || colorObj.secondary || '#64748b',
          accent: colorObj.acento || colorObj.accent || '#f1f5f9',
          text: colorObj.texto || colorObj.text || '#1e293b',
          background: colorObj.fondo || colorObj.background || '#ffffff'
        }
      }
    } catch (e) {
      // If JSON parsing fails, try the old comma-separated format
      const colors = coloresTheme.split(',').map(color => color.trim())
      
      if (colors.length >= 3) {
        return {
          primary: colors[0] || '#3b82f6',
          secondary: colors[1] || '#64748b',
          accent: colors[2] || '#f1f5f9',
          text: colors[3] || '#1e293b',
          background: colors[4] || '#ffffff'
        }
      }
    }
    
    return null
  }
  
  // Function to generate lighter and darker variations
  const generateColorVariations = (hex: string) => {
    // Remove # if present
    const cleanHex = hex.replace('#', '')
    
    // Convert to RGB
    const r = parseInt(cleanHex.substring(0, 2), 16)
    const g = parseInt(cleanHex.substring(2, 4), 16)
    const b = parseInt(cleanHex.substring(4, 6), 16)
    
    // Generate lighter version (increase brightness)
    const lighter = {
      r: Math.min(255, Math.floor(r + (255 - r) * 0.3)),
      g: Math.min(255, Math.floor(g + (255 - g) * 0.3)),
      b: Math.min(255, Math.floor(b + (255 - b) * 0.3))
    }
    
    // Generate darker version (decrease brightness)
    const darker = {
      r: Math.floor(r * 0.7),
      g: Math.floor(g * 0.7),
      b: Math.floor(b * 0.7)
    }
    
    return {
      light: `#${lighter.r.toString(16).padStart(2, '0')}${lighter.g.toString(16).padStart(2, '0')}${lighter.b.toString(16).padStart(2, '0')}`,
      dark: `#${darker.r.toString(16).padStart(2, '0')}${darker.g.toString(16).padStart(2, '0')}${darker.b.toString(16).padStart(2, '0')}`
    }
  }
  
  // Apply theme colors to CSS custom properties
  const applyTheme = (colors: ThemeColors) => {
    const root = document.documentElement
    
    // Set main colors
    root.style.setProperty('--theme-primary', colors.primary)
    root.style.setProperty('--theme-secondary', colors.secondary)
    root.style.setProperty('--theme-accent', colors.accent)
    root.style.setProperty('--theme-text', colors.text)
    root.style.setProperty('--theme-background', colors.background)
    
    // Generate and set variations
    const primaryVariations = generateColorVariations(colors.primary)
    const secondaryVariations = generateColorVariations(colors.secondary)
    const accentVariations = generateColorVariations(colors.accent)
    
    root.style.setProperty('--theme-primary-light', primaryVariations.light)
    root.style.setProperty('--theme-primary-dark', primaryVariations.dark)
    root.style.setProperty('--theme-secondary-light', secondaryVariations.light)
    root.style.setProperty('--theme-secondary-dark', secondaryVariations.dark)
    root.style.setProperty('--theme-accent-light', accentVariations.light)
    root.style.setProperty('--theme-accent-dark', accentVariations.dark)
    
    // Update reactive state
    themeColors.value = colors
  }
  
  // Set theme from company data
  const setThemeFromCompany = (companyData: CompanyData) => {
    if (!companyData?.colores_tema) {
      return
    }
    
    const colors = parseCompanyColors(companyData.colores_tema)
    if (colors) {
      applyTheme(colors)
    }
  }
  
  // Set favicon dynamically
  const setFavicon = (faviconUrl: string) => {
    if (!faviconUrl) return
    
    const link = document.querySelector("link[rel*='icon']") as HTMLLinkElement
    
    // Only update if it's different from current favicon
    if (link && link.href === faviconUrl) {
      return
    }
    
    // Create or update favicon link
    const newLink = link || document.createElement('link')
    newLink.type = 'image/x-icon'
    newLink.rel = 'shortcut icon'
    newLink.href = faviconUrl
    
    if (!link) {
      document.getElementsByTagName('head')[0].appendChild(newLink)
    }
  }
  
  // Computed properties for current theme
  const currentTheme = computed(() => themeColors.value)
  
  // Function to calculate color luminance
  const getLuminance = (hex: string): number => {
    const cleanHex = hex.replace('#', '')
    const r = parseInt(cleanHex.substring(0, 2), 16) / 255
    const g = parseInt(cleanHex.substring(2, 4), 16) / 255
    const b = parseInt(cleanHex.substring(4, 6), 16) / 255
    
    // Apply gamma correction
    const sRGB = [r, g, b].map(c => {
      return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4)
    })
    
    // Calculate relative luminance
    return 0.2126 * sRGB[0] + 0.7152 * sRGB[1] + 0.0722 * sRGB[2]
  }
  
  // Function to calculate contrast ratio between two colors
  const getContrastRatio = (color1: string, color2: string): number => {
    const lum1 = getLuminance(color1)
    const lum2 = getLuminance(color2)
    const brightest = Math.max(lum1, lum2)
    const darkest = Math.min(lum1, lum2)
    return (brightest + 0.05) / (darkest + 0.05)
  }
  
  // Function to get optimal text color for a background
  const getOptimalTextColor = (backgroundColor: string): string => {
    // Fallback to simple brightness calculation if hex parsing fails
    try {
      const cleanHex = backgroundColor.replace('#', '')
      if (cleanHex.length !== 6) {
        return '#ffffff' // Default to white for invalid colors
      }
      
      const r = parseInt(cleanHex.substring(0, 2), 16)
      const g = parseInt(cleanHex.substring(2, 4), 16)
      const b = parseInt(cleanHex.substring(4, 6), 16)
      
      // Calculate perceived brightness using YIQ formula
      const brightness = (r * 299 + g * 587 + b * 114) / 1000
      
      // If brightness is greater than 128 (mid-point), use black text
      // Otherwise use white text
      return brightness > 128 ? '#000000' : '#ffffff'
    } catch (error) {
      console.warn('Error calculating text color for:', backgroundColor)
      return '#ffffff' // Default to white on error
    }
  }

  const isDarkMode = computed(() => {
    const bgColor = themeColors.value.background
    // Simple check if background is dark
    const hex = bgColor.replace('#', '')
    const r = parseInt(hex.substring(0, 2), 16)
    const g = parseInt(hex.substring(2, 4), 16)
    const b = parseInt(hex.substring(4, 6), 16)
    const brightness = (r * 299 + g * 587 + b * 114) / 1000
    return brightness < 128
  })
  
  // Computed for secondary color text
  const secondaryTextColor = computed(() => {
    return getOptimalTextColor(themeColors.value.secondary)
  })
  
  return {
    themeColors: currentTheme,
    isDarkMode,
    secondaryTextColor,
    getOptimalTextColor,
    getContrastRatio,
    setThemeFromCompany,
    setFavicon,
    applyTheme,
    parseCompanyColors
  }
}