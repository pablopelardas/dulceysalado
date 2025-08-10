<!-- WhatsAppButton.vue - Botón flotante con Tailwind CSS -->
<template>
  <Teleport to="body">
    <div 
      v-if="showButton"
      class="fixed bottom-6 right-6 z-50 md:bottom-4 md:right-4"
    >
      <!-- Botón principal -->
      <button
        class="relative w-15 h-15 md:w-14 md:h-14 bg-gradient-to-br from-green-500 to-green-600 rounded-full shadow-xl cursor-pointer transition-all duration-300 flex items-center justify-center overflow-hidden hover:scale-110 active:scale-95"
        :class="{ 'rounded-full': !showTooltip }"
        @click="handleClick"
        @mouseenter="handleMouseEnter"
        @mouseleave="handleMouseLeave"
        :aria-label="buttonLabel"
        style="box-shadow: 0 8px 25px rgba(37, 211, 102, 0.3);"
      >
        <!-- Ícono de WhatsApp -->
        <div class="relative w-7 h-7 md:w-6 md:h-6 text-white z-10" style="filter: drop-shadow(0 1px 2px rgba(0, 0, 0, 0.2));">
          <svg fill="currentColor" viewBox="0 0 24 24" class="w-full h-full">
            <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893A11.821 11.821 0 0020.485 3.097"/>
          </svg>
        </div>

        <!-- Tooltip/Texto -->
        <Transition name="tooltip">
          <div 
            v-if="showTooltip" 
            class="absolute right-16 top-1/2 transform -translate-y-1/2 bg-gray-900 text-white px-3 py-2 rounded-lg text-sm font-medium whitespace-nowrap shadow-lg hidden md:block"
          >
            {{ tooltipText }}
            <div class="absolute top-1/2 left-full transform -translate-y-1/2 border-6 border-transparent border-l-gray-900"></div>
          </div>
        </Transition>

        <!-- Indicador de nuevos mensajes (opcional) -->
        <div v-if="showNotification" class="absolute -top-0.5 -right-0.5 w-4.5 h-4.5 bg-red-500 rounded-full border-2 border-white z-20">
          <span class="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-2 h-2 bg-white rounded-full animate-pulse"></span>
        </div>

        <!-- Animación de ondas -->
        <div v-if="isAnimating" class="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-full h-full pointer-events-none">
          <div class="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-full h-full border-2 border-green-400 rounded-full opacity-0 animate-ping"></div>
          <div class="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-full h-full border-2 border-green-400 rounded-full opacity-0 animate-ping animation-delay-200"></div>
          <div class="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-full h-full border-2 border-green-400 rounded-full opacity-0 animate-ping animation-delay-400"></div>
        </div>

        <!-- Ping animation por defecto -->
        <div class="absolute inset-0 rounded-full bg-green-500 animate-ping opacity-20"></div>
      </button>

      <!-- Mensaje flotante emergente (opcional) -->
      <Transition name="message-popup">
        <div 
          v-if="showMessagePopup" 
          class="absolute bottom-20 right-0 w-70 md:w-60 bg-white rounded-xl shadow-2xl overflow-hidden border border-gray-200"
        >
          <div class="flex p-4 gap-3">
            <div class="flex-shrink-0 w-10 h-10 rounded-full overflow-hidden bg-gray-100">
              <img 
                :src="EMPRESA_CONFIG.logoUrl" 
                :alt="EMPRESA_CONFIG.nombre"
                class="w-full h-full object-cover"
              />
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex justify-between items-center mb-1">
                <strong class="font-semibold text-sm text-gray-900">{{ EMPRESA_CONFIG.nombre }}</strong>
                <span class="text-xs text-gray-500">{{ currentTime }}</span>
              </div>
              <div class="text-sm text-gray-900 leading-normal">
                {{ popupMessage }}
              </div>
            </div>
          </div>
          
          <button 
            class="absolute top-2 right-2 w-6 h-6 flex items-center justify-center text-gray-400 rounded transition-colors duration-200 hover:bg-gray-100 hover:text-gray-900"
            @click="hideMessagePopup"
            aria-label="Cerrar mensaje"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>
      </Transition>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import EMPRESA_CONFIG, { getWhatsAppLink } from '@/config/empresa.config'

