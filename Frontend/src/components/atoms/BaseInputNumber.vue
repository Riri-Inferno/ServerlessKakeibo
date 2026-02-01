<script setup lang="ts">
import { computed, ref, watch } from "vue";

/**
 * BaseInputNumber - 金額入力専用フィールド
 *
 * 機能:
 * - ¥マーク表示
 * - 3桁カンマ区切り
 * - v-model は数値（number）で扱う
 * - 数字以外の入力を完全にブロック
 *
 * TODO: カラーパレット確定後、エラー色を調整
 */

type Size = "sm" | "md" | "lg";

interface Props {
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
  size: "md",
  placeholder: "0",
  disabled: false,
  readonly: false,
  required: false,
  error: false,
});

// v-model は number 型（undefined も許容）
const modelValue = defineModel<number | null | undefined>();

// 表示用の文字列（カンマ区切り）
const displayValue = ref("");

// フォーカス状態を管理
const isFocused = ref(false);

// 数値をカンマ区切りに整形
const formatNumber = (value: number | null | undefined): string => {
  if (value === null || value === undefined || isNaN(value)) return "";
  return value.toLocaleString("ja-JP");
};

// カンマ区切り文字列を数値に変換
const parseNumber = (value: string): number | null => {
  const cleaned = value.replace(/[¥,]/g, "");
  if (cleaned === "") return null;
  const num = Number(cleaned);
  return isNaN(num) ? null : num;
};

// 初期値を表示用に整形
watch(
  () => modelValue.value,
  (newValue) => {
    if (!isFocused.value) {
      displayValue.value = formatNumber(newValue);
    }
  },
  { immediate: true },
);

// 入力時の処理
const handleInput = (event: Event) => {
  const input = event.target as HTMLInputElement;
  let value = input.value;

  // 数字以外を除去
  value = value.replace(/[^\d]/g, "");

  // 入力フィールドを更新（即座に反映）
  input.value = value;

  // 数値に変換
  const numValue = parseNumber(value);
  modelValue.value = numValue;

  // フォーカス中は数値のまま表示
  displayValue.value = value;
};

// キー入力制御：数字以外を入力させない
const handleKeydown = (event: KeyboardEvent) => {
  const key = event.key;

  // 許可するキー
  const allowedKeys = [
    "Backspace",
    "Delete",
    "Tab",
    "Enter",
    "ArrowLeft",
    "ArrowRight",
    "ArrowUp",
    "ArrowDown",
    "Home",
    "End",
  ];

  // Ctrl/Cmd + A, C, V, X (全選択、コピー、ペースト、カット)
  if (
    (event.ctrlKey || event.metaKey) &&
    ["a", "c", "v", "x"].includes(key.toLowerCase())
  ) {
    return;
  }

  // 数字または許可キーの場合は通過
  if (/^\d$/.test(key) || allowedKeys.includes(key)) {
    return;
  }

  // それ以外は入力を防ぐ
  event.preventDefault();
};

// ペースト制御：数字以外を除去
const handlePaste = (event: ClipboardEvent) => {
  event.preventDefault();

  const pasteData = event.clipboardData?.getData("text") || "";
  const numbersOnly = pasteData.replace(/[^\d]/g, "");

  if (numbersOnly) {
    const numValue = parseNumber(numbersOnly);
    modelValue.value = numValue;
    displayValue.value = numbersOnly;
  }
};

// フォーカス時：カンマを除去して編集しやすく
const handleFocus = (event: Event) => {
  isFocused.value = true;
  const input = event.target as HTMLInputElement;
  if (modelValue.value !== null && modelValue.value !== undefined) {
    displayValue.value = String(modelValue.value);
    input.value = String(modelValue.value);
  } else {
    displayValue.value = "";
    input.value = "";
  }
};

// フォーカスアウト時：カンマ区切りに戻す
const handleBlur = (event: Event) => {
  isFocused.value = false;
  const input = event.target as HTMLInputElement;
  const formatted = formatNumber(modelValue.value);
  displayValue.value = formatted;
  input.value = formatted;
};

const sizeClasses: Record<Size, string> = {
  sm: "py-2 pl-8 pr-3 text-sm",
  md: "py-3 pl-10 pr-4 text-base",
  lg: "py-4 pl-12 pr-5 text-lg",
};

const iconSizeClasses: Record<Size, string> = {
  sm: "left-3 text-sm",
  md: "left-3 text-base",
  lg: "left-4 text-lg",
};

const inputClass = computed(() => {
  const base =
    "w-full rounded-lg border-2 transition-all duration-200 font-medium text-right appearance-none"; // appearance-none追加
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
  <div class="relative">
    <!-- ¥マーク -->
    <span
      :class="[
        'absolute top-1/2 -translate-y-1/2 text-gray-500 font-medium pointer-events-none',
        iconSizeClasses[size],
      ]"
    >
      ¥
    </span>

    <!-- 入力フィールド -->
    <input
      :value="displayValue"
      type="text"
      inputmode="numeric"
      :placeholder="placeholder"
      :disabled="disabled"
      :readonly="readonly"
      :required="required"
      :class="inputClass"
      @input="handleInput"
      @keydown="handleKeydown"
      @paste="handlePaste"
      @focus="handleFocus"
      @blur="handleBlur"
    />
  </div>
</template>
