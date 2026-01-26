<script setup lang="ts">
import BaseText from "../atoms/BaseText.vue";
import type { HighlightsResult } from "../../types/statistics";

interface Props {
  highlights: HighlightsResult;
}

const props = defineProps<Props>();

const formatCurrency = (amount: number) => {
  return `¥${amount.toLocaleString()}`;
};
</script>

<template>
  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
    <!-- 最高額の支出 -->
    <div
      v-if="highlights.maxExpenseTransaction"
      class="p-4 bg-red-50 rounded-lg border border-red-100"
    >
      <BaseText variant="caption" color="gray" class="mb-2">
        最高額の支出
      </BaseText>
      <BaseText variant="body" weight="bold" class="mb-1">
        {{ formatCurrency(highlights.maxExpenseTransaction.amount) }}
      </BaseText>
      <BaseText variant="caption" color="gray">
        {{ highlights.maxExpenseTransaction.payee }}
      </BaseText>
    </div>

    <!-- 支出回数が多いカテゴリ -->
    <div
      v-if="highlights.mostFrequentCategory"
      class="p-4 bg-blue-50 rounded-lg border border-blue-100"
    >
      <BaseText variant="caption" color="gray" class="mb-2">
        支出回数が多いカテゴリ
      </BaseText>
      <BaseText variant="body" weight="bold" class="mb-1">
        {{ highlights.mostFrequentCategory.categoryName }}
      </BaseText>
      <BaseText variant="caption" color="gray">
        {{ highlights.mostFrequentCategory.count }}回
      </BaseText>
    </div>

    <!-- 1日平均支出 -->
    <div class="p-4 bg-green-50 rounded-lg border border-green-100">
      <BaseText variant="caption" color="gray" class="mb-2">
        1日平均支出
      </BaseText>
      <BaseText variant="body" weight="bold">
        {{ formatCurrency(Math.round(highlights.averageExpensePerDay)) }}
      </BaseText>
    </div>

    <!-- 支出日数 -->
    <div class="p-4 bg-purple-50 rounded-lg border border-purple-100">
      <BaseText variant="caption" color="gray" class="mb-2">
        支出があった日数
      </BaseText>
      <BaseText variant="body" weight="bold">
        {{ highlights.daysWithExpense }}日
      </BaseText>
    </div>
  </div>
</template>
