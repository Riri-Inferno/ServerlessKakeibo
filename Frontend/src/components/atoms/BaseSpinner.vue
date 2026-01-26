<script setup lang="ts">
import { computed } from "vue";
import { ArrowPathIcon, Cog6ToothIcon } from "@heroicons/vue/24/outline";
import type { Component } from "vue";

type Size = "sm" | "md" | "lg" | "xl";
type Color =
  | "primary"
  | "secondary"
  | "success"
  | "danger"
  | "warning"
  | "info"
  | "gray"
  | "white";
type IconType = "refresh" | "settings";

interface Props {
  size?: Size;
  color?: Color;
  icon?: IconType;
  label?: string;
}

const props = withDefaults(defineProps<Props>(), {
  size: "md",
  color: "primary",
  icon: "refresh",
  label: "読み込み中",
});

const iconMap: Record<IconType, Component> = {
  refresh: ArrowPathIcon,
  settings: Cog6ToothIcon,
};

const iconComponent = computed(() => iconMap[props.icon]);

const sizeClasses: Record<Size, string> = {
  sm: "w-4 h-4",
  md: "w-5 h-5",
  lg: "w-6 h-6",
  xl: "w-8 h-8",
};

const colorClasses: Record<Color, string> = {
  primary: "text-blue-500",
  secondary: "text-gray-500",
  success: "text-green-500",
  danger: "text-red-500",
  warning: "text-yellow-500",
  info: "text-blue-400",
  gray: "text-gray-400",
  white: "text-white",
};

const iconClass = computed(() =>
  [sizeClasses[props.size], colorClasses[props.color], "animate-spin"].join(
    " ",
  ),
);
</script>

<template>
  <div
    class="inline-flex items-center justify-center"
    role="status"
    :aria-label="label"
  >
    <component :is="iconComponent" :class="iconClass" />
    <span class="sr-only">{{ label }}</span>
  </div>
</template>
