<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useTransactions } from "../composables/useTransactions";
import { TransactionType, CategoryLabels } from "../types/transaction";
import type { GetTransactionsRequest } from "../types/transaction";
import DefaultLayout from "../layouts/DefaultLayout.vue";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseButton from "../components/atoms/BaseButton.vue";
import BaseBadge from "../components/atoms/BaseBadge.vue";
import TransactionFilter from "../components/molecules/TransactionFilter.vue";
import TransactionCreateSelectModal from "../components/organisms/TransactionCreateSelectModal.vue";
import TransactionFormModal from "../components/organisms/TransactionFormModal.vue";
import TransactionDetailModal from "../components/organisms/TransactionDetailModal.vue";

const {
  transactions,
  totalCount,
  currentPage,
  totalPages,
  isLoading,
  errorMessage,
  fetchTransactions,
  nextPage,
  prevPage,
} = useTransactions();

const selectedTransactionId = ref<string | null>(null);
const isDetailModalOpen = ref(false);
const isCreateSelectModalOpen = ref(false);
const isFormModalOpen = ref(false);
const formMode = ref<"manual" | "receipt">("manual");
const currentFilters = ref<GetTransactionsRequest>({});

const openDetail = (id: string) => {
  selectedTransactionId.value = id;
  isDetailModalOpen.value = true;
};

const closeDetailModal = () => {
  isDetailModalOpen.value = false;
  selectedTransactionId.value = null;
};

const openCreateModal = () => {
  isCreateSelectModalOpen.value = true;
};

const closeCreateSelectModal = () => {
  isCreateSelectModalOpen.value = false;
};

const selectManual = () => {
  formMode.value = "manual";
  isCreateSelectModalOpen.value = false;
  isFormModalOpen.value = true;
};

const selectReceipt = () => {
  formMode.value = "receipt";
  isCreateSelectModalOpen.value = false;
  isFormModalOpen.value = true;
};

const closeFormModal = () => {
  isFormModalOpen.value = false;
};

const handleCreateSuccess = () => {
  fetchTransactions(currentFilters.value);
};

const handleSearch = (filters: GetTransactionsRequest) => {
  currentFilters.value = filters;
  fetchTransactions(filters);
};

const handleClearFilters = () => {
  currentFilters.value = {};
  fetchTransactions();
};

const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  return date.toLocaleDateString("ja-JP", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
};

const formatAmount = (amount: number, type: string) => {
  const sign = type === TransactionType.Income ? "+" : "-";
  return `${sign}${amount.toLocaleString()}円`;
};

onMounted(() => {
  fetchTransactions();
});
</script>

<template>
  <DefaultLayout>
    <div class="max-w-7xl mx-auto space-y-6">
      <div class="flex items-center justify-between">
        <div>
          <BaseText variant="h1" class="mb-2">取引一覧</BaseText>
          <BaseText variant="caption" color="gray">
            全{{ totalCount }}件
          </BaseText>
        </div>
        <BaseButton variant="primary" @click="openCreateModal">
          新規登録
        </BaseButton>
      </div>

      <TransactionFilter @search="handleSearch" @clear="handleClearFilters" />

      <div v-if="isLoading" class="text-center py-12">
        <BaseText variant="body" color="gray">読み込み中...</BaseText>
      </div>

      <div v-else-if="errorMessage" class="text-center py-12">
        <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
      </div>

      <div v-else-if="transactions.length === 0" class="text-center py-12">
        <BaseText variant="body" color="gray">取引がありません</BaseText>
      </div>

      <div v-else class="space-y-4">
        <BaseCard
          v-for="transaction in transactions"
          :key="transaction.id"
          clickable
          @click="openDetail(transaction.id)"
          class="hover:shadow-lg transition-shadow"
        >
          <div class="flex items-center justify-between gap-4">
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-2">
                <BaseBadge
                  :color="
                    transaction.type === TransactionType.Income
                      ? 'success'
                      : 'danger'
                  "
                  size="sm"
                >
                  {{
                    transaction.type === TransactionType.Income
                      ? "収入"
                      : "支出"
                  }}
                </BaseBadge>
                <BaseBadge color="gray" size="sm">
                  {{
                    CategoryLabels[transaction.category] || transaction.category
                  }}
                </BaseBadge>
              </div>
              <BaseText variant="body" weight="bold" class="mb-1">
                {{ transaction.payee }}
              </BaseText>
              <BaseText variant="caption" color="gray">
                {{ formatDate(transaction.transactionDate) }}
              </BaseText>
            </div>

            <div class="text-right flex-shrink-0">
              <BaseText
                variant="h3"
                :color="
                  transaction.type === TransactionType.Income
                    ? 'success'
                    : 'danger'
                "
                weight="bold"
              >
                {{ formatAmount(transaction.amountTotal, transaction.type) }}
              </BaseText>
            </div>
          </div>
        </BaseCard>

        <div class="flex items-center justify-between pt-4">
          <BaseButton
            variant="outline"
            :disabled="currentPage === 1"
            @click="prevPage"
          >
            前へ
          </BaseButton>

          <BaseText variant="body" color="gray">
            {{ currentPage }} / {{ totalPages }}
          </BaseText>

          <BaseButton
            variant="outline"
            :disabled="currentPage === totalPages"
            @click="nextPage"
          >
            次へ
          </BaseButton>
        </div>
      </div>
    </div>

    <TransactionCreateSelectModal
      :is-open="isCreateSelectModalOpen"
      @close="closeCreateSelectModal"
      @select-manual="selectManual"
      @select-receipt="selectReceipt"
    />

    <TransactionFormModal
      :is-open="isFormModalOpen"
      :mode="formMode"
      @close="closeFormModal"
      @success="handleCreateSuccess"
    />

    <TransactionDetailModal
      v-if="selectedTransactionId"
      :transaction-id="selectedTransactionId"
      :is-open="isDetailModalOpen"
      @close="closeDetailModal"
    />
  </DefaultLayout>
</template>
