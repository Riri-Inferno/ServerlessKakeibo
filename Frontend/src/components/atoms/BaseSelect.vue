<script setup lang="ts">
import { computed } from "vue";

type Size = "sm" | "md" | "lg";

interface Option {
  value: string | number;
  label: string;
}

interface Props {
  options: Option[];
  size?: Size;
  placeholder?: string;
  disabled?: boolean;
  error?: boolean;
  hidePlaceholder?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  size: "md",
  placeholder: "選択してください",
  disabled: false,
  error: false,
  hidePlaceholder: false,
});

const modelValue = defineModel<string | number | null>();

const sizeClasses: Record<Size, string> = {
  sm: "py-2 pl-3 pr-10 text-sm",
  md: "py-3 pl-4 pr-10 text-base",
  lg: "py-4 pl-5 pr-12 text-lg",
};

const selectClass = computed(() => {
  const base =
    "w-full rounded-lg border-2 transition-all duration-200 font-medium appearance-none bg-white";
  const size = sizeClasses[props.size];

  const state = props.error
    ? "border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-200"
    : "border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200";

  const disabled = props.disabled
    ? "bg-gray-100 cursor-not-allowed opacity-60"
    : "";

  return `${base} ${size} ${state} ${disabled} focus:outline-none`;
});
</script>

<template>
  <div class="relative">
    <select v-model="modelValue" :disabled="disabled" :class="selectClass">
      <option v-if="!hidePlaceholder" :value="null">{{ placeholder }}</option>
      <option
        v-for="option in options"
        :key="option.value"
        :value="option.value"
      >
        {{ option.label }}
      </option>
    </select>

    <!-- カスタム矢印アイコン -->
    <div
      class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none"
    >
      <svg
        class="w-5 h-5 text-gray-400"
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 20 20"
        fill="currentColor"
      >
        <path
          fill-rule="evenodd"
          d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z"
          clip-rule="evenodd"
        />
      </svg>
    </div>
  </div>
</template>
