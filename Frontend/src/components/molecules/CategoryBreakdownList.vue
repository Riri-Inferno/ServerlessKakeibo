<script setup lang="ts">
import BaseText from "../atoms/BaseText.vue";
import BaseBadge from "../atoms/BaseBadge.vue";
import type { CategorySummary } from "../../types/statistics";

interface Props {
  categories: CategorySummary[];
  totalExpense: number;
}

const props = defineProps<Props>();

const formatCurrency = (amount: number) => {
  return `¥${amount.toLocaleString()}`;
};

const calculatePercentage = (amount: number) => {
  if (props.totalExpense === 0) return 0;
  return Math.round((amount / props.totalExpense) * 100);
};

const getCategoryColor = (index: number) => {
  const hue = index * 137.5; // Golden angle for color distribution
  return `hsl(${hue % 360}, 70%, 50%)`;
};
</script>

<template>
  <div class="space-y-2">
    <div
      v-for="(item, index) in categories"
      :key="item.category"
      class="flex items-center justify-between py-2 border-b border-gray-100 last:border-b-0"
    >
      <div class="flex items-center gap-3 flex-1">
        <div
          class="w-3 h-3 rounded-full"
          :style="{ backgroundColor: getCategoryColor(index) }"
        ></div>
        <BaseText variant="body">{{ item.categoryName }}</BaseText>
        <BaseBadge color="gray" size="sm">
          {{ calculatePercentage(item.amount) }}%
        </BaseBadge>
      </div>
      <BaseText variant="body" weight="bold">
        {{ formatCurrency(item.amount) }}
      </BaseText>
    </div>
  </div>
</template>