interface Props {
  message?: string
  tooltipText?: string
  showNotification?: boolean
  autoShowPopup?: boolean
  popupDelay?: number
  position?: 'bottom-right' | 'bottom-left' | 'top-right' | 'top-left'
}

const props = withDefaults(defineProps<Props>(), {
  message: '¡Hola! Me interesa conocer más sobre sus productos.',
  tooltipText: 'Chateá con nosotros',
  showNotification: false,
  autoShowPopup: true,
  popupDelay: 5000, // 5 segundos
  position: 'bottom-right'
})

const emit = defineEmits<{
  click: []
  messageShow: []
  messageHide: []
}>()

// State
const showTooltip = ref(false)
const showMessagePopup = ref(false)
const isAnimating = ref(false)
const popupTimeout = ref<number | null>(null)
const currentTime = ref('')

// Computed
const showButton = computed(() => EMPRESA_CONFIG.features.pedidosWhatsapp)

const buttonLabel = computed(() => `Contactar por WhatsApp con ${EMPRESA_CONFIG.nombre}`)

const whatsappLink = computed(() => getWhatsAppLink(props.message))

const popupMessage = computed(() => {
  return `¡Hola! 👋 ¿En qué podemos ayudarte? Consultanos sobre nuestros productos.`
})

// Methods
const handleClick = () => {
  // Animar el botón
  isAnimating.value = true
  setTimeout(() => {
    isAnimating.value = false
  }, 600)

  // Abrir WhatsApp
  window.open(whatsappLink.value, '_blank', 'noopener,noreferrer')
  
  // Ocultar popup si está visible
  if (showMessagePopup.value) {
    hideMessagePopup()
  }
  
  emit('click')
}

const handleMouseEnter = () => {
  if (!showMessagePopup.value) {
    showTooltip.value = true
  }
}

const handleMouseLeave = () => {
  showTooltip.value = false
}

const showMessagePopupDelayed = () => {
  if (!props.autoShowPopup) return

  popupTimeout.value = window.setTimeout(() => {
    showMessagePopup.value = true
    updateCurrentTime()
    emit('messageShow')
  }, props.popupDelay)
}

const hideMessagePopup = () => {
  showMessagePopup.value = false
  showTooltip.value = false
  emit('messageHide')
}

const updateCurrentTime = () => {
  const now = new Date()
  currentTime.value = now.toLocaleTimeString('es-AR', { 
    hour: '2-digit', 
    minute: '2-digit' 
  })
}

// Lifecycle
onMounted(() => {
  // Iniciar animación periódica sutil
  setInterval(() => {
    if (!showMessagePopup.value && !isAnimating.value) {
      isAnimating.value = true
      setTimeout(() => {
        isAnimating.value = false
      }, 600)
    }
  }, 10000) // Cada 10 segundos

  // Mostrar popup después del delay si está habilitado
  showMessagePopupDelayed()
})

onUnmounted(() => {
  if (popupTimeout.value) {
    window.clearTimeout(popupTimeout.value)
  }
})
</script>

<style scoped>
/* Transiciones para tooltip */
.tooltip-enter-active,
.tooltip-leave-active {
  transition: all 200ms ease;
}

.tooltip-enter-from,
.tooltip-leave-to {
  opacity: 0;
  transform: translateY(-50%) translateX(10px);
}

/* Transiciones para popup de mensaje */
.message-popup-enter-active,
.message-popup-leave-active {
  transition: all 300ms ease;
}

.message-popup-enter-from,
.message-popup-leave-to {
  opacity: 0;
  transform: translateY(20px) scale(0.9);
}

/* Delays de animación personalizados */
.animation-delay-200 {
  animation-delay: 0.2s;
}

.animation-delay-400 {
  animation-delay: 0.4s;
}
</style>