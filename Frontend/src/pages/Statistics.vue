<script setup lang="ts">
import { ref } from "vue";
import DefaultLayout from "../layouts/DefaultLayout.vue";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseIcon from "../components/atoms/BaseIcon.vue";
import BaseBadge from "../components/atoms/BaseBadge.vue";

// モックデータ（将来的にAPIから取得）
const currentMonth = ref("2026年1月");
const totalIncome = ref(361035);
const totalExpense = ref(125678);
const balance = ref(235357);
const previousMonthIncome = ref(350000);
const previousMonthExpense = ref(130000);

// 前月比計算
const incomeChange = ref(
  Math.round(
    ((totalIncome.value - previousMonthIncome.value) /
      previousMonthIncome.value) *
      100,
  ),
);
const expenseChange = ref(
  Math.round(
    ((totalExpense.value - previousMonthExpense.value) /
      previousMonthExpense.value) *
      100,
  ),
);

// カテゴリ別支出（モックデータ）
const categoryExpenses = ref([
  { category: "食費", amount: 45000, percentage: 36 },
  { category: "外食", amount: 28000, percentage: 22 },
  { category: "日用品", amount: 18000, percentage: 14 },
  { category: "交通費", amount: 15000, percentage: 12 },
  { category: "その他", amount: 19678, percentage: 16 },
]);

const formatCurrency = (amount: number) => {
  return `¥${amount.toLocaleString()}`;
};

const getChangeColor = (change: number) => {
  if (change > 0) return "success";
  if (change < 0) return "danger";
  return "gray";
};

const getChangeIcon = (change: number) => {
  if (change > 0) return "chevron-up";
  if (change < 0) return "chevron-down";
  return "minus";
};
</script>

<template>
  <DefaultLayout>
    <div class="max-w-7xl mx-auto space-y-6">
      <!-- ヘッダー -->
      <div class="mb-6">
        <BaseText variant="h1" class="mb-2">統計</BaseText>
        <BaseText variant="caption" color="gray">
          {{ currentMonth }}の収支状況
        </BaseText>
      </div>

      <!-- 月次サマリーカード -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- 収入 -->
        <BaseCard>
          <div class="space-y-2">
            <div class="flex items-center gap-2">
              <BaseIcon name="arrow-down" size="sm" class="text-green-500" />
              <BaseText variant="caption" color="gray">収入</BaseText>
            </div>
            <BaseText variant="h2" color="success">
              {{ formatCurrency(totalIncome) }}
            </BaseText>
            <div class="flex items-center gap-1">
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
              {{ formatCurrency(totalExpense) }}
            </BaseText>
            <div class="flex items-center gap-1">
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

      <!-- カテゴリ別支出 -->
      <BaseCard>
        <div class="space-y-4">
          <div class="flex items-center gap-2 mb-4">
            <BaseIcon name="tag" size="md" class="text-gray-500" />
            <BaseText variant="h3">カテゴリ別支出</BaseText>
          </div>

          <!-- 円グラフプレースホルダー -->
          <div
            class="bg-gray-100 rounded-lg flex items-center justify-center"
            style="height: 300px"
          >
            <div class="text-center">
              <BaseIcon name="chart" size="xl" class="text-gray-400 mb-2" />
              <BaseText variant="body" color="gray">
                グラフ表示（準備中）
              </BaseText>
            </div>
          </div>

          <!-- カテゴリリスト -->
          <div class="space-y-2 mt-4">
            <div
              v-for="item in categoryExpenses"
              :key="item.category"
              class="flex items-center justify-between py-2 border-b border-gray-100 last:border-b-0"
            >
              <div class="flex items-center gap-3 flex-1">
                <div
                  class="w-2 h-2 rounded-full bg-blue-500"
                  :style="{
                    backgroundColor: `hsl(${item.percentage * 3.6}, 70%, 50%)`,
                  }"
                ></div>
                <BaseText variant="body">{{ item.category }}</BaseText>
                <BaseBadge color="gray" size="sm">
                  {{ item.percentage }}%
                </BaseBadge>
              </div>
              <BaseText variant="body" weight="bold">
                {{ formatCurrency(item.amount) }}
              </BaseText>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- 月次推移 -->
      <BaseCard>
        <div class="space-y-4">
          <div class="flex items-center gap-2 mb-4">
            <BaseIcon name="chart" size="md" class="text-gray-500" />
            <BaseText variant="h3">月次推移</BaseText>
          </div>

          <!-- 棒グラフプレースホルダー -->
          <div
            class="bg-gray-100 rounded-lg flex items-center justify-center"
            style="height: 300px"
          >
            <div class="text-center">
              <BaseIcon name="chart" size="xl" class="text-gray-400 mb-2" />
              <BaseText variant="body" color="gray">
                グラフ表示（準備中）
              </BaseText>
              <BaseText variant="caption" color="gray" class="mt-1 block">
                収入 vs 支出の月次推移
              </BaseText>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- 今月のハイライト -->
      <BaseCard>
        <div class="space-y-4">
          <div class="flex items-center gap-2 mb-4">
            <BaseIcon name="info" size="md" class="text-gray-500" />
            <BaseText variant="h3">今月のハイライト</BaseText>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <!-- 最高額の支出 -->
            <div class="p-4 bg-red-50 rounded-lg border border-red-100">
              <BaseText variant="caption" color="gray" class="mb-2">
                最高額の支出
              </BaseText>
              <BaseText variant="body" weight="bold" class="mb-1">
                {{ formatCurrency(3200) }}
              </BaseText>
              <BaseText variant="caption" color="gray">
                カフェチェーンB
              </BaseText>
            </div>

            <!-- 支出回数が多いカテゴリ -->
            <div class="p-4 bg-blue-50 rounded-lg border border-blue-100">
              <BaseText variant="caption" color="gray" class="mb-2">
                支出回数が多いカテゴリ
              </BaseText>
              <BaseText variant="body" weight="bold" class="mb-1">
                食費
              </BaseText>
              <BaseText variant="caption" color="gray"> 15回 </BaseText>
            </div>
          </div>
        </div>
      </BaseCard>
    </div>
  </DefaultLayout>
</template>
