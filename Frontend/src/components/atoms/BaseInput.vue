<script setup lang="ts">
import { computed } from "vue";

/**
 * BaseInput - 汎用入力フィールド
 * text, email, password, date などに対応
 *
 * 金額入力は BaseInputNumber を使用すること
 * TODO: カラーパレット確定後、エラー色を調整
 */

type InputType = "text" | "email" | "password" | "date" | "tel";
type Size = "sm" | "md" | "lg";

interface Props {
  /** input type */
  type?: InputType;
  /** サイズ */
  size?: Size;
  /** プレースホルダー */
  placeholder?: string;
  /** 無効状態 */
  disabled?: boolean;
  /** 読み取り専用 */
  readonly?: boolean;
  /** 必須項目 */
  required?: boolean;
  /** エラー状態 */
  error?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  type: "text",
  size: "md",
  placeholder: "",
  disabled: false,
  readonly: false,
  required: false,
  error: false,
});

const modelValue = defineModel<string>();

const sizeClasses: Record<Size, string> = {
  sm: "py-2 px-3 text-sm",
  md: "py-3 px-4 text-base",
  lg: "py-4 px-5 text-lg",
};

const inputClass = computed(() => {
  const base =
    "w-full rounded-lg border-2 transition-all duration-200 font-medium";
  const size = sizeClasses[props.size];

  // TODO: カラーパレット確定後、色を調整
  const state = props.error
    ? "border-red-500 focus:border-red-600 focus:ring-2 focus:ring-red-200"
    : "border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200";

  const disabled =
    props.disabled || props.readonly
      ? "bg-gray-100 cursor-not-allowed"
      : "bg-white";

  return `${base} ${size} ${state} ${disabled} focus:outline-none`;
});
</script>

<template>
  <input
    v-model="modelValue"
    :type="type"
    :placeholder="placeholder"
    :disabled="disabled"
    :readonly="readonly"
    :required="required"
    :class="inputClass"
  />
</template>
