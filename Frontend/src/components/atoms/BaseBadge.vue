<script setup lang="ts">
import { computed } from "vue";

type Color = "default" | "primary" | "success" | "danger" | "warning" | "gray";
type Size = "sm" | "md" | "lg";

interface Props {
  color?: Color;
  size?: Size;
}

const props = withDefaults(defineProps<Props>(), {
  color: "default",
  size: "md",
});

const colorClasses: Record<Color, string> = {
  default: "bg-gray-100 text-gray-800",
  primary: "bg-blue-100 text-blue-800",
  success: "bg-green-100 text-green-800",
  danger: "bg-red-100 text-red-800",
  warning: "bg-yellow-100 text-yellow-800",
  gray: "bg-gray-100 text-gray-600",
};

const sizeClasses: Record<Size, string> = {
  sm: "text-xs px-2 py-0.5",
  md: "text-sm px-2.5 py-1",
  lg: "text-base px-3 py-1.5",
};

const badgeClass = computed(() => {
  const base = "inline-flex items-center font-medium rounded-full";
  const color = colorClasses[props.color];
  const size = sizeClasses[props.size];

  return `${base} ${color} ${size}`;
});
</script>

<template>
  <span :class="badgeClass">
    <slot />
  </span>
</template>
