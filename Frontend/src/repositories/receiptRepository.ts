import apiClient from "../api/axios";
import type { ReceiptParseResult } from "../types/receipt";

interface ApiResponse<T> {
  status: "Success" | "Fail";
  message: string | null;
  data: T;
}

export const receiptRepository = {
  async parseReceipt(file: File): Promise<ReceiptParseResult> {
    const formData = new FormData();
    formData.append("File", file);
    formData.append("Options.IncludeRaw", "true");

    const response = await apiClient.post<ApiResponse<ReceiptParseResult>>(
      "/ReceiptParse/Receipt-parse",
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      }
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "領収書の解析に失敗しました");
    }

    return response.data.data;
  },
};
