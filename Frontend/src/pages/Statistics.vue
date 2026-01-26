<script setup lang="ts">
import { onMounted } from "vue";
import { useStatistics } from "../composables/useStatistics";
import DefaultLayout from "../layouts/DefaultLayout.vue";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseIcon from "../components/atoms/BaseIcon.vue";
import BaseButton from "../components/atoms/BaseButton.vue";
import BaseSpinner from "../components/atoms/BaseSpinner.vue";
import StatsSummaryCards from "../components/molecules/StatsSummaryCards.vue";
import CategoryBreakdownList from "../components/molecules/CategoryBreakdownList.vue";
import HighlightsCards from "../components/molecules/HighlightsCards.vue";
import CategoryPieChart from "../components/organisms/CategoryPieChart.vue";
import MonthlyTrendChart from "../components/organisms/MonthlyTrendChart.vue";

const {
  monthlyComparison,
  categoryBreakdown,
  monthlyTrend,
  highlights,
  isLoading,
  errorMessage,
  currentMonthLabel,
  fetchCurrentMonth,
  goToPreviousMonth,
  goToNextMonth,
  goToCurrentMonth,
} = useStatistics();

onMounted(async () => {
  await fetchCurrentMonth();
});
</script>

<template>
  <DefaultLayout>
    <div class="max-w-7xl mx-auto space-y-6">
      <!-- ヘッダー + 月切り替え -->
      <div class="mb-6">
        <div
          class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-4"
        >
          <BaseText variant="h1">統計</BaseText>

          <!-- 月切り替えUI -->
          <div class="flex items-center justify-center sm:justify-end gap-2">
            <BaseButton
              variant="outline"
              size="sm"
              @click="goToPreviousMonth"
              :disabled="isLoading"
            >
              <BaseIcon name="chevron-left" size="sm" />
            </BaseButton>

            <div class="min-w-[140px] text-center px-4">
              <BaseText variant="h3">{{ currentMonthLabel }}</BaseText>
            </div>

            <BaseButton
              variant="outline"
              size="sm"
              @click="goToNextMonth"
              :disabled="isLoading"
            >
              <BaseIcon name="chevron-right" size="sm" />
            </BaseButton>

            <BaseButton
              variant="outline"
              size="sm"
              @click="goToCurrentMonth"
              :disabled="isLoading"
              class="ml-2"
            >
              今月
            </BaseButton>
          </div>
        </div>

        <BaseText variant="caption" color="gray">
          {{ currentMonthLabel }}の収支状況
        </BaseText>
      </div>

      <!-- ローディング -->
      <div
        v-if="isLoading"
        class="flex-1 flex items-center justify-center py-12"
      >
        <div class="text-center">
          <BaseSpinner
            icon="refresh"
            size="lg"
            color="primary"
            label="統計データを読み込み中"
            class="mb-2"
          />
          <BaseText variant="body" color="gray">読み込み中...</BaseText>
        </div>
      </div>

      <!-- エラー -->
      <div v-else-if="errorMessage" class="text-center py-12">
        <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
      </div>

      <!-- データ表示 -->
      <template v-else>
        <!-- 月次サマリーカード -->
        <StatsSummaryCards
          v-if="monthlyComparison"
          :income="monthlyComparison.current.income"
          :expense="monthlyComparison.current.expense"
          :balance="monthlyComparison.current.balance"
          :income-change="monthlyComparison.incomeChangePercent"
          :expense-change="monthlyComparison.expenseChangePercent"
        />

        <!-- カテゴリ別支出 -->
        <BaseCard
          v-if="categoryBreakdown && categoryBreakdown.categories.length > 0"
        >
          <div class="space-y-6">
            <div class="flex items-center gap-2">
              <BaseIcon name="tag" size="md" class="text-gray-500" />
              <BaseText variant="h3">カテゴリ別支出</BaseText>
            </div>

            <!-- 円グラフ -->
            <CategoryPieChart :categories="categoryBreakdown.categories" />

            <!-- カテゴリリスト -->
            <CategoryBreakdownList
              :categories="categoryBreakdown.categories"
              :total-expense="categoryBreakdown.totalExpense"
            />
          </div>
        </BaseCard>

        <!-- 月次推移 -->
        <BaseCard v-if="monthlyTrend">
          <div class="space-y-4">
            <div class="flex items-center gap-2 mb-4">
              <BaseIcon name="chart" size="md" class="text-gray-500" />
              <BaseText variant="h3">月次推移</BaseText>
            </div>

            <MonthlyTrendChart :trend="monthlyTrend" />
          </div>
        </BaseCard>

        <!-- 今月のハイライト -->
        <BaseCard v-if="highlights">
          <div class="space-y-4">
            <div class="flex items-center gap-2 mb-4">
              <BaseIcon name="info" size="md" class="text-gray-500" />
              <BaseText variant="h3">今月のハイライト</BaseText>
            </div>

            <HighlightsCards :highlights="highlights" />
          </div>
        </BaseCard>
      </template>
    </div>
  </DefaultLayout>
</template>
