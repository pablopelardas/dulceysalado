import { ref, nextTick } from 'vue'
import { useCartStore } from '@/stores/cart'

const WELCOME_BACK_KEY = 'welcome-back-shown'
const SESSION_START_TIME = Date.now()

export function useWelcomeBack() {
  const cartStore = useCartStore()
  const showWelcomeModal = ref(false)
  
  const checkWelcomeBack = async () => {
    // Wait for next tick to ensure cart store is initialized
    await nextTick()
    
    // Check if cart has items
    if (cartStore.isEmpty) {
      return
    }
    
    // Check if we already showed the modal in this session
    const lastShown = sessionStorage.getItem(WELCOME_BACK_KEY)
    const currentSession = SESSION_START_TIME.toString()
    
    if (lastShown === currentSession) {
      return
    }
    
    // Show welcome back modal
    showWelcomeModal.value = true
    
    // Mark as shown for this session
    sessionStorage.setItem(WELCOME_BACK_KEY, currentSession)
  }
  
  const handleKeepList = () => {
    showWelcomeModal.value = false
    // List is already loaded, nothing to do
  }
  
  const handleClearList = () => {
    cartStore.clearCart()
    showWelcomeModal.value = false
  }
  
  return {
    showWelcomeModal,
    checkWelcomeBack,
    handleKeepList,
    handleClearList
  }
}