<template>
  <Teleport to="body">
    <Transition name="toast">
      <div
        v-if="visible"
        class="fixed top-4 right-4 z-[70] max-w-sm"
      >
        <div 
          class="bg-white rounded-lg shadow-xl border border-gray-100 p-4 flex items-center gap-3"
          :class="{
            'border-green-200': type === 'success',
            'border-red-200': type === 'error',
            'border-blue-200': type === 'info'
          }"
        >
          <!-- Icon -->
          <div 
            class="w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0"
            :class="{
              'bg-green-100 text-green-600': type === 'success',
              'bg-red-100 text-red-600': type === 'error',
              'bg-blue-100 text-blue-600': type === 'info'
            }"
          >
            <CheckIcon v-if="type === 'success'" class="w-5 h-5" />
            <ExclamationTriangleIcon v-else-if="type === 'error'" class="w-5 h-5" />
            <InformationCircleIcon v-else class="w-5 h-5" />
          </div>
          
          <!-- Content -->
          <div class="flex-1">
            <p class="text-sm font-medium text-gray-900">{{ title }}</p>
            <p v-if="message" class="text-xs text-gray-500 mt-0.5">{{ message }}</p>
          </div>
          
          <!-- Close button -->
          <button 
            @click="close"
            class="p-1 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
          >
            <XMarkIcon class="w-4 h-4 text-gray-400" />
          </button>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { 
  CheckIcon, 
  ExclamationTriangleIcon,
  InformationCircleIcon,
  XMarkIcon 
} from '@heroicons/vue/24/outline'

export interface ToastProps {
  type?: 'success' | 'error' | 'info'
  title: string
  message?: string
  duration?: number
}

const props = withDefaults(defineProps<ToastProps>(), {
  type: 'success',
  duration: 3000
})

const emit = defineEmits<{
  close: []
}>()

const visible = ref(true)
let timeout: ReturnType<typeof setTimeout> | undefined

const startTimer = () => {
  if (props.duration > 0) {
    timeout = setTimeout(() => {
      close()
    }, props.duration)
  }
}

const close = () => {
  visible.value = false
  setTimeout(() => {
    emit('close')
  }, 300)
}

// Start timer when component mounts
startTimer()

// Clear timeout on unmount
watch(visible, (newVal) => {
  if (!newVal && timeout) {
    clearTimeout(timeout)
  }
})
</script>

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(20px);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(20px);
}
</style>