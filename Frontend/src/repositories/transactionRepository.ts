import apiClient from "../api/axios";
import type {
  TransactionSummary,
  TransactionDetail,
  PagedResult,
  GetTransactionsRequest,
} from "../types/transaction";

interface ApiResponse<T> {
  status: "Success" | "Fail";
  message: string | null;
  data: T;
}

export const transactionRepository = {
  async getList(
    params: GetTransactionsRequest
  ): Promise<PagedResult<TransactionSummary>> {
    const response = await apiClient.get<
      ApiResponse<PagedResult<TransactionSummary>>
    >("/Transaction", { params });

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "取引一覧の取得に失敗しました");
    }

    return response.data.data;
  },

  async getDetail(id: string): Promise<TransactionDetail> {
    const response = await apiClient.get<ApiResponse<TransactionDetail>>(
      `/Transaction/${id}`
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "取引詳細の取得に失敗しました");
    }

    return response.data.data;
  },
};
