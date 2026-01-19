<script setup lang="ts">
import { ref } from "vue";
import AppHeader from "../components/organisms/AppHeader.vue";
import AppSidebar from "../components/organisms/AppSidebar.vue";

const isSidebarOpen = ref(false);

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value;
};
</script>

<template>
  <div class="flex flex-col h-screen bg-gray-50">
    <AppHeader @toggle-sidebar="toggleSidebar" class="flex-shrink-0" />

    <div class="flex flex-1 overflow-hidden">
      <AppSidebar
        :is-open="isSidebarOpen"
        @close="isSidebarOpen = false"
        class="flex-shrink-0"
      />

      <main class="flex-1 overflow-y-auto p-4 md:p-6 lg:p-8">
        <slot />
      </main>
    </div>
  </div>
</template>
