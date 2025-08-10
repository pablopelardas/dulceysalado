/**
 * Utilidad para aplicar el tema de la empresa usando CSS custom properties
 */

import { getThemeConfig } from '@/config/empresa.config'

export function applyTheme(): void {
  const tema = getThemeConfig()
  const root = document.documentElement

  // Aplicar variables CSS para usar con Tailwind
  root.style.setProperty('--theme-primary', tema.colorPrimario)
  root.style.setProperty('--theme-primary-dark', tema.colorGrisOscuro) // Usar gris oscuro en lugar de azul
  root.style.setProperty('--theme-secondary', tema.colorSecundario) 
  root.style.setProperty('--theme-accent', tema.colorAcento)
  root.style.setProperty('--theme-black', tema.colorNegro)
  root.style.setProperty('--theme-white', tema.colorBlanco)
  root.style.setProperty('--theme-gray-dark', tema.colorGrisOscuro)
  root.style.setProperty('--theme-gray-medium', tema.colorGrisMedio)
  root.style.setProperty('--theme-gray-light', tema.colorGrisClaro)
}