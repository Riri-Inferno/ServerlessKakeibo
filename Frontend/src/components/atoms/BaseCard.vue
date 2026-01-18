<script setup lang="ts">
import { computed } from "vue";

/**
 * BaseCard - カードコンテナ
 *
 * ログインボックス、ダッシュボードのカード、リストアイテムなどに使用
 *
 * TODO: カラーパレット確定後、影やホバー時の色を調整
 */

type Padding = "sm" | "md" | "lg" | "none";

interface Props {
  /** 内側の余白 */
  padding?: Padding;
  /** クリック可能（ホバー効果あり） */
  clickable?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  padding: "md",
  clickable: false,
});

const paddingClasses: Record<Padding, string> = {
  none: "",
  sm: "p-4",
  md: "p-6",
  lg: "p-8",
};

const cardClass = computed(() => {
  const base = "bg-white rounded-2xl shadow-lg transition-all duration-200";
  const padding = paddingClasses[props.padding];
  const clickable = props.clickable
    ? "cursor-pointer hover:shadow-xl hover:-translate-y-1"
    : "";

  return `${base} ${padding} ${clickable}`;
});
</script>

<template>
  <div :class="cardClass">
    <slot />
  </div>
</template>
