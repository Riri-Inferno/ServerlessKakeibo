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
    <!-- スマホ: space-y-4, タブレット・PC: space-y-6 -->
    <div class="max-w-7xl mx-auto space-y-4 md:space-y-6">
      <!-- ヘッダー -->
      <div>
        <BaseText variant="h1" class="mb-1 md:mb-2">ダッシュボード</BaseText>
        <BaseText variant="caption" color="gray" class="mt-0.5 md:mt-1">
          {{ currentPeriodLabel }}
        </BaseText>
      </div>

      <!-- ローディング -->
      <div v-if="isLoading" class="text-center py-8 md:py-12">
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
      <div v-else-if="errorMessage" class="text-center py-8 md:py-12">
        <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
      </div>

      <!-- データ表示 -->
      <template v-else-if="monthlySummary">
        <!-- サマリーカード: スマホは gap-3, タブレット・PCは元のまま（gap-6） -->
        <div
          class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3 md:gap-4 md:gap-6"
        >
          <!-- 収入カード: スマホは p-3, タブレット・PCは元のまま -->
          <BaseCard class="p-3 md:p-4 md:p-6">
            <BaseText variant="caption" color="gray" class="mb-1 md:mb-2"
              >今月の収入</BaseText
            >
            <BaseText variant="amount" color="success" class="text-xl md:text-3xl">
              +{{ monthlySummary.income.toLocaleString() }}円
            </BaseText>
            <BaseText variant="caption" color="gray" class="mt-1 md:mt-2">
              {{ monthlySummary.incomeCount }}件
            </BaseText>
          </BaseCard>

          <!-- 支出カード -->
          <BaseCard class="p-3 md:p-4 md:p-6">
            <BaseText variant="caption" color="gray" class="mb-1 md:mb-2"
              >今月の支出</BaseText
            >
            <BaseText variant="amount" color="danger" class="text-xl md:text-3xl">
              -{{ monthlySummary.expense.toLocaleString() }}円
            </BaseText>
            <BaseText variant="caption" color="gray" class="mt-1 md:mt-2">
              {{ monthlySummary.expenseCount }}件
            </BaseText>
          </BaseCard>

          <!-- 残高カード -->
          <BaseCard class="p-3 md:p-4 md:p-6">
            <BaseText variant="caption" color="gray" class="mb-1 md:mb-2"
              >残高</BaseText
            >
            <BaseText variant="amount" class="text-xl md:text-3xl">
              {{ monthlySummary.balance.toLocaleString() }}円
            </BaseText>
            <BaseText variant="caption" color="gray" class="mt-1 md:mt-2">
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
          class="p-4 md:p-6"
        >
          <div class="space-y-4 md:space-y-6">
            <div class="flex items-center gap-2">
              <BaseIcon name="tag" size="md" class="text-gray-500" />
              <BaseText variant="h3">今月のカテゴリ別支出</BaseText>
            </div>

            <!-- 円グラフ -->
            <CategoryPieChart :categories="categoryBreakdown.categories" />

            <!-- 合計表示 -->
            <div class="text-center pt-3 md:pt-4 border-t border-gray-200">
              <BaseText variant="caption" color="gray" class="mb-1">
                支出合計
              </BaseText>
              <BaseText variant="h2" color="danger" class="text-xl md:text-2xl">
                {{ categoryBreakdown?.totalExpense?.toLocaleString() }}円
              </BaseText>
            </div>
          </div>
        </BaseCard>

        <!-- 支出トップ3: スマホでコンパクトに -->
        <BaseCard v-if="monthlySummary?.topExpenseCategories?.length > 0" class="p-4 md:p-6">
          <div class="flex items-center gap-2 mb-3 md:mb-4">
            <BaseIcon name="chart" size="md" class="text-gray-500" />
            <BaseText variant="h3">支出トップ3</BaseText>
          </div>
          <div class="space-y-2 md:space-y-3">
            <div
              v-for="(category, index) in monthlySummary.topExpenseCategories"
              :key="category.categoryId"
              class="relative overflow-hidden rounded-lg md:rounded-xl border border-gray-200 hover:shadow-md transition-shadow"
            >
              <!-- カテゴリカラーのアクセントバー -->
              <div
                class="absolute left-0 top-0 bottom-0 w-1 md:w-1.5"
                :style="{ backgroundColor: category.colorCode }"
              ></div>

              <!-- コンテンツ: スマホでパディング削減 -->
              <div class="flex items-center justify-between p-3 md:p-4 pl-4 md:pl-5">
                <div class="flex items-center gap-2 md:gap-4">
                  <!-- ランキングアイコン: スマホで小さく -->
                  <div class="relative flex-shrink-0">
                    <div
                      class="w-8 h-8 md:w-10 md:h-10 rounded-full flex items-center justify-center font-bold text-base md:text-lg"
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
                      class="absolute -top-1 -right-1 w-4 h-4 md:w-5 md:h-5 bg-yellow-400 rounded-full flex items-center justify-center"
                    >
                      <BaseIcon name="star" size="sm" class="text-white scale-75 md:scale-100" />
                    </div>
                  </div>

                  <!-- カテゴリ情報 -->
                  <div class="min-w-0">
                    <BaseText variant="body" weight="bold" class="mb-0.5 text-sm md:text-base truncate">
                      {{ category.categoryName }}
                    </BaseText>
                    <div class="flex items-center gap-2">
                      <BaseText variant="caption" color="gray" class="text-xs md:text-sm">
                        {{ category.count }}件
                      </BaseText>
                    </div>
                  </div>
                </div>

                <!-- 金額: スマホで文字サイズ調整 -->
                <div class="text-right flex-shrink-0">
                  <BaseText variant="h3" color="danger" class="text-base md:text-xl whitespace-nowrap">
                    {{ category.amount.toLocaleString() }}
                  </BaseText>
                  <BaseText variant="caption" color="gray" class="text-xs">円</BaseText>
                </div>
              </div>

              <!-- 金額バー（視覚化） -->
              <div class="h-0.5 md:h-1 bg-gray-100">
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
