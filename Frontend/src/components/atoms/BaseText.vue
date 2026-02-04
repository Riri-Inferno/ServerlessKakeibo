<script setup lang="ts">
import { computed } from "vue";

/**
 * BaseText - 汎用テキストコンポーネント
 *
 * 家計簿アプリで使用する各種テキスト表示
 *
 * TODO: カラーパレット確定後、色指定を調整
 */

type Variant = "h1" | "h2" | "h3" | "body" | "caption" | "amount";
type Color = "default" | "primary" | "success" | "danger" | "gray" | "muted";
type Weight = "normal" | "medium" | "bold";

interface Props {
  /** テキストの種類 */
  variant?: Variant;
  /** 文字色 */
  color?: Color;
  /** 文字の太さ */
  weight?: Weight;
  /** HTML要素のタグ */
  as?: string;
}

const props = withDefaults(defineProps<Props>(), {
  variant: "body",
  color: "default",
  weight: "normal",
  as: undefined,
});

// variant ごとのデフォルトスタイル
const variantClasses: Record<Variant, string> = {
  h1: "text-3xl font-bold",
  h2: "text-2xl font-bold",
  h3: "text-xl font-semibold",
  body: "text-base",
  caption: "text-sm",
  amount: "text-2xl font-bold",
};

// TODO: カラーパレット確定後、色を調整
const colorClasses: Record<Color, string> = {
  default: "text-gray-900",
  primary: "text-blue-600",
  success: "text-green-600", // 収入用
  danger: "text-red-600", // 支出用
  gray: "text-gray-500",
  muted: "text-gray-400",
};

const weightClasses: Record<Weight, string> = {
  normal: "font-normal",
  medium: "font-medium",
  bold: "font-bold",
};

// variant に応じたデフォルトのHTMLタグ
const defaultTag: Record<Variant, string> = {
  h1: "h1",
  h2: "h2",
  h3: "h3",
  body: "p",
  caption: "span",
  amount: "span",
};

const component = computed(() => props.as || defaultTag[props.variant]);

const textClass = computed(() => {
  const variant = variantClasses[props.variant];
  const color = colorClasses[props.color];
  const weight = weightClasses[props.weight];

  return `${variant} ${color} ${weight}`;
});
</script>

<template>
  <component :is="component" :class="textClass">
    <slot />
  </component>
</template>
