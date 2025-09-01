<template>
  <div class="relative">
    <!-- Input Field -->
    <button
      type="button"
      @click="toggleCalendar"
      class="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm bg-white text-left focus:outline-none focus:ring-2 focus:ring-red-500 focus:border-red-500 disabled:bg-gray-100 disabled:text-gray-500 disabled:cursor-not-allowed flex items-center justify-between"
      :disabled="disabled"
    >
      <span :class="selectedDate ? 'text-gray-900' : 'text-gray-500'">
        {{ selectedDate ? formatSelectedDate(selectedDate) : placeholder }}
      </span>
      <CalendarDaysIcon class="w-5 h-5 text-gray-400" />
    </button>

    <!-- Calendar Modal -->
    <Transition name="calendar-fade">
      <div
        v-if="showCalendar"
        class="absolute top-full left-0 z-50 mt-1 bg-white border border-gray-200 rounded-md shadow-lg w-72"
        @click.stop
      >
        <!-- Calendar Header -->
        <div class="flex items-center justify-between px-3 py-2 border-b border-gray-100">
          <button
            type="button"
            @click="previousMonth"
            class="p-1 rounded hover:bg-gray-100 text-gray-500 hover:text-gray-700 cursor-pointer"
          >
            <ChevronLeftIcon class="w-4 h-4" />
          </button>
          
          <div class="text-sm font-medium text-gray-900">
            {{ formatMonthYear(currentDate) }}
          </div>
          
          <button
            type="button"
            @click="nextMonth"
            class="p-1 rounded hover:bg-gray-100 text-gray-500 hover:text-gray-700 cursor-pointer"
          >
            <ChevronRightIcon class="w-4 h-4" />
          </button>
        </div>

        <!-- Days Grid -->
        <div class="p-2">
          <!-- Weekdays Header -->
          <div class="grid grid-cols-7 gap-1 mb-1">
            <div
              v-for="day in weekDays"
              :key="day"
              class="text-center text-xs font-medium text-gray-400 py-1"
            >
              {{ day }}
            </div>
          </div>

          <!-- Calendar Days -->
          <div class="grid grid-cols-7 gap-1">
            <button
              v-for="day in calendarDays"
              :key="`${day.date}-${day.isCurrentMonth}`"
              type="button"
              @click="selectDate(day)"
              :disabled="!day.isSelectable"
              class="w-8 h-8 flex items-center justify-center text-xs rounded transition-colors"
              :class="getDayClasses(day)"
            >
              {{ day.day }}
            </button>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="flex items-center justify-between px-3 py-2 border-t border-gray-100 bg-gray-50">
          <button
            type="button"
            @click="selectToday"
            :disabled="!isTodaySelectable"
            class="text-xs text-red-600 hover:text-red-700 font-medium disabled:text-gray-400 disabled:cursor-not-allowed cursor-pointer"
          >
            Mañana
          </button>
          <button
            type="button"
            @click="clearDate"
            class="text-xs text-gray-500 hover:text-gray-700 cursor-pointer"
          >
            Limpiar
          </button>
        </div>
      </div>
    </Transition>

    <!-- Backdrop -->
    <div
      v-if="showCalendar"
      @click="closeCalendar"
      class="fixed inset-0 z-40"
    ></div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import {
  CalendarDaysIcon,
  ChevronLeftIcon,
  ChevronRightIcon
} from '@heroicons/vue/24/outline'

interface Props {
  modelValue?: string
  disabled?: boolean
  minDate?: string
  maxDate?: string
  placeholder?: string
}

interface Emits {
  'update:modelValue': [value: string]
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: 'Seleccionar fecha',
  disabled: false
})

const emit = defineEmits<Emits>()

// State
const showCalendar = ref(false)
const currentDate = ref(new Date())
const selectedDate = ref(props.modelValue || '')

// Calendar data
const weekDays = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb']

interface CalendarDay {
  date: string
  day: number
  isCurrentMonth: boolean
  isToday: boolean
  isSelected: boolean
  isSelectable: boolean
}

