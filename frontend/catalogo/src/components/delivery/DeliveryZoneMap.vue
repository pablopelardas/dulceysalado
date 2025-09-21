<!-- DeliveryZoneMap.vue - Mapa con zona de cobertura de delivery -->
<template>
  <div class="w-full">
    <div class="rounded-lg overflow-hidden border border-gray-200 shadow-sm">
      <div ref="mapContainer" class="h-64 w-full bg-gray-100"></div>
    </div>
    <div class="mt-2 flex items-center gap-2 text-xs">
      <div class="flex items-center gap-1">
        <div class="w-3 h-3 bg-green-500 rounded-sm opacity-40"></div>
        <span class="text-gray-600">Zona de cobertura</span>
      </div>
      <div class="flex items-center gap-1">
        <svg class="w-3 h-3 text-red-500" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />
        </svg>
        <span class="text-gray-600">Nuestro local</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

// Referencias
const mapContainer = ref<HTMLElement>()
let map: L.Map | null = null

// Configuración de la zona de delivery
// Local: Av. Bernardo Ader 161, Boulogne - Coordenadas exactas
const STORE_LOCATION: [number, number] = [-34.51384666704656, -58.56670738883928]
// Zona de delivery aproximada (ajustar con el cliente)
// Este polígono cubre aproximadamente 2km alrededor del local
const DELIVERY_ZONE: [number, number][] = [
  [-34.5038, -58.5767], // Norte (hacia Villa Adelina)
  [-34.5038, -58.5567], // Noreste
  [-34.5138, -58.5467], // Este (hacia Martínez)
  [-34.5238, -58.5567], // Sureste
  [-34.5238, -58.5767], // Sur (hacia San Isidro centro)
  [-34.5138, -58.5867], // Oeste (hacia Carapachay)
  [-34.5038, -58.5767]  // Cerrar el polígono
]

onMounted(() => {
  if (!mapContainer.value) return

  // Inicializar el mapa
  map = L.map(mapContainer.value).setView(STORE_LOCATION, 14)

  // Agregar capa de tiles (OpenStreetMap)
  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors',
    maxZoom: 18
  }).addTo(map)

  // Crear el polígono de la zona de delivery
  const deliveryZonePolygon = L.polygon(DELIVERY_ZONE, {
    color: '#10b981', // verde
    fillColor: '#10b981',
    fillOpacity: 0.3,
    weight: 2
  }).addTo(map)

  // Agregar popup a la zona
  deliveryZonePolygon.bindPopup('<strong>Zona de Delivery</strong><br>Entregamos en esta área')

  // Crear ícono personalizado para el marcador del local
  const storeIcon = L.divIcon({
    html: `
      <div class="relative">
        <svg class="w-8 h-8 text-red-500 drop-shadow-lg" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clip-rule="evenodd" />
        </svg>
      </div>
    `,
    className: 'custom-div-icon',
    iconSize: [32, 32],
    iconAnchor: [16, 32],
    popupAnchor: [0, -32]
  })

  // Agregar marcador del local
  const storeMarker = L.marker(STORE_LOCATION, { icon: storeIcon }).addTo(map)
  storeMarker.bindPopup('<strong>Dulce y Salado</strong><br>Av. Bernardo Ader 161, Boulogne<br>Horario: L-V 9:00-19:00, Sáb 9:00-14:00')

  // Ajustar el mapa para mostrar toda la zona
  const bounds = L.latLngBounds([...DELIVERY_ZONE, STORE_LOCATION])
  map.fitBounds(bounds, { padding: [20, 20] })

  // Fix para iconos de Leaflet que no se muestran correctamente
  delete (L.Icon.Default.prototype as any)._getIconUrl
  L.Icon.Default.mergeOptions({
    iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
    iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
    shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png'
  })
})

onUnmounted(() => {
  // Limpiar el mapa al desmontar el componente
  if (map) {
    map.remove()
    map = null
  }
})
</script>

<style scoped>
/* Estilos para el ícono personalizado */
:deep(.custom-div-icon) {
  background: transparent;
  border: none;
}

/* Fix para controles de Leaflet */
:deep(.leaflet-control-container) {
  position: absolute;
  z-index: 400;
}

/* Asegurar que el contenedor del mapa tenga altura */
:deep(.leaflet-container) {
  z-index: 1;
}
</style>