import { ref } from "vue";
import { receiptRepository } from "../repositories/receiptRepository";
import type { ReceiptParseResult } from "../types/receipt";

export function useReceiptOcr() {
  const isLoading = ref(false);
  const errorMessage = ref("");
  const result = ref<ReceiptParseResult | null>(null);

  // 画像保存フラグ（デフォルトはtrue）
  const shouldSaveImage = ref(true);

  // アップロードしたファイルを保持
  const uploadedFile = ref<File | null>(null);

  const parseReceipt = async (
    file: File,
  ): Promise<ReceiptParseResult | null> => {
    isLoading.value = true;
    errorMessage.value = "";
    result.value = null;

    try {
      const parseResult = await receiptRepository.parseReceipt(file);
      result.value = parseResult;

      // ファイルを保持（後で添付する可能性があるため）
      uploadedFile.value = file;

      return parseResult;
    } catch (error) {
      console.error("OCR解析エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "領収書の解析に失敗しました";
      return null;
    } finally {
      isLoading.value = false;
    }
  };

  const reset = () => {
    result.value = null;
    errorMessage.value = "";
    shouldSaveImage.value = true;
    uploadedFile.value = null;
  };

  return {
    isLoading,
    errorMessage,
    result,
    shouldSaveImage,
    uploadedFile,
    parseReceipt,
    reset,
  };
}
