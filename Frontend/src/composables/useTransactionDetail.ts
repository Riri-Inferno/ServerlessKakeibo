import { ref } from "vue";
import { transactionRepository } from "../repositories/transactionRepository";
import type {
  TransactionDetail,
  UpdateTransactionRequest,
  TransactionResult,
  ReceiptImageUrlResult,
} from "../types/transaction";

export function useTransactionDetail() {
  const transaction = ref<TransactionDetail | null>(null);
  const receiptImageUrl = ref<string | null>(null);
  const receiptImageExpiresAt = ref<Date | null>(null);
  const isLoading = ref(false);
  const isLoadingImage = ref(false);
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
   * レシート画像の署名付きURLを取得
   *
   * 注意: URLは1時間有効です。有効期限切れの場合は再取得してください。
   */
  const fetchReceiptImageUrl = async (id: string): Promise<boolean> => {
    isLoadingImage.value = true;
    errorMessage.value = "";

    try {
      const result: ReceiptImageUrlResult =
        await transactionRepository.getReceiptImageUrl(id);
      receiptImageUrl.value = result.signedUrl;
      receiptImageExpiresAt.value = new Date(result.expiresAt);
      return true;
    } catch (error) {
      console.error("レシート画像URL取得エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "レシート画像の取得に失敗しました";
      return false;
    } finally {
      isLoadingImage.value = false;
    }
  };

  /**
   * レシート画像URLが有効期限切れかチェック
   */
  const isReceiptImageExpired = (): boolean => {
    if (!receiptImageExpiresAt.value) return true;
    return new Date() >= receiptImageExpiresAt.value;
  };

  /**
   * レシート画像を添付
   */
  const attachReceipt = async (id: string, _: File): Promise<boolean> => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      // const result = await transactionRepository.attachReceipt(id, file);

      // 添付成功後、取引詳細を再取得（sourceUrlとreceiptAttachedAtが更新される）
      await fetchDetail(id);

      return true;
    } catch (error) {
      console.error("レシート添付エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "レシート画像の添付に失敗しました";
      return false;
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
    receiptImageUrl,
    receiptImageExpiresAt,
    isLoading,
    isLoadingImage,
    errorMessage,
    fetchDetail,
    fetchReceiptImageUrl,
    isReceiptImageExpired,
    attachReceipt,
    updateTransaction,
    deleteTransaction,
  };
}
