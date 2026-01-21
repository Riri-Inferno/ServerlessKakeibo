<script setup lang="ts">
import { onMounted, onUnmounted, ref, nextTick } from "vue";
import { useTransactions } from "../composables/useTransactions";
import {
  TransactionType,
  CategoryLabels,
  TaxInclusionTypeLabels,
} from "../types/transaction";
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
  setContainerHeight,
  setItemHeight,
} = useTransactions();

const selectedTransactionId = ref<string | null>(null);
const isDetailModalOpen = ref(false);
const isCreateSelectModalOpen = ref(false);
const isFormModalOpen = ref(false);
const isEditModalOpen = ref(false);
const formMode = ref<"manual" | "receipt">("manual");
const currentFilters = ref<GetTransactionsRequest>({});

const listContainerRef = ref<HTMLElement>();
let resizeObserver: ResizeObserver | null = null;

const measureHeights = () => {
  if (!listContainerRef.value) return;

  const screenHeight = window.innerHeight;
  const containerTop = listContainerRef.value.getBoundingClientRect().top;

  const paginationElement = document.querySelector("[data-pagination]");
  const paginationHeight = paginationElement
    ? paginationElement.getBoundingClientRect().height + 16
    : 76;

  const isMobile = window.innerWidth < 768;
  const bottomNavHeight = isMobile ? 80 : 0;

  const BOTTOM_PADDING = 16;

  const availableHeight =
    screenHeight -
    containerTop -
    paginationHeight -
    bottomNavHeight -
    BOTTOM_PADDING;

  setContainerHeight(availableHeight);

  const firstCard = listContainerRef.value.querySelector(
    "[data-transaction-card]",
  );
  if (firstCard) {
    const itemRect = firstCard.getBoundingClientRect();
    const gap = 16;
    setItemHeight(itemRect.height + gap);
  }
};

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

const handleEdit = () => {
  isDetailModalOpen.value = false;
  isEditModalOpen.value = true;
};

const handleDelete = async () => {
  if (!selectedTransactionId.value) return;

  if (confirm("この取引を削除してもよろしいですか？")) {
    const { deleteTransaction } = useTransactions();
    const success = await deleteTransaction(selectedTransactionId.value);

    if (success) {
      isDetailModalOpen.value = false;
      selectedTransactionId.value = null;
      await fetchTransactions(currentFilters.value);
    }
  }
};

const closeEditModal = () => {
  isEditModalOpen.value = false;
};

const handleEditSuccess = () => {
  fetchTransactions(currentFilters.value);
};

onMounted(async () => {
  await fetchTransactions();
  await nextTick();
  measureHeights();

  if (listContainerRef.value) {
    resizeObserver = new ResizeObserver(() => {
      measureHeights();
    });
    resizeObserver.observe(listContainerRef.value);
  }
});

onUnmounted(() => {
  if (resizeObserver) {
    resizeObserver.disconnect();
    resizeObserver = null;
  }
});
</script>

<template>
  <DefaultLayout>
    <div class="max-w-7xl mx-auto h-full flex flex-col">
      <!-- ヘッダー部分 -->
      <div class="flex-shrink-0 space-y-6 mb-6">
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
      </div>

      <!-- メインコンテンツ -->
      <div class="flex-1 flex flex-col min-h-0">
        <div v-if="isLoading" class="flex-1 flex items-center justify-center">
          <BaseText variant="body" color="gray">読み込み中...</BaseText>
        </div>

        <div
          v-else-if="errorMessage"
          class="flex-1 flex items-center justify-center"
        >
          <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
        </div>

        <div
          v-else-if="transactions.length === 0"
          class="flex-1 flex items-center justify-center"
        >
          <BaseText variant="body" color="gray">取引がありません</BaseText>
        </div>

        <template v-else>
          <div
            ref="listContainerRef"
            class="flex-1 space-y-4 overflow-y-auto pr-2"
          >
            <BaseCard
              v-for="transaction in transactions"
              :key="transaction.id"
              data-transaction-card
              clickable
              @click="openDetail(transaction.id)"
              class="hover:shadow-lg transition-shadow"
            >
              <div class="flex items-center justify-between gap-4">
                <div class="flex-1 min-w-0">
                  <div class="flex items-center gap-2 mb-2 flex-wrap">
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
                        CategoryLabels[transaction.category] ||
                        transaction.category
                      }}
                    </BaseBadge>
                    <!-- 税区分バッジ -->
                    <BaseBadge
                      v-if="transaction.taxInclusionType"
                      color="info"
                      size="sm"
                    >
                      {{ TaxInclusionTypeLabels[transaction.taxInclusionType] }}
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
                    {{
                      formatAmount(transaction.amountTotal, transaction.type)
                    }}
                  </BaseText>
                </div>
              </div>
            </BaseCard>
          </div>

          <div
            class="flex items-center justify-between pt-4 flex-shrink-0 border-t border-gray-200 mt-4"
            data-pagination
          >
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
        </template>
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
      @edit="handleEdit"
      @delete="handleDelete"
    />

    <TransactionFormModal
      v-if="selectedTransactionId"
      :is-open="isEditModalOpen"
      :mode="'manual'"
      :transaction-id="selectedTransactionId"
      @close="closeEditModal"
      @success="handleEditSuccess"
    />
  </DefaultLayout>
</template>
