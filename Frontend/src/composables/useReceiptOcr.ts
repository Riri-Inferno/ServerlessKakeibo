import { ref } from "vue";
import { receiptRepository } from "../repositories/receiptRepository";
import type { ReceiptParseResult } from "../types/receipt";

export function useReceiptOcr() {
  const isLoading = ref(false);
  const errorMessage = ref("");
  const result = ref<ReceiptParseResult | null>(null);

  const parseReceipt = async (
    file: File
  ): Promise<ReceiptParseResult | null> => {
    isLoading.value = true;
    errorMessage.value = "";
    result.value = null;

    try {
      const parseResult = await receiptRepository.parseReceipt(file);
      result.value = parseResult;
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
  };

  return {
    isLoading,
    errorMessage,
    result,
    parseReceipt,
    reset,
  };
}
