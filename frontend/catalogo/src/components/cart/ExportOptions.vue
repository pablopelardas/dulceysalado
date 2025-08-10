<template>
  <Transition name="modal-fade">
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4">
      <!-- Backdrop -->
      <div 
        class="absolute inset-0 bg-black/60"
        @click="closeModal"
      ></div>
      
      <!-- Modal -->
      <div class="relative bg-white rounded-2xl shadow-2xl max-w-md w-full">
        <!-- Header -->
        <div class="px-6 py-4 border-b border-gray-100 rounded-t-2xl">
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold text-gray-900">
              Exportar lista de compras
            </h3>
            <button 
              @click="closeModal"
              class="p-2 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
            >
              <XMarkIcon class="w-5 h-5 text-gray-500" />
            </button>
          </div>
          <p class="text-sm text-gray-500 mt-1">
            Elige cómo quieres exportar tu lista
          </p>
          <div class="mt-2 p-3 bg-blue-50 rounded-lg border border-blue-200">
            <p class="text-xs text-blue-700 flex items-center gap-2">
              <svg class="w-4 h-4 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
              </svg>
              Esta herramienta es para planificar tu compra. No genera pedidos automáticos. Confirma disponibilidad y precios en el local.
            </p>
          </div>
        </div>
        
        <!-- Export Options -->
        <div class="p-6">
          <div class="space-y-3">
            <!-- Copy to Clipboard -->
            <button 
              @click="copyToClipboard"
              class="w-full flex items-center gap-4 p-4 bg-purple-50 hover:bg-purple-100 rounded-lg transition-colors cursor-pointer group"
            >
              <div class="w-12 h-12 bg-purple-500 rounded-full flex items-center justify-center text-white">
                <ClipboardDocumentIcon class="w-6 h-6" />
              </div>
              <div class="flex-1 text-left">
                <div class="font-medium text-gray-900 group-hover:text-purple-700">Copiar al portapapeles</div>
                <div class="text-sm text-gray-500">Copiar lista para pegar donde necesites</div>
              </div>
              <ChevronRightIcon class="w-5 h-5 text-gray-400 group-hover:text-purple-600" />
            </button>
            
            <!-- WhatsApp to Company (if configured and feature enabled) -->
            <button 
              v-if="EMPRESA_CONFIG.whatsapp && EMPRESA_CONFIG.features.pedidosWhatsapp"
              @click="() => exportToCompanyWhatsApp()"
              class="w-full flex items-center gap-4 p-4 bg-green-50 hover:bg-green-100 rounded-lg transition-colors cursor-pointer group border-2 border-green-200"
            >
              <div class="w-12 h-12 bg-green-600 rounded-full flex items-center justify-center text-white">
                <svg class="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893A11.821 11.821 0 0020.885 3.106"/>
                </svg>
              </div>
              <div class="flex-1 text-left">
                <div class="font-medium text-gray-900 group-hover:text-green-700 flex items-center gap-2">
                  Enviar pedido a {{ EMPRESA_CONFIG.nombre }}
                  <span class="px-2 py-1 text-xs bg-green-600 text-white rounded-full">Empresa</span>
                </div>
                <div class="text-sm text-gray-500">Enviar lista como pedido por WhatsApp</div>
              </div>
              <ChevronRightIcon class="w-5 h-5 text-gray-400 group-hover:text-green-600" />
            </button>
            
            <!-- WhatsApp General -->
            <button 
              @click="exportToWhatsApp"
              class="w-full flex items-center gap-4 p-4 bg-green-50 hover:bg-green-100 rounded-lg transition-colors cursor-pointer group"
            >
              <div class="w-12 h-12 bg-green-500 rounded-full flex items-center justify-center text-white">
                <svg class="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893A11.821 11.821 0 0020.885 3.106"/>
                </svg>
              </div>
              <div class="flex-1 text-left">
                <div class="font-medium text-gray-900 group-hover:text-green-700">WhatsApp</div>
                <div class="text-sm text-gray-500">Enviar por WhatsApp a cualquier contacto</div>
              </div>
              <ChevronRightIcon class="w-5 h-5 text-gray-400 group-hover:text-green-600" />
            </button>
            
            <!-- Email -->
            <button 
              @click="exportToEmail"
              class="w-full flex items-center gap-4 p-4 bg-blue-50 hover:bg-blue-100 rounded-lg transition-colors cursor-pointer group"
            >
              <div class="w-12 h-12 bg-blue-500 rounded-full flex items-center justify-center text-white">
                <EnvelopeIcon class="w-6 h-6" />
              </div>
              <div class="flex-1 text-left">
                <div class="font-medium text-gray-900 group-hover:text-blue-700">Email</div>
                <div class="text-sm text-gray-500">Enviar por correo electrónico</div>
              </div>
              <ChevronRightIcon class="w-5 h-5 text-gray-400 group-hover:text-blue-600" />
            </button>
            
            <!-- PDF File -->
            <button 
              @click="exportToPDF"
              class="w-full flex items-center gap-4 p-4 bg-red-50 hover:bg-red-100 rounded-lg transition-colors cursor-pointer group"
            >
              <div class="w-12 h-12 bg-red-500 rounded-full flex items-center justify-center text-white">
                <DocumentTextIcon class="w-6 h-6" />
              </div>
              <div class="flex-1 text-left">
                <div class="font-medium text-gray-900 group-hover:text-red-700">Documento PDF</div>
                <div class="text-sm text-gray-500">Descargar como archivo PDF</div>
              </div>
              <ChevronRightIcon class="w-5 h-5 text-gray-400 group-hover:text-red-600" />
            </button>
          </div>
        </div>
        
        <!-- Footer -->
        <div class="px-6 py-4 bg-gray-50 rounded-b-2xl">
          <div class="text-xs text-gray-500 text-center">
            Lista con {{ cartStore.itemCount }} productos y {{ cartStore.totalItems }} unidades
          </div>
        </div>
      </div>
    </div>
  </Transition>
  
  <!-- Customer Data Modal -->
  <CustomerDataModal
    :is-open="showCustomerDataModal"
    :required-fields="[]"
    @close="showCustomerDataModal = false"
    @submit="handleCustomerDataSubmit"
  />
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useCartStore } from '@/stores/cart'
import { EMPRESA_CONFIG } from '@/config/empresa.config'
import { useToast } from '@/composables/useToast'
import jsPDF from 'jspdf'
import CustomerDataModal from './CustomerDataModal.vue'
import type { CustomerData } from './CustomerDataModal.vue'
import { 
  XMarkIcon,
  EnvelopeIcon,
  DocumentTextIcon,
  ClipboardDocumentIcon,
  ChevronRightIcon
} from '@heroicons/vue/24/outline'

