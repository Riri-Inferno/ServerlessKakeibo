<script setup lang="ts">
import { computed } from "vue";
import markUrl from "../../assets/icons/selene-mark.svg?url";

type Variant = "mark" | "horizontal" | "wordmark";
type Size = "sm" | "md" | "lg" | "xl" | "2xl";

interface Props {
  variant?: Variant;
  size?: Size;
  alt?: string;
}

const props = withDefaults(defineProps<Props>(), {
  variant: "horizontal",
  size: "md",
  alt: "SELENE",
});

const markSizeClass = computed(() => {
  const sizes: Record<Size, string> = {
    sm: "h-5 w-5",
    md: "h-7 w-7",
    lg: "h-10 w-10",
    xl: "h-14 w-14",
    "2xl": "h-20 w-20",
  };
  return sizes[props.size];
});

const wordmarkClass = computed(() => {
  const sizes: Record<Size, string> = {
    sm: "text-base",
    md: "text-xl",
    lg: "text-2xl",
    xl: "text-3xl",
    "2xl": "text-4xl",
  };
  return sizes[props.size];
});

const gapClass = computed(() => {
  const sizes: Record<Size, string> = {
    sm: "gap-1.5",
    md: "gap-2",
    lg: "gap-2.5",
    xl: "gap-3",
    "2xl": "gap-4",
  };
  return sizes[props.size];
});
</script>

<template>
  <img
    v-if="variant === 'mark'"
    :src="markUrl"
    :alt="alt"
    :class="markSizeClass"
    draggable="false"
  />

  <span
    v-else-if="variant === 'wordmark'"
    :class="[wordmarkClass, 'font-bold tracking-[0.2em] text-indigo-950 leading-none inline-block']"
    :aria-label="alt"
  >SELENE</span>

  <span
    v-else
    :class="['inline-flex items-center whitespace-nowrap leading-none', gapClass]"
    :aria-label="alt"
    role="img"
  >
    <img :src="markUrl" alt="" :class="markSizeClass" draggable="false" />
    <span
      :class="[wordmarkClass, 'font-bold tracking-[0.2em] text-indigo-950 leading-none']"
    >SELENE</span>
  </span>
</template>
