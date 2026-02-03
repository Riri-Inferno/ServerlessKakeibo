<script setup lang="ts">
import { computed } from "vue";
import { CheckIcon } from "@heroicons/vue/24/outline";
import BaseText from "../atoms/BaseText.vue";

/**
 * LabeledCheckbox - 汎用チェックボックスコンポーネント
 *
 * チェックボックス本体（Atom相当）とラベルテキスト（BaseText）を
 * 組み合わせたMoleculeコンポーネント
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

const iconSizeClasses: Record<Size, string> = {
  sm: "w-3 h-3",
  md: "w-4 h-4",
  lg: "w-5 h-5",
};

const checkboxContainerClass = computed(() => {
  const base =
    "rounded border-2 transition-all duration-200 flex items-center justify-center flex-shrink-0";
  const size = sizeClasses[props.size];

  // TODO: カラーパレット確定後、色を調整
  const state = modelValue.value
    ? "bg-blue-600 border-blue-600"
    : "bg-white border-gray-300 hover:border-blue-500";

  const cursor = props.disabled
    ? "opacity-50 cursor-not-allowed"
    : "cursor-pointer";

  return `${base} ${size} ${state} ${cursor}`;
});

const labelClass = computed(() => {
  const base = "select-none";
  const cursor = props.disabled ? "cursor-not-allowed" : "cursor-pointer";
  return `${base} ${cursor}`;
});

// 説明文がある場合は上揃え、ない場合は中央揃え
const containerAlignClass = computed(() => {
  return props.description ? "items-start" : "items-center";
});
</script>

<template>
  <label :class="`flex gap-3 ${containerAlignClass} ${labelClass}`">
    <input
      v-model="modelValue"
      type="checkbox"
      :disabled="disabled"
      class="sr-only"
    />

    <!-- チェックボックス本体（説明文がある場合は少し下げる） -->
    <div :class="[checkboxContainerClass, description ? 'mt-0.5' : '']">
      <CheckIcon
        v-if="modelValue"
        :class="iconSizeClasses[size]"
        class="text-white stroke-[3]"
      />
    </div>

    <div v-if="label || description" class="flex-1">
      <BaseText v-if="label" variant="body" :class="labelClass">
        {{ label }}
      </BaseText>
      <BaseText v-if="description" variant="caption" color="gray" class="mt-1">
        {{ description }}
      </BaseText>
    </div>

    <!-- 追加コンテンツ用スロット -->
    <slot />
  </label>
</template>
