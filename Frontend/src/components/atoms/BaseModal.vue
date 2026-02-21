<script setup lang="ts">
import { watch, onMounted, onUnmounted } from "vue";
import BaseIcon from "./BaseIcon.vue";

interface Props {
  isOpen: boolean;
  title?: string;
  closeOnClickOutside?: boolean;
  closeOnEscape?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  closeOnClickOutside: false,
  closeOnEscape: true,
});

const emit = defineEmits<{
  close: [];
}>();

const handleEscapeKey = (event: KeyboardEvent) => {
  if (props.isOpen && props.closeOnEscape && event.key === "Escape") {
    emit("close");
  }
};

onMounted(() => {
  document.addEventListener("keydown", handleEscapeKey);
});

onUnmounted(() => {
  document.removeEventListener("keydown", handleEscapeKey);
});

watch(
  () => props.isOpen,
  (newValue) => {
    if (newValue) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }
  },
);

const handleBackgroundClick = () => {
  if (props.closeOnClickOutside) {
    emit("close");
  }
};
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="isOpen"
        class="fixed inset-0 z-50 flex items-center justify-center py-6 px-3 md:p-4"
      >
        <div
          class="fixed inset-0 bg-black bg-opacity-50"
          @click="handleBackgroundClick"
        ></div>

        <div
          class="relative bg-white rounded-xl md:rounded-2xl shadow-2xl w-full max-w-2xl max-h-[88vh] md:max-h-[90vh] overflow-hidden flex flex-col"
        >
          <div
            class="flex items-center justify-between p-3 md:p-4 lg:p-6 border-b border-gray-200"
          >
            <h2 class="text-lg md:text-xl font-bold text-gray-900">{{ title }}</h2>
            <button
              @click="emit('close')"
              class="p-2 hover:bg-gray-100 rounded-lg transition-colors flex-shrink-0"
            >
              <BaseIcon name="x" size="md" />
            </button>
          </div>

          <div class="flex-1 overflow-y-auto p-3 md:p-4 lg:p-6">
            <slot />
          </div>

          <div v-if="$slots.footer" class="p-3 md:p-4 lg:p-6 border-t border-gray-200">
            <slot name="footer" />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
</style>
