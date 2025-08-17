<template>
  <Transition name="modal-fade">
    <div v-if="isOpen" class="fixed inset-0 z-[70] flex items-center justify-center p-4">
      <!-- Backdrop -->
      <div class="absolute inset-0 bg-black/60"></div>
      
      <!-- Modal -->
      <div class="relative bg-white rounded-2xl shadow-2xl max-w-md w-full">
        <!-- Header -->
        <div class="px-6 py-6 text-center">
          <div class="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
            <ClipboardDocumentListIcon class="w-8 h-8 text-blue-600" />
          </div>
          
          <h3 class="text-xl font-semibold text-gray-900 mb-2">
            ¡Bienvenido de vuelta!
          </h3>
          
          <p class="text-gray-600 mb-6">
            Tienes <span class="font-semibold text-gray-900">{{ itemCount }}</span> 
            {{ itemCount === 1 ? 'producto' : 'productos' }} guardados en tu lista de compras.
            ¿Qué deseas hacer?
          </p>
          
          <!-- Actions -->
          <div class="space-y-3">
            <button 
              @click="keepList"
              class="w-full px-4 py-3 text-white rounded-lg font-medium transition-colors cursor-pointer"
              :style="{ background: 'var(--theme-accent)' }"
            >
              <div class="flex items-center justify-center gap-2">
                <CheckIcon class="w-5 h-5" />
                Mantener mi lista
              </div>
            </button>
            
            <button 
              @click="clearList"
              class="w-full px-4 py-3 text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg font-medium transition-colors cursor-pointer"
            >
              <div class="flex items-center justify-center gap-2">
                <TrashIcon class="w-5 h-5" />
                Empezar lista nueva
              </div>
            </button>
          </div>
          
          <!-- Note -->
          <p class="text-xs text-gray-500 mt-4">
            Tu lista se guarda automáticamente para tu comodidad
          </p>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { 
  ClipboardDocumentListIcon,
  CheckIcon,
  TrashIcon
} from '@heroicons/vue/24/outline'

interface Props {
  isOpen: boolean
  itemCount: number
}

defineProps<Props>()

const emit = defineEmits<{
  keep: []
  clear: []
}>()

// Methods
const keepList = () => {
  emit('keep')
}

const clearList = () => {
  emit('clear')
}
</script>

<style scoped>
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: all 0.15s ease;
}

.modal-fade-enter-from {
  opacity: 0;
  transform: scale(0.9);
}

.modal-fade-leave-to {
  opacity: 0;
  transform: scale(0.9);
}
</style>