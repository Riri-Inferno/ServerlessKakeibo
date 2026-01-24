<script setup lang="ts">
import { computed } from "vue";
import BaseText from "./BaseText.vue";

/**
 * BaseCheckbox - 汎用チェックボックスコンポーネント
 *
 * TODO: カラーパレット確定後、色指定を調整
 */

type Size = "sm" | "md" | "lg";

interface Props {
  /** ラベルテキスト */
  label?: string;
  /** サイズ */
  size?: Size;
  /** 無効状態 */
  disabled?: boolean;
  /** 説明文（小さいグレーのテキスト） */
  description?: string;
}

const props = withDefaults(defineProps<Props>(), {
  label: "",
  size: "md",
  disabled: false,
  description: "",
});

const modelValue = defineModel<boolean>();

const sizeClasses: Record<Size, string> = {
  sm: "w-4 h-4",
  md: "w-5 h-5",
  lg: "w-6 h-6",
};

const checkboxClass = computed(() => {
  const base =
    "rounded border-2 transition-all duration-200 cursor-pointer focus:ring-2 focus:ring-blue-200 focus:outline-none";
  const size = sizeClasses[props.size];

  // TODO: カラーパレット確定後、色を調整
  const state = modelValue.value
    ? "bg-blue-600 border-blue-600 text-white"
    : "bg-white border-gray-300 hover:border-blue-500";

  const disabled = props.disabled
    ? "opacity-50 cursor-not-allowed"
    : "cursor-pointer";

  return `${base} ${size} ${state} ${disabled}`;
});

const labelClass = computed(() => {
  const base = "select-none";
  const cursor = props.disabled ? "cursor-not-allowed" : "cursor-pointer";
  return `${base} ${cursor}`;
});
</script>

<template>
  <label :class="`flex items-start gap-3 ${labelClass}`">
    <input
      v-model="modelValue"
      type="checkbox"
      :disabled="disabled"
      :class="checkboxClass"
    />
    <div v-if="label || description" class="flex-1">
      <BaseText v-if="label" variant="body" :class="labelClass">
        {{ label }}
      </BaseText>
      <BaseText v-if="description" variant="caption" color="gray" class="mt-1">
        {{ description }}
      </BaseText>
    </div>
    <slot />
  </label>
</template>
