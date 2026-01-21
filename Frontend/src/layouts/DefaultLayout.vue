<script setup lang="ts">
import { ref } from "vue";
import AppHeader from "../components/organisms/AppHeader.vue";
import AppSidebar from "../components/organisms/AppSidebar.vue";
import AppBottomNav from "../components/organisms/AppBottomNav.vue";

const isSidebarOpen = ref(false);

const toggleSidebar = () => {
  isSidebarOpen.value = !isSidebarOpen.value;
};
</script>

<template>
  <div class="h-screen bg-gray-50 flex flex-col overflow-hidden">
    <AppHeader @toggle-sidebar="toggleSidebar" class="flex-shrink-0" />

    <div class="flex flex-1 overflow-hidden min-h-0">
      <AppSidebar
        :is-open="isSidebarOpen"
        @close="isSidebarOpen = false"
        class="flex-shrink-0"
      />

      <main class="flex-1 overflow-y-auto p-4 md:p-6 lg:p-8 pb-24 md:pb-6">
        <slot />
      </main>
    </div>

    <AppBottomNav class="flex-shrink-0" />
  </div>
</template>