interface Props {
  isOpen: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  exported: [type: string]
}>()

// Stores
const cartStore = useCartStore()

// Composables
const { success, error } = useToast()

// State
const showCustomerDataModal = ref(false)

// Company data
const companyName = EMPRESA_CONFIG.nombre

// Methods
const closeModal = () => {
  emit('close')
}

const askToClearCart = () => {
  // Removed the confirm dialog - let users clear manually if they want
  // This provides a better UX without interrupting the flow
}

const handleCustomerDataSubmit = (data: CustomerData) => {
  showCustomerDataModal.value = false
  // Call exportToCompanyWhatsApp with the customer data
  exportToCompanyWhatsApp(data)
}

const exportToCompanyWhatsApp = (customerData?: CustomerData) => {
  // Simplified - no required fields collection for now
  
  // Use default template
  let messageTemplate: string | undefined
  
  const message = cartStore.exportForWhatsAppPedido(companyName, messageTemplate, customerData)
  
  // Use the correct WhatsApp URL format for both mobile and desktop
  const phoneNumber = EMPRESA_CONFIG.whatsapp
  let url = ''
  
  if (navigator.userAgent.includes('Mobile') || navigator.userAgent.includes('Android') || navigator.userAgent.includes('iPhone')) {
    url = `https://wa.me/${phoneNumber}?text=${message}`
  } else {
    url = `https://web.whatsapp.com/send?phone=${phoneNumber}&text=${message}`
  }
    
  window.open(url, '_blank')
  emit('exported', 'whatsapp-company')
  success('Pedido enviado', `Se abrió WhatsApp para enviar el pedido a ${companyName}`)
  closeModal()
  askToClearCart()
}

