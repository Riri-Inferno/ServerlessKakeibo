<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useAuthStore } from "../stores/authStore";
import apiClient from "../api/axios";
import DefaultLayout from "../layouts/DefaultLayout.vue";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";

interface MonthlySummary {
  year: number;
  month: number;
  income: number;
  expense: number;
  balance: number;
  transactionCount: number;
  incomeCount: number;
  expenseCount: number;
  topExpenseCategories: Array<{
    category: string;
    categoryName: string;
    amount: number;
    count: number;
  }>;
}

const authStore = useAuthStore();
const summary = ref<MonthlySummary | null>(null);
const isLoading = ref(true);
const errorMessage = ref("");

const fetchMonthlySummary = async () => {
  try {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1;

    const response = await apiClient.get(
      `/TransactionSummary/monthly?Year=${year}&Month=${month}`
    );

    if (response.data.status === "Success") {
      summary.value = response.data.data;
    }
  } catch (error) {
    console.error("サマリー取得エラー:", error);
    errorMessage.value = "データの取得に失敗しました";
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchMonthlySummary();
});
</script>

<template>
  <DefaultLayout>
    <div class="max-w-7xl mx-auto space-y-6">
      <div>
        <BaseText variant="h1" class="mb-2">ダッシュボード</BaseText>
        <BaseText variant="body" color="gray">
          ようこそ、{{ authStore.user?.displayName }}さん
        </BaseText>
      </div>

      <div v-if="isLoading" class="text-center py-12">
        <BaseText variant="body" color="gray">読み込み中...</BaseText>
      </div>

      <div v-else-if="errorMessage" class="text-center py-12">
        <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
      </div>

      <div
        v-else-if="summary"
        class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 md:gap-6"
      >
        <BaseCard>
          <BaseText variant="caption" color="gray" class="mb-2"
            >今月の収入</BaseText
          >
          <BaseText variant="amount" color="success">
            +{{ summary.income.toLocaleString() }}円
          </BaseText>
          <BaseText variant="caption" color="gray" class="mt-2">
            {{ summary.incomeCount }}件
          </BaseText>
        </BaseCard>

        <BaseCard>
          <BaseText variant="caption" color="gray" class="mb-2"
            >今月の支出</BaseText
          >
          <BaseText variant="amount" color="danger">
            -{{ summary.expense.toLocaleString() }}円
          </BaseText>
          <BaseText variant="caption" color="gray" class="mt-2">
            {{ summary.expenseCount }}件
          </BaseText>
        </BaseCard>

        <BaseCard>
          <BaseText variant="caption" color="gray" class="mb-2">残高</BaseText>
          <BaseText variant="amount">
            {{ summary.balance.toLocaleString() }}円
          </BaseText>
          <BaseText variant="caption" color="gray" class="mt-2">
            合計{{ summary.transactionCount }}件
          </BaseText>
        </BaseCard>
      </div>

      <BaseCard v-if="summary && summary.topExpenseCategories.length > 0">
        <BaseText variant="h3" class="mb-4">支出トップ3</BaseText>
        <div class="space-y-3">
          <div
            v-for="category in summary.topExpenseCategories"
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
