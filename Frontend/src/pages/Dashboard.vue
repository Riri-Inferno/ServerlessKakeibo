<script setup lang="ts">
import { onMounted } from "vue";
import { useStatistics } from "../composables/useStatistics";
import DefaultLayout from "../layouts/DefaultLayout.vue";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseIcon from "../components/atoms/BaseIcon.vue";
import BaseSpinner from "../components/atoms/BaseSpinner.vue";
import CategoryPieChart from "../components/organisms/CategoryPieChart.vue";

const {
  monthlySummary,
  categoryBreakdown,
  isLoading,
  errorMessage,
  fetchMonthlySummary,
  fetchCategoryBreakdown,
  getCurrentYearMonth,
  currentPeriodLabel,
} = useStatistics();

onMounted(async () => {
  const { year, month } = getCurrentYearMonth();
  await Promise.all([
    fetchMonthlySummary(year, month),
    fetchCategoryBreakdown(year, month),
  ]);
});
</script>

<template>
  <DefaultLayout>
    <div class="max-w-7xl mx-auto space-y-6">
      <!-- ヘッダー -->
      <div>
        <BaseText variant="h1" class="mb-2">ダッシュボード</BaseText>
        <BaseText variant="caption" color="gray" class="mt-1">
          {{ currentPeriodLabel }}
        </BaseText>
      </div>

      <!-- ローディング -->
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

      <!-- エラー -->
      <div v-else-if="errorMessage" class="text-center py-12">
        <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
      </div>

      <!-- データ表示 -->
      <template v-else-if="monthlySummary">
        <!-- サマリーカード -->
        <div
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
            <BaseText variant="caption" color="gray" class="mb-2"
              >残高</BaseText
            >
            <BaseText variant="amount">
              {{ monthlySummary.balance.toLocaleString() }}円
            </BaseText>
            <BaseText variant="caption" color="gray" class="mt-2">
              合計{{ monthlySummary.transactionCount }}件
            </BaseText>
          </BaseCard>
        </div>

        <!-- カテゴリ別支出（円グラフ） -->
        <BaseCard
          v-if="
            categoryBreakdown &&
            Array.isArray(categoryBreakdown.categories) &&
            categoryBreakdown.categories.length > 0
          "
        >
          <div class="space-y-6">
            <div class="flex items-center gap-2">
              <BaseIcon name="tag" size="md" class="text-gray-500" />
              <BaseText variant="h3">今月のカテゴリ別支出</BaseText>
            </div>

            <!-- 円グラフ -->
            <CategoryPieChart :categories="categoryBreakdown.categories" />

            <!-- 合計表示 -->
            <div class="text-center pt-4 border-t border-gray-200">
              <BaseText variant="caption" color="gray" class="mb-1">
                支出合計
              </BaseText>
              <BaseText variant="h2" color="danger">
                {{ categoryBreakdown.totalExpense.toLocaleString() }}円
              </BaseText>
            </div>
          </div>
        </BaseCard>

        <!-- 支出トップ3 -->
        <BaseCard v-if="monthlySummary.topExpenseCategories.length > 0">
          <div class="flex items-center gap-2 mb-4">
            <BaseIcon name="chart" size="md" class="text-gray-500" />
            <BaseText variant="h3">支出トップ3</BaseText>
          </div>
          <div class="space-y-3">
            <div
              v-for="(category, index) in monthlySummary.topExpenseCategories"
              :key="category.categoryId"
              class="flex justify-between items-center p-3 bg-gray-50 rounded-lg"
            >
              <!-- ランキングバッジ -->
              <div class="flex items-center gap-3">
                <div
                  class="w-8 h-8 rounded-full flex items-center justify-center font-bold text-white"
                  :class="{
                    'bg-yellow-500': index === 0,
                    'bg-gray-400': index === 1,
                    'bg-orange-600': index === 2,
                  }"
                >
                  {{ index + 1 }}
                </div>
                <div>
                  <BaseText variant="body" weight="medium">
                    {{ category.categoryName }}
                  </BaseText>
                  <BaseText variant="caption" color="gray">
                    {{ category.count }}件
                  </BaseText>
                </div>
              </div>
              <BaseText variant="body" color="danger" weight="bold">
                {{ category.amount.toLocaleString() }}円
              </BaseText>
            </div>
          </div>
        </BaseCard>
      </template>
    </div>
  </DefaultLayout>
</template>
