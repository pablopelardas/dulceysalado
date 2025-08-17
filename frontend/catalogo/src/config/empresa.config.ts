/**
 * Configuración estática de la empresa Dulce y Salado
 * Esta configuración reemplaza la carga dinámica desde la API
 */

export interface EmpresaConfig {
  id: number;
  codigo: string;
  nombre: string;
  razonSocial: string;
  telefono: string;
  email: string;
  direccion: string;
  logoUrl: string;
  faviconUrl: string;
  
  // Redes sociales
  whatsapp: string;
  facebook: string;
  instagram: string;
  
  // Configuración de catálogo
  mostrarPrecios: boolean;
  mostrarStock: boolean;
  permitirPedidos: boolean;
  productosPorPagina: number;
  
  // Features activas
  features: {
    autenticacion: boolean;
    pedidosWhatsapp: boolean;
    clientesMayoristas: boolean;
    catalogoListaPublico: boolean;
  };
  
  // Tema y colores
  tema: {
    colorPrimario: string;
    colorSecundario: string;
    colorAcento: string;
    colorNegro: string;
    colorBlanco: string;
    colorGrisOscuro: string;
    colorGrisMedio: string;
    colorGrisClaro: string;
  };
}

export const EMPRESA_CONFIG: EmpresaConfig = {
  // Datos básicos
  id: 1,
  codigo: "DULCEYSALADO",
  nombre: "Dulce & Salado MAX",
  razonSocial: "Dulce y Salado S.A.",
  telefono: "+54 11 0000-0000",
  email: "info@dulceysalado.com",
  direccion: "Av. Bernardo Ader 161, B1609 Boulogne, Provincia de Buenos Aires",
  
  // URLs de recursos (locales)
  logoUrl: "/assets/logo-dulceysalado.png",
  faviconUrl: "/favicon.ico",
  
  // Redes sociales
  whatsapp: "541100000000", // Número sin + para links de WhatsApp
  facebook: "https://facebook.com/dulceysalado",
  instagram: "https://instagram.com/dulceysalado",
  
  // Configuración de catálogo
  mostrarPrecios: true,
  mostrarStock: false,
  permitirPedidos: true, // Activado para sistema de carrito y pedidos
  productosPorPagina: 100,
  
  // Features activas (simplificado para esta implementación)
  features: {
    autenticacion: true, // Activado para sistema de pedidos
    pedidosWhatsapp: true, // Activo para mostrar botón flotante
    clientesMayoristas: false,
    catalogoListaPublico: false
  },
  
  // Tema y colores personalizados
  tema: {
    colorPrimario: "#000000",      // Negro profundo
    colorSecundario: "#FFFFFF",    // Blanco puro
    colorAcento: "#E50000",        // Rojo vivo
    colorNegro: "#000000",
    colorBlanco: "#FFFFFF",
    colorGrisOscuro: "#1E1E1E",
    colorGrisMedio: "#4A4A4A",
    colorGrisClaro: "#F5F5F5"
  }
};

// Helper para obtener el número de WhatsApp formateado
export const getWhatsAppLink = (mensaje?: string): string => {
  const baseUrl = `https://wa.me/${EMPRESA_CONFIG.whatsapp}`;
  if (mensaje) {
    const encodedMessage = encodeURIComponent(mensaje);
    return `${baseUrl}?text=${encodedMessage}`;
  }
  return baseUrl;
};

// Helper para obtener la configuración del tema
export const getThemeConfig = () => EMPRESA_CONFIG.tema;

// Helper para verificar features
export const isFeatureEnabled = (feature: keyof typeof EMPRESA_CONFIG.features): boolean => {
  return EMPRESA_CONFIG.features[feature] || false;
};

// Export default para facilitar imports
export default EMPRESA_CONFIG;