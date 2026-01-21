<script setup lang="ts">
import { useRouter, useRoute } from "vue-router";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseText from "../atoms/BaseText.vue";

interface Props {
  isOpen?: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  close: [];
}>();

const router = useRouter();
const route = useRoute();

const menuItems = [
  { name: "dashboard", label: "ダッシュボード", icon: "home" },
  { name: "transactions", label: "取引一覧", icon: "currency-yen" },
  { name: "stats", label: "統計", icon: "chart" },
  { name: "settings", label: "設定", icon: "settings" },
];

const navigate = (routeName: string) => {
  router.push({ name: routeName });
  emit("close");
};

const isActive = (routeName: string) => {
  return route.name === routeName;
};
</script>

<template>
  <div
    class="fixed inset-0 bg-black bg-opacity-50 z-40 md:hidden"
    v-if="isOpen"
    @click="emit('close')"
  ></div>

  <aside
    class="fixed md:sticky top-0 left-0 bg-white shadow-lg z-50 transition-transform duration-300 md:translate-x-0 flex flex-col"
    :class="{
      'translate-x-0': isOpen,
      '-translate-x-full': !isOpen,
      'w-4/5 max-w-xs': true,
      'md:w-56 lg:w-64': true,
      'h-screen': true,
    }"
  >
    <div
      class="flex items-center justify-between p-4 border-b border-gray-200 md:hidden"
    >
      <BaseText variant="h3">メニュー</BaseText>
      <button @click="emit('close')" class="p-2 hover:bg-gray-100 rounded-lg">
        <BaseIcon name="x" size="md" />
      </button>
    </div>

    <nav class="flex-1 overflow-y-auto p-4">
      <ul class="space-y-2">
        <li v-for="item in menuItems" :key="item.name">
          <button
            @click="navigate(item.name)"
            class="flex items-center gap-3 w-full px-4 py-3 rounded-lg transition-colors"
            :class="{
              'bg-blue-50 text-blue-600': isActive(item.name),
              'hover:bg-gray-100 text-gray-700': !isActive(item.name),
            }"
          >
            <BaseIcon :name="item.icon as any" size="md" />
            <BaseText
              variant="body"
              :weight="isActive(item.name) ? 'bold' : 'normal'"
            >
              {{ item.label }}
            </BaseText>
          </button>
        </li>
      </ul>
    </nav>
  </aside>
</template>
