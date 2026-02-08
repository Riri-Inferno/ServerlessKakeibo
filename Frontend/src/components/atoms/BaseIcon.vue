<script setup lang="ts">
import { computed, type Component } from "vue";

// ===== Outline (24x24) =====
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
  FolderIcon,
  PhotoIcon,
  DocumentTextIcon,
  ArrowDownTrayIcon,
  ArrowPathIcon,
  CalendarIcon,
  TagIcon,
  BanknotesIcon,
  ClipboardDocumentListIcon,
  CheckCircleIcon,
  ArrowUpIcon,
  ArrowDownIcon,
  ChevronRightIcon,
  ChevronLeftIcon,
  Bars3Icon,
} from "@heroicons/vue/24/outline";

// ===== Solid (24x24) =====
import {
  ExclamationTriangleIcon as ExclamationTriangleIconSolid,
  CheckCircleIcon as CheckCircleIconSolid,
  InformationCircleIcon as InformationCircleIconSolid,
  PhotoIcon as PhotoIconSolid,
  FolderIcon as FolderIconSolid,
} from "@heroicons/vue/24/solid";

// ===== Mini (20x20) =====
import {
  ExclamationTriangleIcon as ExclamationTriangleIconMini,
  CheckCircleIcon as CheckCircleIconMini,
  InformationCircleIcon as InformationCircleIconMini,
} from "@heroicons/vue/20/solid";

/**
 * BaseIcon - heroicons アイコン表示専用
 *
 * heroicons のアイコンを統一的に扱うためのラッパーコンポーネント
 *
 * バリアント:
 * - outline: 線のみ（デフォルト）
 * - solid: 塗りつぶし（24x24）
 * - mini: 小さい塗りつぶし（20x20）
 */

type IconName =
  | "plus"
  | "minus"
  | "x"
  | "x-mark"
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
  | "bars-3"
  | "pencil"
  | "trash"
  | "check"
  | "warning"
  | "exclamation-triangle"
  | "info"
  | "alert"
  | "currency-yen"
  | "folder"
  | "photo"
  | "document"
  | "download"
  | "refresh"
  | "arrow-path"
  | "calendar"
  | "tag"
  | "banknotes"
  | "clipboard-list"
  | "check-circle"
  | "arrow-up"
  | "arrow-down"
  | "chevron-left"
  | "chevron-right";

type Size = "sm" | "md" | "lg" | "xl";
type Variant = "outline" | "solid" | "mini";

interface Props {
  /** アイコン名 */
  name: IconName;
  /** サイズ */
  size?: Size;
  /** バリアント（outline: 線のみ, solid: 塗りつぶし, mini: 小さい塗りつぶし） */
  variant?: Variant;
}

const props = withDefaults(defineProps<Props>(), {
  size: "md",
  variant: "outline",
});

// Outline (24x24)
const iconMapOutline: Record<IconName, Component> = {
  plus: PlusIcon,
  minus: MinusIcon,
  x: XMarkIcon,
  "x-mark": XMarkIcon,
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
  "bars-3": Bars3Icon,
  pencil: PencilIcon,
  trash: TrashIcon,
  check: CheckIcon,
  warning: ExclamationTriangleIcon,
  "exclamation-triangle": ExclamationTriangleIcon,
  info: InformationCircleIcon,
  "currency-yen": CurrencyYenIcon,
  folder: FolderIcon,
  photo: PhotoIcon,
  document: DocumentTextIcon,
  download: ArrowDownTrayIcon,
  refresh: ArrowPathIcon,
  "arrow-path": ArrowPathIcon,
  calendar: CalendarIcon,
  tag: TagIcon,
  banknotes: BanknotesIcon,
  "clipboard-list": ClipboardDocumentListIcon,
  "check-circle": CheckCircleIcon,
  "arrow-up": ArrowUpIcon,
  "arrow-down": ArrowDownIcon,
  alert: ExclamationTriangleIcon,
  "chevron-left": ChevronLeftIcon,
  "chevron-right": ChevronRightIcon,
};

// Solid (24x24) - 必要に応じて追加
const iconMapSolid: Record<IconName, Component> = {
  warning: ExclamationTriangleIconSolid,
  "exclamation-triangle": ExclamationTriangleIconSolid,
  "check-circle": CheckCircleIconSolid,
  info: InformationCircleIconSolid,
  photo: PhotoIconSolid,
  folder: FolderIconSolid,
  plus: PlusIcon,
  minus: MinusIcon,
  x: XMarkIcon,
  "x-mark": XMarkIcon,
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
  "bars-3": Bars3Icon,
  pencil: PencilIcon,
  trash: TrashIcon,
  check: CheckIcon,
  "currency-yen": CurrencyYenIcon,
  document: DocumentTextIcon,
  download: ArrowDownTrayIcon,
  refresh: ArrowPathIcon,
  "arrow-path": ArrowPathIcon,
  calendar: CalendarIcon,
  tag: TagIcon,
  banknotes: BanknotesIcon,
  "clipboard-list": ClipboardDocumentListIcon,
  "arrow-up": ArrowUpIcon,
  "arrow-down": ArrowDownIcon,
  alert: ExclamationTriangleIconSolid,
  "chevron-left": ChevronLeftIcon,
  "chevron-right": ChevronRightIcon,
};

// Mini (20x20) - よく使うものだけ
const iconMapMini: Record<IconName, Component> = {
  warning: ExclamationTriangleIconMini,
  "exclamation-triangle": ExclamationTriangleIconMini,
  "check-circle": CheckCircleIconMini,
  info: InformationCircleIconMini,
  plus: PlusIcon,
  minus: MinusIcon,
  x: XMarkIcon,
  "x-mark": XMarkIcon,
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
  "bars-3": Bars3Icon,
  pencil: PencilIcon,
  trash: TrashIcon,
  check: CheckIcon,
  "currency-yen": CurrencyYenIcon,
  folder: FolderIcon,
  photo: PhotoIcon,
  document: DocumentTextIcon,
  download: ArrowDownTrayIcon,
  refresh: ArrowPathIcon,
  "arrow-path": ArrowPathIcon,
  calendar: CalendarIcon,
  tag: TagIcon,
  banknotes: BanknotesIcon,
  "clipboard-list": ClipboardDocumentListIcon,
  "arrow-up": ArrowUpIcon,
  "arrow-down": ArrowDownIcon,
  alert: ExclamationTriangleIconMini,
  "chevron-left": ChevronLeftIcon,
  "chevron-right": ChevronRightIcon,
};

const iconComponent = computed(() => {
  switch (props.variant) {
    case "solid":
      return iconMapSolid[props.name];
    case "mini":
      return iconMapMini[props.name];
    case "outline":
    default:
      return iconMapOutline[props.name];
  }
});

const sizeClasses: Record<Size, string> = {
  sm: "w-4 h-4",
  md: "w-5 h-5",
  lg: "w-6 h-6",
  xl: "w-8 h-8",
};

const iconClass = computed(() => sizeClasses[props.size]);
</script>

<template>
  <component :is="iconComponent" :class="iconClass" />
</template>
