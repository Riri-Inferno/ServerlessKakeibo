<script setup lang="ts">
import BaseCard from "../atoms/BaseCard.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseIcon from "../atoms/BaseIcon.vue";

interface Props {
  income: number;
  expense: number;
  balance: number;
  incomeChange: number | null;
  expenseChange: number | null;
}

const props = defineProps<Props>();

const formatCurrency = (amount: number) => {
  return `¥${amount.toLocaleString()}`;
};

const getChangeColor = (change: number | null) => {
  if (change === null) return "gray";
  if (change > 0) return "success";
  if (change < 0) return "danger";
  return "gray";
};

const getChangeIcon = (change: number | null) => {
  if (change === null) return "minus";
  if (change > 0) return "chevron-up";
  if (change < 0) return "chevron-down";
  return "minus";
};
</script>

<template>
  <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
    <!-- 収入 -->
    <BaseCard>
      <div class="space-y-2">
        <div class="flex items-center gap-2">
          <BaseIcon name="arrow-down" size="sm" class="text-green-500" />
          <BaseText variant="caption" color="gray">収入</BaseText>
        </div>
        <BaseText variant="h2" color="success">
          {{ formatCurrency(income) }}
        </BaseText>
        <div v-if="incomeChange !== null" class="flex items-center gap-1">
          <BaseIcon
            :name="getChangeIcon(incomeChange)"
            size="sm"
            :class="incomeChange > 0 ? 'text-green-500' : 'text-red-500'"
          />
          <BaseText variant="caption" :color="getChangeColor(incomeChange)">
            前月比 {{ incomeChange > 0 ? "+" : "" }}{{ incomeChange }}%
          </BaseText>
        </div>
      </div>
    </BaseCard>

    <!-- 支出 -->
    <BaseCard>
      <div class="space-y-2">
        <div class="flex items-center gap-2">
          <BaseIcon name="arrow-up" size="sm" class="text-red-500" />
          <BaseText variant="caption" color="gray">支出</BaseText>
        </div>
        <BaseText variant="h2" color="danger">
          {{ formatCurrency(expense) }}
        </BaseText>
        <div v-if="expenseChange !== null" class="flex items-center gap-1">
          <BaseIcon
            :name="getChangeIcon(expenseChange)"
            size="sm"
            :class="expenseChange < 0 ? 'text-green-500' : 'text-red-500'"
          />
          <BaseText
            variant="caption"
            :color="expenseChange < 0 ? 'success' : 'danger'"
          >
            前月比 {{ expenseChange > 0 ? "+" : "" }}{{ expenseChange }}%
          </BaseText>
        </div>
      </div>
    </BaseCard>

    <!-- 収支 -->
    <BaseCard>
      <div class="space-y-2">
        <div class="flex items-center gap-2">
          <BaseIcon name="banknotes" size="sm" class="text-blue-500" />
          <BaseText variant="caption" color="gray">収支</BaseText>
        </div>
        <BaseText variant="h2" :color="balance > 0 ? 'success' : 'danger'">
          {{ formatCurrency(balance) }}
        </BaseText>
        <BaseText variant="caption" color="gray">
          {{ balance > 0 ? "黒字" : "赤字" }}
        </BaseText>
      </div>
    </BaseCard>
  </div>
</template>
