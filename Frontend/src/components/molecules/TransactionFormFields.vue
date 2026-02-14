<script setup lang="ts">
import { computed } from "vue";
import {
  TransactionType,
  TaxInclusionType,
  TaxInclusionTypeLabels,
} from "../../types/transaction";
import { PaymentMethodLabels } from "../../types/receipt";
import type { TransactionCategoryDto } from "../../types/transactionCategory";
import BaseText from "../atoms/BaseText.vue";
import BaseInput from "../atoms/BaseInput.vue";
import BaseInputNumber from "../atoms/BaseInputNumber.vue";
import BaseSelect from "../atoms/BaseSelect.vue";
import LabeledCheckbox from "./LabeledCheckbox.vue";

interface Props {
  type: TransactionType;
  transactionDate: string;
  amountTotal: number | null;
  payer: string;
  payee: string;
  category: string | null;
  paymentMethod: string;
  notes: string;
  taxInclusionType?: TaxInclusionType;
  calculatedTotal: number;
  isAutoCalculate: boolean;
  isTypeReadonly?: boolean;
  transactionCategories: TransactionCategoryDto[];
}

const props = withDefaults(defineProps<Props>(), {
  isTypeReadonly: false,
  taxInclusionType: undefined,
});

const emit = defineEmits<{
  "update:type": [value: TransactionType];
  "update:transactionDate": [value: string];
  "update:amountTotal": [value: number | null];
  "update:payer": [value: string];
  "update:payee": [value: string];
  "update:category": [value: string | null];
  "update:paymentMethod": [value: string];
  "update:notes": [value: string];
  "update:taxInclusionType": [value: TaxInclusionType | undefined];
  "update:isAutoCalculate": [value: boolean];
}>();

const typeOptions = [
  { value: TransactionType.Expense, label: "支出" },
  { value: TransactionType.Income, label: "収入" },
];

// カテゴリオプションを動的生成
const categoryOptions = computed(() =>
  props.transactionCategories.map((cat) => ({
    value: cat.id,
    label: cat.name,
  })),
);

const paymentMethodOptions = Object.entries(PaymentMethodLabels).map(
  ([key, label]) => ({
    value: key,
    label,
  }),
);

const taxInclusionTypeOptions = Object.entries(TaxInclusionTypeLabels).map(
  ([key, label]) => ({
    value: key,
    label,
  }),
);

const totalDifference = computed(() => {
  if (!props.amountTotal) return 0;
  return Math.abs(props.calculatedTotal - props.amountTotal);
});

const hasCalculationError = computed(() => {
  return totalDifference.value > 1;
});

// 自動計算のv-model用
const autoCalculate = computed({
  get: () => props.isAutoCalculate,
  set: (value: boolean) => emit("update:isAutoCalculate", value),
});
</script>

<template>
  <div class="space-y-4">
    <!-- 取引種別 -->
    <div>
      <BaseText variant="caption" color="gray" class="mb-1">取引種別</BaseText>
      <BaseSelect
        :model-value="type"
        @update:model-value="emit('update:type', $event as TransactionType)"
        :options="typeOptions"
        :disabled="isTypeReadonly"
        size="md"
      />
    </div>

    <!-- 取引日 -->
    <div>
      <BaseText variant="caption" color="gray" class="mb-1">取引日</BaseText>
      <BaseInput
        :model-value="transactionDate"
        @update:model-value="emit('update:transactionDate', $event as string)"
        type="date"
        size="md"
      />
    </div>

    <!-- 合計金額 -->
    <div>
      <div class="flex flex-wrap items-center justify-between gap-2 mb-1">
        <BaseText variant="caption" color="gray">合計金額</BaseText>
        <LabeledCheckbox v-model="autoCalculate" label="自動計算" size="sm" />
      </div>
      <BaseInputNumber
        :model-value="amountTotal"
        @update:model-value="
          emit('update:amountTotal', $event as number | null)
        "
        size="md"
        :disabled="isAutoCalculate"
      />
      <div
        v-if="calculatedTotal > 0"
        class="mt-1"
        :class="{
          'text-red-600': hasCalculationError,
          'text-gray-500': !hasCalculationError,
        }"
      >
        <BaseText variant="caption">
          明細合計: {{ calculatedTotal.toLocaleString() }}円
          <span v-if="hasCalculationError">
            （差額: {{ totalDifference.toLocaleString() }}円）
          </span>
        </BaseText>
      </div>
    </div>

    <!-- 支払者（収入時のみ有効） -->
    <div>
      <BaseText
        variant="caption"
        :color="type === TransactionType.Income ? 'gray' : 'muted'"
        class="mb-1"
      >
        支払者
        <span v-if="type !== TransactionType.Income" class="text-xs">
          （収入時のみ）
        </span>
      </BaseText>
      <BaseInput
        :model-value="payer"
        @update:model-value="emit('update:payer', $event as string)"
        type="text"
        placeholder="給与振込元など"
        size="md"
        :disabled="type !== TransactionType.Income"
        :class="{
          'opacity-50 cursor-not-allowed': type !== TransactionType.Income,
        }"
      />
    </div>

    <!-- 支払先（支出時のみ有効） -->
    <div>
      <BaseText
        variant="caption"
        :color="type === TransactionType.Expense ? 'gray' : 'muted'"
        class="mb-1"
      >
        支払先
        <span v-if="type !== TransactionType.Expense" class="text-xs">
          （支出時のみ）
        </span>
      </BaseText>
      <BaseInput
        :model-value="payee"
        @update:model-value="emit('update:payee', $event as string)"
        type="text"
        placeholder="店舗名など"
        size="md"
        :disabled="type !== TransactionType.Expense"
        :class="{
          'opacity-50 cursor-not-allowed': type !== TransactionType.Expense,
        }"
      />
    </div>

    <!-- カテゴリ -->
    <div>
      <BaseText variant="caption" color="gray" class="mb-1">カテゴリ</BaseText>
      <BaseSelect
        :model-value="category || ''"
        @update:model-value="
          emit('update:category', $event ? ($event as string) : null)
        "
        :options="categoryOptions"
        placeholder="選択してください"
        size="md"
      />
    </div>

    <!-- 支払方法 -->
    <div>
      <BaseText variant="caption" color="gray" class="mb-1">支払方法</BaseText>
      <BaseSelect
        :model-value="paymentMethod"
        @update:model-value="emit('update:paymentMethod', $event as string)"
        :options="paymentMethodOptions"
        placeholder="選択してください"
        size="md"
      />
    </div>

    <!-- 税区分 -->
    <div>
      <BaseText variant="caption" color="gray" class="mb-1">
        税区分
        <span class="text-xs text-gray-400">（任意）</span>
      </BaseText>
      <BaseSelect
        :model-value="taxInclusionType || ''"
        @update:model-value="
          emit(
            'update:taxInclusionType',
            $event ? ($event as TaxInclusionType) : undefined,
          )
        "
        :options="taxInclusionTypeOptions"
        placeholder="選択してください"
        size="md"
      />
    </div>

    <div>
      <BaseText variant="caption" color="gray" class="mb-1">メモ</BaseText>
      <textarea
        :value="notes"
        @input="
          emit('update:notes', ($event.target as HTMLTextAreaElement).value)
        "
        placeholder="備考・メモ"
        rows="3"
        class="w-full rounded-lg border-2 border-gray-300 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 focus:outline-none p-3 resize-none"
      ></textarea>
    </div>
  </div>
</template>
