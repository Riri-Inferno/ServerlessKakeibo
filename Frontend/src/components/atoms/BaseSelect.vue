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
  sm: "py-2 px-3 text-sm",
  md: "py-3 px-4 text-base",
  lg: "py-4 px-5 text-lg",
};

const selectClass = computed(() => {
  const base =
    "w-full rounded-lg border-2 transition-all duration-200 font-medium";
  const size = sizeClasses[props.size];

  const state = props.error
    ? "border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-200"
    : "border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200";

  const disabled = props.disabled
    ? "bg-gray-100 cursor-not-allowed opacity-60"
    : "bg-white";

  return `${base} ${size} ${state} ${disabled} focus:outline-none`;
});
</script>

<template>
  <select v-model="modelValue" :disabled="disabled" :class="selectClass">
    <option v-if="!hidePlaceholder" :value="null">{{ placeholder }}</option>
    <option v-for="option in options" :key="option.value" :value="option.value">
      {{ option.label }}
    </option>
  </select>
</template>
