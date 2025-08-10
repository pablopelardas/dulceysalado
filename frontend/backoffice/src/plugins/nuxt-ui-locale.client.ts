export default defineNuxtPlugin((nuxtApp) => {
  // Proveer el contexto de localización que espera Nuxt UI
  nuxtApp.vueApp.provide(Symbol.for('nuxt-ui.locale-context'), {
    locale: ref('es'),
    locales: ref(['es', 'en'])
  })
})