import { ref, computed } from "vue";
import { transactionRepository } from "../repositories/transactionRepository";
import type {
  TransactionSummary,
  GetTransactionsRequest,
} from "../types/transaction";

export function useTransactions() {
  const transactions = ref<TransactionSummary[]>([]);
  const totalCount = ref(0);
  const currentPage = ref(1);
  const pageSize = ref(20);
  const isLoading = ref(false);
  const errorMessage = ref("");

  const totalPages = computed(() =>
    Math.ceil(totalCount.value / pageSize.value),
  );

  const fetchTransactions = async (params?: GetTransactionsRequest) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      const result = await transactionRepository.getList({
        page: currentPage.value,
        pageSize: pageSize.value,
        ...params,
      });

      transactions.value = result.items;
      totalCount.value = result.totalCount;
      currentPage.value = result.page;
    } catch (error) {
      console.error("取引一覧取得エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "取引一覧の取得に失敗しました";
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 取引を削除して一覧を再取得
   */
  const deleteTransaction = async (id: string): Promise<boolean> => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      await transactionRepository.delete(id);
      // 削除後に一覧を再取得
      await fetchTransactions();
      return true;
    } catch (error) {
      console.error("取引削除エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "取引の削除に失敗しました";
      return false;
    } finally {
      isLoading.value = false;
    }
  };

  const nextPage = () => {
    if (currentPage.value < totalPages.value) {
      currentPage.value++;
      fetchTransactions();
    }
  };

  const prevPage = () => {
    if (currentPage.value > 1) {
      currentPage.value--;
      fetchTransactions();
    }
  };

  const goToPage = (page: number) => {
    if (page >= 1 && page <= totalPages.value) {
      currentPage.value = page;
      fetchTransactions();
    }
  };

  return {
    transactions,
    totalCount,
    currentPage,
    pageSize,
    totalPages,
    isLoading,
    errorMessage,
    fetchTransactions,
    deleteTransaction,
    nextPage,
    prevPage,
    goToPage,
  };
}
