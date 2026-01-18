<script setup lang="ts">
import { computed, type Component } from "vue";
import {
  PlusIcon,
  MinusIcon,
  XMarkIcon,
  UserIcon,
  HomeIcon,
  ShoppingCartIcon,
  CurrencyYenIcon,
  ChartBarIcon,
  Cog6ToothIcon,
  ArrowRightIcon,
  ArrowLeftIcon,
  ChevronDownIcon,
  ChevronUpIcon,
  PencilIcon,
  TrashIcon,
  CheckIcon,
  ExclamationTriangleIcon,
  InformationCircleIcon,
} from "@heroicons/vue/24/outline";

/**
 * BaseIcon - heroicons アイコン表示専用
 *
 * heroicons のアイコンを統一的に扱うためのラッパーコンポーネント
 *
 * TODO: 必要に応じてアイコンを追加
 */

type IconName =
  | "plus"
  | "minus"
  | "x"
  | "user"
  | "home"
  | "cart"
  | "yen"
  | "chart"
  | "settings"
  | "arrow-right"
  | "arrow-left"
  | "chevron-down"
  | "chevron-up"
  | "pencil"
  | "trash"
  | "check"
  | "warning"
  | "info";

type Size = "sm" | "md" | "lg" | "xl";

interface Props {
  /** アイコン名 */
  name: IconName;
  /** サイズ */
  size?: Size;
}

const props = withDefaults(defineProps<Props>(), {
  size: "md",
});

// アイコン名とコンポーネントのマッピング
const iconMap: Record<IconName, Component> = {
  plus: PlusIcon,
  minus: MinusIcon,
  x: XMarkIcon,
  user: UserIcon,
  home: HomeIcon,
  cart: ShoppingCartIcon,
  yen: CurrencyYenIcon,
  chart: ChartBarIcon,
  settings: Cog6ToothIcon,
  "arrow-right": ArrowRightIcon,
  "arrow-left": ArrowLeftIcon,
  "chevron-down": ChevronDownIcon,
  "chevron-up": ChevronUpIcon,
  pencil: PencilIcon,
  trash: TrashIcon,
  check: CheckIcon,
  warning: ExclamationTriangleIcon,
  info: InformationCircleIcon,
};

const sizeClasses: Record<Size, string> = {
  sm: "w-4 h-4",
  md: "w-5 h-5",
  lg: "w-6 h-6",
  xl: "w-8 h-8",
};

const iconComponent = computed(() => iconMap[props.name]);
const iconClass = computed(() => sizeClasses[props.size]);
</script>

<template>
  <component :is="iconComponent" :class="iconClass" />
</template>
