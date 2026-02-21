<script setup lang="ts">
import { useRouter, useRoute } from "vue-router";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseText from "../atoms/BaseText.vue";

const router = useRouter();
const route = useRoute();

const navItems = [
  { name: "dashboard", label: "ホーム", icon: "home" },
  { name: "transactions", label: "取引", icon: "currency-yen" },
  { name: "stats", label: "統計", icon: "chart" },
  { name: "settings", label: "設定", icon: "settings" },
];

const isActive = (routeName: string) => {
  return route.name === routeName;
};

const navigate = (routeName: string) => {
  router.push({ name: routeName });
};
</script>

<template>
  <nav
    class="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 shadow-lg z-40 md:hidden"
  >
    <div class="flex justify-around items-center h-16">
      <button
        v-for="item in navItems"
        :key="item.name"
        @click="navigate(item.name)"
        class="flex flex-col items-center justify-center flex-1 py-2 transition-colors"
        :class="{
          'text-blue-600': isActive(item.name),
          'text-gray-500': !isActive(item.name),
        }"
      >
        <BaseIcon :name="item.icon as any" size="md" />
        <BaseText
          variant="caption"
          :color="isActive(item.name) ? 'primary' : 'gray'"
          :weight="isActive(item.name) ? 'bold' : 'normal'"
          class="mt-1"
        >
          {{ item.label }}
        </BaseText>
      </button>
    </div>
  </nav>
</template>
