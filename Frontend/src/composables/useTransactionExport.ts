import { ref } from "vue";
import { transactionRepository } from "../repositories/transactionRepository";
import type {
  ExportTransactionsRequest,
  TransactionExportResult,
} from "../types/transaction";

/**
 * 取引エクスポート用のComposable
 */
export function useTransactionExport() {
  const isExporting = ref(false);
  const errorMessage = ref("");
  const exportResult = ref<TransactionExportResult | null>(null);

  /**
   * Base64をBlobに変換
   */
  const base64ToBlob = (base64: string, mimeType: string): Blob => {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: mimeType });
  };

  /**
   * Zipファイルをダウンロード
   */
  const downloadZip = (fileName: string, zipDataBase64: string): void => {
    const blob = base64ToBlob(zipDataBase64, "application/zip");
    const url = URL.createObjectURL(blob);

    const a = document.createElement("a");
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);

    URL.revokeObjectURL(url);
  };

  /**
   * エクスポートを実行
   *
   * @param request エクスポート条件
   * @returns 成功時: true, 失敗時: false
   */
  const exportTransactions = async (
    request: ExportTransactionsRequest,
  ): Promise<boolean> => {
    isExporting.value = true;
    errorMessage.value = "";
    exportResult.value = null;

    try {
      const result = await transactionRepository.export(request);

      // 結果を保存（モーダル表示用）
      exportResult.value = result;

      // Zipファイルをダウンロード
      downloadZip(result.fileName, result.zipDataBase64);

      return true;
    } catch (error) {
      console.error("エクスポートエラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "エクスポートに失敗しました";
      return false;
    } finally {
      isExporting.value = false;
    }
  };

  /**
   * 結果をクリア（モーダルを閉じる際に使用）
   */
  const clearResult = () => {
    exportResult.value = null;
    errorMessage.value = "";
  };

  return {
    isExporting,
    errorMessage,
    exportResult,
    exportTransactions,
    clearResult,
  };
}