const exportToWhatsApp = () => {
  const message = cartStore.exportForWhatsApp()
  const url = `https://wa.me/?text=${message}`
  window.open(url, '_blank')
  emit('exported', 'whatsapp')
  success('Lista exportada', 'Se abrió WhatsApp con tu lista')
  closeModal()
  askToClearCart()
}

const exportToEmail = () => {
  const subject = encodeURIComponent('Lista de Compras')
  const body = cartStore.exportForEmail()
  const url = `mailto:?subject=${subject}&body=${body}`
  window.open(url)
  emit('exported', 'email')
  success('Lista exportada', 'Se abrió tu cliente de correo con la lista')
  closeModal()
  askToClearCart()
}

const exportToPDF = () => {
  const doc = new jsPDF()
  
  // Title
  doc.setFontSize(20)
  doc.setFont('helvetica', 'bold')
  doc.text(`Lista de Compras - ${companyName}`, 20, 30)
  
  // Date
  doc.setFontSize(10)
  doc.setFont('helvetica', 'normal')
  const date = new Date().toLocaleDateString('es-ES')
  doc.text(`Fecha: ${date}`, 20, 40)
  
  // Line separator
  doc.setLineWidth(0.5)
  doc.line(20, 45, 190, 45)
  
  let yPosition = 60
  
  // Items
  doc.setFontSize(12)
  cartStore.items.forEach((item, index) => {
    // Check if we need a new page
    if (yPosition > 250) {
      doc.addPage()
      yPosition = 30
    }
    
    // Product name (bold)
    doc.setFont('helvetica', 'bold')
    doc.text(`${index + 1}. ${item.nombre}`, 20, yPosition)
    yPosition += 8
    
    // Product details (normal)
    doc.setFont('helvetica', 'normal')
    doc.setFontSize(10)
    doc.text(`   Código: ${item.codigo}`, 20, yPosition)
    yPosition += 6
    doc.text(`   Cantidad: ${item.cantidad}`, 20, yPosition)
    yPosition += 6
    doc.text(`   Precio unitario: $${item.precio.toFixed(2)}`, 20, yPosition)
    yPosition += 6
    doc.text(`   Subtotal: $${(item.precio * item.cantidad).toFixed(2)}`, 20, yPosition)
    yPosition += 12
    
    doc.setFontSize(12)
  })
  
  // Totals section
  yPosition += 10
  if (yPosition > 240) {
    doc.addPage()
    yPosition = 30
  }
  
  doc.setLineWidth(0.3)
  doc.line(20, yPosition, 190, yPosition)
  yPosition += 15
  
  doc.setFont('helvetica', 'bold')
  doc.setFontSize(12)
  doc.text(`Productos diferentes: ${cartStore.itemCount}`, 20, yPosition)
  yPosition += 8
  doc.text(`Unidades totales: ${cartStore.totalItems}`, 20, yPosition)
  yPosition += 12
  
  doc.setFontSize(14)
  doc.text(`TOTAL GENERAL: $${cartStore.totalAmount.toFixed(2)}`, 20, yPosition)
  
  // Note about prices
  yPosition += 20
  if (yPosition > 260) {
    doc.addPage()
    yPosition = 30
  }
  
  doc.setFont('helvetica', 'italic')
  doc.setFontSize(9)
  doc.text('* Los precios mostrados están sujetos a cambios por parte del vendedor.', 20, yPosition)
  yPosition += 4
  doc.text('Los precios en esta página web pueden no reflejar los precios finales del local.', 20, yPosition)
  
  // Save the PDF
  const fileName = `lista-compras-${new Date().toISOString().split('T')[0]}.pdf`
  doc.save(fileName)
  
  emit('exported', 'pdf')
  success('PDF generado', `Se descargó ${fileName}`)
  closeModal()
  askToClearCart()
}

const copyToClipboard = async () => {
  try {
    const content = cartStore.exportToText()
    await navigator.clipboard.writeText(content)
    success('¡Copiado!', 'La lista fue copiada al portapapeles')
    emit('exported', 'clipboard')
    closeModal()
    askToClearCart()
  } catch (err) {
    console.error('Error copying to clipboard:', err)
    error('Error', 'No se pudo copiar al portapapeles')
  }
}

// Prevent body scroll when modal is open
watch(() => props.isOpen, (isOpen) => {
  if (isOpen) {
    document.body.style.overflow = 'hidden'
  } else {
    document.body.style.overflow = ''
  }
})
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