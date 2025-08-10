import tailwindcss from "@tailwindcss/vite";
// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-05-15',
  devtools: { enabled: true },
  srcDir: 'src',
  css: ['~/assets/css/tailwind.css'],
  
  // devServer: {
  //   https: {
  //     key: './cert/localhost-key.pem',
  //     cert: './cert/localhost.pem'
  //   },
  //   port: 3000
  // },

  app:{
    head:{
      htmlAttrs:{
        lang: 'es'
      }
    }
  },
  ui:{
    theme:{
      colors:[
        'purple',
        'blue',
        'green',
        'red',
        'yellow',
        'orange',
        'amber',
        'pink',
        'gray',
        'indigo',
        'prima',
        'primary',
        'secondary',
        'tertiary',
        'info',
        'success',
        'warning',
        'error'
      ]
    }
  },

  colorMode: {
    preference: 'system', // default value of $colorMode.preference
    fallback: 'light', // fallback value if not system preference found
    hid: 'nuxt-color-mode-script',
    globalName: '__NUXT_COLOR_MODE__',
    componentName: 'ColorScheme',
    classPrefix: '',
    classSuffix: '',
    storageKey: 'nuxt-color-mode'
  },
  appConfig:{
    ui:{
      colors:{
        purple: 'purple',
        blue: 'blue',
        green: 'green',
        red: 'red',
        yellow: 'yellow',
        orange: 'orange',
        amber: 'amber',
        pink: 'pink',
        gray: 'gray',
        indigo: 'indigo',
      }
    }
  },


  modules: [
    // '@nuxt/content', // Deshabilitado temporalmente - causa errores de WebSocket
    '@nuxt/eslint',
    '@nuxt/fonts',
    '@nuxt/icon',
    '@nuxt/image',
    '@nuxt/test-utils',
    '@nuxt/ui',
    '@pinia/nuxt'
  ],
  runtimeConfig: {
    // Variables privadas del servidor
    apiSecret: '', // NUXT_API_SECRET
    
    // Variables públicas (cliente y servidor)
    public: {
      // Configuración básica que puede ser sobrescrita por entorno
      nodeEnv: process.env.NODE_ENV || 'development'
    }
  },

  vite: {
    plugins: [
      tailwindcss(),
    ],
  },
})