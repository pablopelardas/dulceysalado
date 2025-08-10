<template>
  <div class="flex justify-center mt-10">
    <div class="glass px-6 py-4 rounded-xl">
      <div class="flex items-center justify-between bg-white px-4 py-3 border border-gray-200 rounded-lg animate-pulse">
        <!-- Mobile skeleton -->
        <div class="flex flex-1 justify-between sm:hidden">
          <div class="h-9 w-20 bg-gray-300 rounded-md"></div>
          <div class="h-9 w-20 bg-gray-300 rounded-md ml-3"></div>
        </div>
        
        <!-- Desktop skeleton -->
        <div class="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
          <div class="h-5 w-32 bg-gray-300 rounded"></div>
          
          <div class="flex items-center space-x-1">
            <!-- Previous button skeleton -->
            <div class="h-10 w-10 bg-gray-300 rounded-l-md"></div>
            
            <!-- Page numbers skeleton -->
            <div class="h-10 w-10 bg-gray-300"></div>
            <div class="h-10 w-10 bg-gray-400"></div>
            <div class="h-10 w-10 bg-gray-300"></div>
            <div class="h-10 w-10 bg-gray-300"></div>
            <div class="h-10 w-10 bg-gray-300"></div>
            
            <!-- Next button skeleton -->
            <div class="h-10 w-10 bg-gray-300 rounded-r-md"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Pagination skeleton component
</script>