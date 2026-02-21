<script setup lang="ts">
import { onMounted, onUnmounted, ref, nextTick } from "vue";
import { useTransactions } from "../composables/useTransactions";
import type { GetTransactionsRequest } from "../types/transaction";
import DefaultLayout from "../layouts/DefaultLayout.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseButton from "../components/atoms/BaseButton.vue";
import TransactionFilter from "../components/molecules/TransactionFilter.vue";
import TransactionListItem from "../components/molecules/TransactionListItem.vue";
import TransactionCreateSelectModal from "../components/organisms/TransactionCreateSelectModal.vue";
import TransactionFormModal from "../components/organisms/TransactionFormModal.vue";
import TransactionDetailModal from "../components/organisms/TransactionDetailModal.vue";
import TransactionExportModal from "../components/organisms/TransactionExportModal.vue";
import BaseSpinner from "../components/atoms/BaseSpinner.vue";
import BaseIcon from "../components/atoms/BaseIcon.vue";

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
const isExportModalOpen = ref(false);

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

  const firstCard = listContainerRef.value.querySelector("[data-transaction-item]");
  if (firstCard) {
    const itemRect = firstCard.getBoundingClientRect();
    const gap = isMobile ? 0 : 16;
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

const openExportModal = () => {
  isExportModalOpen.value = true;
};

const closeExportModal = () => {
  isExportModalOpen.value = false;
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

  window.addEventListener("resize", measureHeights);
});

onUnmounted(() => {
  if (resizeObserver) {
    resizeObserver.disconnect();
    resizeObserver = null;
  }
  window.removeEventListener("resize", measureHeights);
});
</script>

<template>
  <DefaultLayout>
    <div class="max-w-7xl mx-auto h-full flex flex-col">
      <div class="flex-shrink-0 space-y-3 md:space-y-4 mb-4 md:mb-6">
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 md:gap-4">
          <div>
            <BaseText variant="h1" class="mb-1 md:mb-2">取引一覧</BaseText>
            <BaseText variant="caption" color="gray">全{{ totalCount }}件</BaseText>
          </div>

          <div class="grid grid-cols-2 gap-2 sm:flex sm:gap-2">
            <BaseButton variant="outline" class="w-full sm:w-auto" @click="openExportModal">
              <span class="flex items-center justify-center gap-1">
                <BaseIcon name="download" size="sm" />
                <span class="hidden sm:inline">エクスポート</span>
                <span class="sm:hidden">エクスポート</span>
              </span>
            </BaseButton>

            <BaseButton variant="primary" class="w-full sm:w-auto" @click="openCreateModal">
              <span class="flex items-center justify-center gap-1">
                <BaseIcon name="plus" size="sm" />
                <span>新規登録</span>
              </span>
            </BaseButton>
          </div>
        </div>

        <!-- フィルター -->
        <TransactionFilter @search="handleSearch" @clear="handleClearFilters" />
      </div>

      <div class="flex-1 flex flex-col min-h-0">
        <div v-if="isLoading" class="flex-1 flex items-center justify-center">
          <div class="text-center">
            <BaseSpinner icon="refresh" size="lg" color="primary" label="取引一覧を読み込み中" class="mb-2" />
            <BaseText variant="body" color="gray">読み込み中...</BaseText>
          </div>
        </div>

        <div v-else-if="errorMessage" class="flex-1 flex items-center justify-center">
          <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
        </div>

        <div v-else-if="transactions.length === 0" class="flex-1 flex items-center justify-center">
          <BaseText variant="body" color="gray">取引がありません</BaseText>
        </div>

        <template v-else>
          <div ref="listContainerRef" class="flex-1 overflow-y-auto pr-2">
            <div class="md:hidden bg-white rounded-lg border border-gray-200 shadow-sm">
              <TransactionListItem
                v-for="transaction in transactions"
                :key="transaction.id"
                :transaction="transaction"
                variant="compact"
                data-transaction-item
                @click="openDetail"
              />
            </div>

            <div class="hidden md:block space-y-4">
              <TransactionListItem
                v-for="transaction in transactions"
                :key="transaction.id"
                :transaction="transaction"
                variant="card"
                data-transaction-item
                @click="openDetail"
              />
            </div>
          </div>

          <div
            class="flex items-center justify-between pt-3 md:pt-4 flex-shrink-0 border-t border-gray-200 mt-3 md:mt-4"
            data-pagination
          >
            <BaseButton
              variant="outline"
              :disabled="currentPage === 1"
              @click="prevPage"
              class="text-sm md:text-base px-3 py-1.5 md:px-4 md:py-2"
            >
              前へ
            </BaseButton>

            <BaseText variant="body" color="gray" class="text-sm md:text-base">
              {{ currentPage }} / {{ totalPages }}
            </BaseText>

            <BaseButton
              variant="outline"
              :disabled="currentPage === totalPages"
              @click="nextPage"
              class="text-sm md:text-base px-3 py-1.5 md:px-4 md:py-2"
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

    <TransactionFormModal :is-open="isFormModalOpen" :mode="formMode" @close="closeFormModal" @success="handleCreateSuccess" />

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

    <TransactionExportModal :is-open="isExportModalOpen" :filters="currentFilters" :total-count="totalCount" @close="closeExportModal" />
  </DefaultLayout>
</template>
