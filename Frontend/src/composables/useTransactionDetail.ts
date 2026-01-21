import { ref } from "vue";
import { transactionRepository } from "../repositories/transactionRepository";
import type {
  TransactionDetail,
  UpdateTransactionRequest,
  TransactionResult,
} from "../types/transaction";

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

  /**
   * 取引を更新
   */
  const updateTransaction = async (
    id: string,
    request: UpdateTransactionRequest,
  ): Promise<TransactionResult | null> => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      const result = await transactionRepository.update(id, request);
      // 更新後に詳細を再取得
      await fetchDetail(id);
      return result;
    } catch (error) {
      console.error("取引更新エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "取引の更新に失敗しました";
      return null;
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 取引を削除
   */
  const deleteTransaction = async (id: string): Promise<boolean> => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      await transactionRepository.delete(id);
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

  return {
    transaction,
    isLoading,
    errorMessage,
    fetchDetail,
    updateTransaction,
    deleteTransaction,
  };
}
