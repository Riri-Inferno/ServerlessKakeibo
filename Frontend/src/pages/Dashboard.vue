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
                {{ categoryBreakdown?.totalExpense?.toLocaleString() }}円
              </BaseText>
            </div>
          </div>
        </BaseCard>

        <!-- 支出トップ3 -->
        <BaseCard v-if="monthlySummary?.topExpenseCategories?.length > 0">
          <div class="flex items-center gap-2 mb-4">
            <BaseIcon name="chart" size="md" class="text-gray-500" />
            <BaseText variant="h3">支出トップ3</BaseText>
          </div>
          <div class="space-y-3">
            <div
              v-for="(category, index) in monthlySummary.topExpenseCategories"
              :key="category.categoryId"
              class="relative overflow-hidden rounded-xl border border-gray-200 hover:shadow-md transition-shadow"
            >
              <!-- カテゴリカラーのアクセントバー -->
              <div
                class="absolute left-0 top-0 bottom-0 w-1.5"
                :style="{ backgroundColor: category.colorCode }"
              ></div>

              <!-- コンテンツ -->
              <div class="flex items-center justify-between p-4 pl-5">
                <div class="flex items-center gap-4">
                  <!-- ランキングアイコン -->
                  <div class="relative">
                    <div
                      class="w-10 h-10 rounded-full flex items-center justify-center font-bold text-lg"
                      :style="{
                        backgroundColor: category.colorCode + '20',
                        color: category.colorCode,
                      }"
                    >
                      {{ index + 1 }}
                    </div>
                    <!-- トロフィーアイコン（1位のみ） -->
                    <div
                      v-if="index === 0"
                      class="absolute -top-1 -right-1 w-5 h-5 bg-yellow-400 rounded-full flex items-center justify-center"
                    >
                      <BaseIcon name="star" size="sm" class="text-white" />
                    </div>
                  </div>

                  <!-- カテゴリ情報 -->
                  <div>
                    <BaseText variant="body" weight="bold" class="mb-0.5">
                      {{ category.categoryName }}
                    </BaseText>
                    <div class="flex items-center gap-2">
                      <BaseText variant="caption" color="gray">
                        {{ category.count }}件の取引
                      </BaseText>
                    </div>
                  </div>
                </div>

                <!-- 金額 -->
                <div class="text-right">
                  <BaseText variant="h3" color="danger">
                    {{ category.amount.toLocaleString() }}
                  </BaseText>
                  <BaseText variant="caption" color="gray">円</BaseText>
                </div>
              </div>

              <!-- 金額バー（視覚化） -->
              <div class="h-1 bg-gray-100">
                <div
                  class="h-full transition-all duration-500"
                  :style="{
                    backgroundColor: category.colorCode,
                    width: `${(category.amount / (monthlySummary?.topExpenseCategories?.[0]?.amount || 1)) * 100}%`,
                  }"
                ></div>
              </div>
            </div>
          </div>
        </BaseCard>
      </template>
    </div>
  </DefaultLayout>
</template>