// Computed
const calendarDays = computed((): CalendarDay[] => {
  const year = currentDate.value.getFullYear()
  const month = currentDate.value.getMonth()
  
  // First day of the month
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  
  // Start of calendar (might include previous month days)
  const startDate = new Date(firstDay)
  startDate.setDate(startDate.getDate() - firstDay.getDay())
  
  // Generate 42 days (6 weeks)
  const days: CalendarDay[] = []
  const today = new Date()
  const minDateObj = props.minDate ? new Date(props.minDate) : null
  const maxDateObj = props.maxDate ? new Date(props.maxDate) : null
  
  for (let i = 0; i < 42; i++) {
    const dayDate = new Date(startDate)
    dayDate.setDate(startDate.getDate() + i)
    
    const dateStr = dayDate.toISOString().split('T')[0]
    const isCurrentMonth = dayDate.getMonth() === month
    const isToday = dayDate.toDateString() === today.toDateString()
    const isSelected = dateStr === selectedDate.value
    
    let isSelectable = isCurrentMonth
    if (minDateObj) {
      isSelectable = isSelectable && dayDate >= minDateObj
    }
    if (maxDateObj) {
      isSelectable = isSelectable && dayDate <= maxDateObj
    }
    
    days.push({
      date: dateStr,
      day: dayDate.getDate(),
      isCurrentMonth,
      isToday,
      isSelected,
      isSelectable
    })
  }
  
  return days
})

const isTodaySelectable = computed(() => {
  const tomorrow = new Date()
  tomorrow.setDate(tomorrow.getDate() + 1)
  const tomorrowStr = tomorrow.toISOString().split('T')[0]
  
  const minDateObj = props.minDate ? new Date(props.minDate) : null
  const maxDateObj = props.maxDate ? new Date(props.maxDate) : null
  
  let selectable = true
  if (minDateObj) {
    selectable = selectable && tomorrow >= minDateObj
  }
  if (maxDateObj) {
    selectable = selectable && tomorrow <= maxDateObj
  }
  
  return selectable
})

// Methods
const toggleCalendar = () => {
  if (props.disabled) return
  showCalendar.value = !showCalendar.value
}

const closeCalendar = () => {
  showCalendar.value = false
}

const previousMonth = () => {
  currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() - 1, 1)
}

const nextMonth = () => {
  currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() + 1, 1)
}

const selectDate = (day: CalendarDay) => {
  if (!day.isSelectable) return
  
  selectedDate.value = day.date
  emit('update:modelValue', day.date)
  closeCalendar()
}

const selectToday = () => {
  if (!isTodaySelectable.value) return
  
  const tomorrow = new Date()
  tomorrow.setDate(tomorrow.getDate() + 1)
  const tomorrowStr = tomorrow.toISOString().split('T')[0]
  
  selectedDate.value = tomorrowStr
  emit('update:modelValue', tomorrowStr)
  closeCalendar()
}

const clearDate = () => {
  selectedDate.value = ''
  emit('update:modelValue', '')
  closeCalendar()
}

const formatMonthYear = (date: Date): string => {
  return date.toLocaleDateString('es-AR', { month: 'long', year: 'numeric' })
}

const formatSelectedDate = (dateStr: string): string => {
  const date = new Date(dateStr + 'T00:00:00')
  return date.toLocaleDateString('es-AR', { 
    weekday: 'long',
    day: 'numeric', 
    month: 'long',
    year: 'numeric'
  })
}

const getDayClasses = (day: CalendarDay): string => {
  const classes = []
  
  if (!day.isCurrentMonth) {
    classes.push('text-gray-300')
  } else if (!day.isSelectable) {
    classes.push('text-gray-300 cursor-not-allowed')
  } else {
    classes.push('text-gray-900 hover:bg-red-50 cursor-pointer')
  }
  
  if (day.isSelected && day.isSelectable) {
    classes.push('bg-red-500 text-white hover:bg-red-600')
  } else if (day.isToday && day.isSelectable) {
    classes.push('bg-blue-100 text-blue-800 font-semibold')
  }
  
  return classes.join(' ')
}

// Handle clicks outside
const handleClickOutside = (event: Event) => {
  const target = event.target as Element
  if (!target.closest('.relative')) {
    closeCalendar()
  }
}

// Update selected date when prop changes
const updateFromProp = () => {
  selectedDate.value = props.modelValue || ''
  
  // Set current view to selected date's month
  if (props.modelValue) {
    const date = new Date(props.modelValue + 'T00:00:00')
    currentDate.value = new Date(date.getFullYear(), date.getMonth(), 1)
  }
}

// Lifecycle
onMounted(() => {
  updateFromProp()
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<style scoped>
.calendar-fade-enter-active,
.calendar-fade-leave-active {
  transition: all 0.2s ease;
}

.calendar-fade-enter-from {
  opacity: 0;
  transform: translateY(-10px);
}

.calendar-fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>