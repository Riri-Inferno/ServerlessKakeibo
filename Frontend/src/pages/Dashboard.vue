<script setup lang="ts">
import { onMounted } from "vue";
import { useAuth } from "../composables/useAuth";
import { useStatistics } from "../composables/useStatistics";
import DefaultLayout from "../layouts/DefaultLayout.vue";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseSpinner from "../components/atoms/BaseSpinner.vue";

const { effectiveDisplayName } = useAuth();

const {
  monthlySummary,
  isLoading,
  errorMessage,
  fetchMonthlySummary,
  getCurrentYearMonth,
} = useStatistics();

onMounted(async () => {
  const { year, month } = getCurrentYearMonth();
  await fetchMonthlySummary(year, month);
});
</script>

<template>
  <DefaultLayout>
    <div class="max-w-7xl mx-auto space-y-6">
      <div>
        <BaseText variant="h1" class="mb-2">ダッシュボード</BaseText>
        <BaseText variant="body" color="gray">
          ようこそ、{{ effectiveDisplayName }}さん
        </BaseText>
      </div>

      <div v-if="isLoading" class="text-center py-12">
        <BaseSpinner
          icon="refresh"
          size="lg"
          color="primary"
          label="読み込み中"
          class="mb-2"
        />
        <BaseText variant="body" color="gray">読み込み中...</BaseText>
      </div>

      <div v-else-if="errorMessage" class="text-center py-12">
        <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
      </div>

      <div
        v-else-if="monthlySummary"
        class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 md:gap-6"
      >
        <BaseCard>
          <BaseText variant="caption" color="gray" class="mb-2"
            >今月の収入</BaseText
          >
          <BaseText variant="amount" color="success">
            +{{ monthlySummary.income.toLocaleString() }}円
          </BaseText>
          <BaseText variant="caption" color="gray" class="mt-2">
            {{ monthlySummary.incomeCount }}件
          </BaseText>
        </BaseCard>

        <BaseCard>
          <BaseText variant="caption" color="gray" class="mb-2"
            >今月の支出</BaseText
          >
          <BaseText variant="amount" color="danger">
            -{{ monthlySummary.expense.toLocaleString() }}円
          </BaseText>
          <BaseText variant="caption" color="gray" class="mt-2">
            {{ monthlySummary.expenseCount }}件
          </BaseText>
        </BaseCard>

        <BaseCard>
          <BaseText variant="caption" color="gray" class="mb-2">残高</BaseText>
          <BaseText variant="amount">
            {{ monthlySummary.balance.toLocaleString() }}円
          </BaseText>
          <BaseText variant="caption" color="gray" class="mt-2">
            合計{{ monthlySummary.transactionCount }}件
          </BaseText>
        </BaseCard>
      </div>

      <BaseCard
        v-if="monthlySummary && monthlySummary.topExpenseCategories.length > 0"
      >
        <BaseText variant="h3" class="mb-4">支出トップ3</BaseText>
        <div class="space-y-3">
          <div
            v-for="category in monthlySummary.topExpenseCategories"
            :key="category.category"
            class="flex justify-between items-center p-3 bg-gray-50 rounded-lg"
          >
            <div>
              <BaseText variant="body" weight="medium">{{
                category.categoryName
              }}</BaseText>
              <BaseText variant="caption" color="gray"
                >{{ category.count }}件</BaseText
              >
            </div>
            <BaseText variant="body" color="danger" weight="bold">
              {{ category.amount.toLocaleString() }}円
            </BaseText>
          </div>
        </div>
      </BaseCard>
    </div>
  </DefaultLayout>
</template>
