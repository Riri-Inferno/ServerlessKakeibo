<script setup lang="ts">
import { computed } from "vue";

/**
 * BaseButton - 汎用ボタンコンポーネント
 *
 * TODO: カラーパレット確定後、色指定を調整
 */

type Variant = "primary" | "secondary" | "outline" | "danger" | "text";
type Size = "sm" | "md" | "lg";

interface Props {
  /** ボタンの種類 */
  variant?: Variant;
  /** サイズ */
  size?: Size;
  /** 無効状態 */
  disabled?: boolean;
  /** 全幅表示 */
  fullWidth?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  variant: "primary",
  size: "md",
  disabled: false,
  fullWidth: false,
});

// TODO: カラーパレット確定後、variantの色を調整
const variantClasses: Record<Variant, string> = {
  primary: "bg-blue-600 hover:bg-blue-700 text-white",
  secondary: "bg-gray-600 hover:bg-gray-700 text-white",
  outline:
    "border-2 border-gray-300 hover:border-gray-400 bg-white text-gray-700",
  danger: "bg-red-600 hover:bg-red-700 text-white",
  text: "bg-transparent hover:bg-gray-100 text-gray-700",
};

const sizeClasses: Record<Size, string> = {
  sm: "py-2 px-3 text-sm",
  md: "py-3 px-4 text-base",
  lg: "py-4 px-6 text-lg",
};

const buttonClass = computed(() => {
  const base = "font-medium rounded-lg transition-all duration-200";
  const variant = variantClasses[props.variant];
  const size = sizeClasses[props.size];
  const width = props.fullWidth ? "w-full" : "";
  const disabled = props.disabled
    ? "opacity-50 cursor-not-allowed"
    : "cursor-pointer";

  return `${base} ${variant} ${size} ${width} ${disabled}`;
});
</script>

<template>
  <button :class="buttonClass" :disabled="disabled">
    <slot />
  </button>
</template>
