<script setup lang="ts">
import { computed } from "vue";
import {
  TransactionType,
  TransactionCategory,
  CategoryLabels,
} from "../../types/transaction";
import { PaymentMethodLabels } from "../../types/receipt";
import BaseText from "../atoms/BaseText.vue";
import BaseInput from "../atoms/BaseInput.vue";
import BaseInputNumber from "../atoms/BaseInputNumber.vue";
import BaseSelect from "../atoms/BaseSelect.vue";

interface Props {
  type: TransactionType;
  transactionDate: string;
  amountTotal: number | null;
  payee: string;
  category: TransactionCategory;
  paymentMethod: string;
  notes: string;
  calculatedTotal: number;
  isAutoCalculate: boolean;
  isTypeReadonly?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  isTypeReadonly: false,
});

const emit = defineEmits<{
  "update:type": [value: TransactionType];
  "update:transactionDate": [value: string];
  "update:amountTotal": [value: number | null];
  "update:payee": [value: string];
  "update:category": [value: TransactionCategory];
  "update:paymentMethod": [value: string];
  "update:notes": [value: string];
  "update:isAutoCalculate": [value: boolean];
}>();

const typeOptions = [
  { value: TransactionType.Expense, label: "支出" },
  { value: TransactionType.Income, label: "収入" },
];

const categoryOptions = Object.entries(CategoryLabels).map(([key, label]) => ({
  value: key,
  label,
}));

const paymentMethodOptions = Object.entries(PaymentMethodLabels).map(
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
</script>

<template>
  <div class="space-y-4">
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

    <div>
      <BaseText variant="caption" color="gray" class="mb-1">取引日</BaseText>
      <BaseInput
        :model-value="transactionDate"
        @update:model-value="emit('update:transactionDate', $event as string)"
        type="date"
        size="md"
      />
    </div>

    <div>
      <div class="flex items-center justify-between mb-1">
        <BaseText variant="caption" color="gray">合計金額</BaseText>
        <label class="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            :checked="isAutoCalculate"
            @change="
              emit(
                'update:isAutoCalculate',
                ($event.target as HTMLInputElement).checked,
              )
            "
            class="rounded"
          />
          <BaseText variant="caption" color="gray">自動計算</BaseText>
        </label>
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

    <div>
      <BaseText variant="caption" color="gray" class="mb-1">支払先</BaseText>
      <BaseInput
        :model-value="payee"
        @update:model-value="emit('update:payee', $event as string)"
        type="text"
        placeholder="店舗名など"
        size="md"
      />
    </div>

    <div>
      <BaseText variant="caption" color="gray" class="mb-1">カテゴリ</BaseText>
      <BaseSelect
        :model-value="category"
        @update:model-value="
          emit('update:category', $event as TransactionCategory)
        "
        :options="categoryOptions"
        size="md"
      />
    </div>

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
