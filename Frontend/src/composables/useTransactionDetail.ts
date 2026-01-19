import { ref } from "vue";
import { transactionRepository } from "../repositories/transactionRepository";
import type { TransactionDetail } from "../types/transaction";

export function useTransactionDetail() {
  const transaction = ref<TransactionDetail | null>(null);
  const isLoading = ref(false);
  const errorMessage = ref("");

  const fetchDetail = async (id: string) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      transaction.value = await transactionRepository.getDetail(id);
    } catch (error) {
      console.error("取引詳細取得エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "取引詳細の取得に失敗しました";
    } finally {
      isLoading.value = false;
    }
  };

  return {
    transaction,
    isLoading,
    errorMessage,
    fetchDetail,
  };
}
