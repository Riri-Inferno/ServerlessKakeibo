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
import BaseSelect from "../components/atoms/BaseSelect.vue";

const {
  monthlyComparison,
  categoryBreakdown,
  monthlyTrend,
  highlights,
  isLoading,
  isPreviousMonthDisabled,
  errorMessage,
  currentYear,
  currentMonth,
  currentMonthLabel,
  isCurrentMonth,
  yearOptions,
  monthOptions,
  fetchCurrentMonth,
  goToPreviousMonth,
  goToNextMonth,
  goToCurrentMonth,
  handleYearChange,
  handleMonthChange,
  currentPeriodLabel,
} = useStatistics();

onMounted(async () => {
  await fetchCurrentMonth();
});
</script>

<template>
  <DefaultLayout>
    <div class="max-w-7xl mx-auto space-y-4 md:space-y-6">
      <div class="mb-4 md:mb-6">
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 md:gap-4 mb-3 md:mb-4">
          
          <div>
            <BaseText variant="h1" class="text-xl md:text-2xl lg:text-3xl">統計</BaseText>
            <BaseText variant="caption" color="gray" class="text-xs md:text-sm">
              月次統計情報
            </BaseText>
          </div>

          <div
            class="flex flex-col sm:flex-row items-stretch sm:items-center gap-2 md:gap-3"
          >
            <div class="flex items-center gap-2 order-2 sm:order-1">
              <BaseSelect
                :model-value="currentYear"
                :options="yearOptions"
                @update:model-value="(value) => handleYearChange(Number(value))"
                :disabled="isLoading"
                size="sm"
                hide-placeholder
                class="!w-28"
              />
              <BaseSelect
                :model-value="currentMonth"
                :options="monthOptions"
                @update:model-value="
                  (value) => handleMonthChange(Number(value))
                "
                :disabled="isLoading"
                size="sm"
                hide-placeholder
                class="w-24"
              />
            </div>

            <div
              class="flex items-center justify-center gap-2 order-1 sm:order-2"
            >
              <BaseButton
                variant="outline"
                size="sm"
                @click="goToPreviousMonth"
                :disabled="isLoading || isPreviousMonthDisabled"
                aria-label="前月"
              >
                <BaseIcon name="chevron-left" size="sm" />
              </BaseButton>

              <div class="min-w-[120px] text-center px-2 sm:hidden">
                <BaseText variant="body" weight="medium" class="text-sm">
                  {{ currentPeriodLabel }}
                </BaseText>
              </div>

              <BaseButton
                variant="outline"
                size="sm"
                @click="goToNextMonth"
                :disabled="isLoading || isCurrentMonth"
                aria-label="翌月"
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
        </div>

        <BaseText variant="caption" color="gray" class="text-xs md:text-sm">
          {{ currentPeriodLabel }}
        </BaseText>
      </div>

      <div
        v-if="isLoading"
        class="flex-1 flex items-center justify-center py-8 md:py-12"
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

      <div v-else-if="errorMessage" class="text-center py-8 md:py-12">
        <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
      </div>

      <template v-else>
        <StatsSummaryCards
          v-if="monthlyComparison"
          :income="monthlyComparison.current.income"
          :expense="monthlyComparison.current.expense"
          :balance="monthlyComparison.current.balance"
          :income-change="monthlyComparison.incomeChangePercent"
          :expense-change="monthlyComparison.expenseChangePercent"
        />

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
              <BaseText variant="h3">カテゴリ別支出</BaseText>
            </div>

            <CategoryPieChart
              :key="`chart-${currentYear}-${currentMonth}`"
              :categories="categoryBreakdown.categories"
            />

            <CategoryBreakdownList
              :categories="categoryBreakdown.categories"
              :total-expense="categoryBreakdown.totalExpense"
            />
          </div>
        </BaseCard>

        <BaseCard v-if="monthlyTrend" class="p-4 md:p-6">
          <div class="space-y-3 md:space-y-4">
            <div class="flex items-center gap-2 mb-3 md:mb-4">
              <BaseIcon name="chart" size="md" class="text-gray-500" />
              <BaseText variant="h3">月次推移</BaseText>
            </div>

            <MonthlyTrendChart :trend="monthlyTrend" />
          </div>
        </BaseCard>

        <BaseCard v-if="highlights" class="p-4 md:p-6">
          <div class="space-y-3 md:space-y-4">
            <div class="flex items-center gap-2 mb-3 md:mb-4">
              <BaseIcon name="info" size="md" class="text-gray-500" />
              <BaseText variant="h3"
                >{{ currentMonthLabel }}のハイライト</BaseText
              >
            </div>

            <HighlightsCards :highlights="highlights" />
          </div>
        </BaseCard>
      </template>
    </div>
  </DefaultLayout>
</template>
