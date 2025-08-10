import { ref, h, render } from 'vue'
import Toast from '@/components/ui/Toast.vue'

interface ToastOptions {
  type?: 'success' | 'error' | 'info'
  title: string
  message?: string
  duration?: number
}

export function useToast() {
  const showToast = (options: ToastOptions) => {
    // Create container for toast
    const container = document.createElement('div')
    document.body.appendChild(container)

    // Create toast component
    const vnode = h(Toast, {
      ...options,
      onClose: () => {
        render(null, container)
        document.body.removeChild(container)
      }
    })

    // Render toast
    render(vnode, container)
  }

  return {
    toast: showToast,
    success: (title: string, message?: string) => 
      showToast({ type: 'success', title, message }),
    error: (title: string, message?: string) => 
      showToast({ type: 'error', title, message }),
    info: (title: string, message?: string) => 
      showToast({ type: 'info', title, message })
  }
}