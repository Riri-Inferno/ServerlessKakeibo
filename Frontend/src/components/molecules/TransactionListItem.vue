<script setup lang="ts">
import { computed } from "vue";
import { TransactionType, TaxInclusionTypeLabels } from "../../types/transaction";
import type { TransactionSummary } from "../../types/transaction";
import BaseCard from "../atoms/BaseCard.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseBadge from "../atoms/BaseBadge.vue";
import BaseIcon from "../atoms/BaseIcon.vue";

interface Props {
  transaction: TransactionSummary;
  variant?: "card" | "compact";
}

const props = withDefaults(defineProps<Props>(), {
  variant: "card",
});

const emit = defineEmits<{
  click: [id: string];
}>();

const displayName = computed(() => {
  if (props.transaction.type === TransactionType.Income) {
    return props.transaction.payer || "不明";
  }
  return props.transaction.payee || "不明";
});

const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  return date.toLocaleDateString("ja-JP", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
};

const formatAmount = (amount: number, type: string) => {
  const sign = type === TransactionType.Income ? "+" : "-";
  return `${sign}${amount.toLocaleString()}円`;
};

const handleClick = () => {
  emit("click", props.transaction.id);
};
</script>

<template>
  <button
    v-if="variant === 'compact'"
    @click="handleClick"
    class="w-full px-3 py-2.5 flex items-center gap-2.5 hover:bg-gray-50 active:bg-gray-100 transition-colors text-left border-b border-gray-200 last:border-b-0"
  >
    <div
      class="w-9 h-9 rounded-full flex items-center justify-center flex-shrink-0"
      :class="
        transaction.type === TransactionType.Income
          ? 'bg-green-100'
          : 'bg-red-100'
      "
    >
      <BaseIcon
        :name="transaction.type === TransactionType.Income ? 'trending-up' : 'trending-down'"
        size="sm"
        :class="
          transaction.type === TransactionType.Income
            ? 'text-green-600'
            : 'text-red-600'
        "
      />
    </div>

    <div class="flex-1 min-w-0">
      <div class="flex items-center gap-1.5 mb-0.5">
        <span class="text-sm font-semibold truncate">
          {{ displayName }}
        </span>
        <span
          v-if="transaction.userTransactionCategory"
          class="text-[10px] px-1.5 py-0.5 rounded-full flex-shrink-0 font-medium"
          :style="{
            backgroundColor: transaction.userTransactionCategory.colorCode + '20',
            color: transaction.userTransactionCategory.colorCode,
          }"
        >
          {{ transaction.userTransactionCategory.name }}
        </span>
      </div>
      <div class="text-xs text-gray-500">
        {{ formatDate(transaction.transactionDate) }}
      </div>
    </div>

    <div class="text-right flex-shrink-0">
      <div
        class="text-sm font-bold whitespace-nowrap"
        :class="
          transaction.type === TransactionType.Income
            ? 'text-green-600'
            : 'text-red-600'
        "
      >
        {{ formatAmount(transaction.amountTotal, transaction.type) }}
      </div>
    </div>
  </button>

  <BaseCard
    v-else
    clickable
    @click="handleClick"
    class="hover:shadow-lg transition-shadow"
  >
    <div class="flex items-center justify-between gap-4">
      <div class="flex-1 min-w-0">
        <div class="flex items-center gap-2 mb-2 flex-wrap">
          <BaseBadge
            :color="transaction.type === TransactionType.Income ? 'success' : 'danger'"
            size="sm"
          >
            {{ transaction.type === TransactionType.Income ? "収入" : "支出" }}
          </BaseBadge>
          <BaseBadge
            v-if="transaction.userTransactionCategory"
            :custom-color="transaction.userTransactionCategory.colorCode"
            size="sm"
          >
            {{ transaction.userTransactionCategory.name }}
          </BaseBadge>
          <BaseBadge
            v-if="transaction.taxInclusionType"
            color="info"
            size="sm"
          >
            {{ TaxInclusionTypeLabels[transaction.taxInclusionType] }}
          </BaseBadge>
        </div>
        <BaseText variant="body" weight="bold" class="mb-1">
          {{ displayName }}
        </BaseText>
        <BaseText variant="caption" color="gray">
          {{ formatDate(transaction.transactionDate) }}
        </BaseText>
      </div>

      <div class="text-right flex-shrink-0">
        <BaseText
          variant="h3"
          :color="transaction.type === TransactionType.Income ? 'success' : 'danger'"
          weight="bold"
        >
          {{ formatAmount(transaction.amountTotal, transaction.type) }}
        </BaseText>
      </div>
    </div>
  </BaseCard>
</template>
