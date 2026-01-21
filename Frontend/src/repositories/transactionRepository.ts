import apiClient from "../api/axios";
import type {
  TransactionSummary,
  TransactionDetail,
  PagedResult,
  GetTransactionsRequest,
  CreateTransactionRequest,
  UpdateTransactionRequest,
  TransactionResult,
} from "../types/transaction";

interface ApiResponse<T> {
  status: "Success" | "Fail";
  message: string | null;
  data: T;
}

export const transactionRepository = {
  async getList(
    params: GetTransactionsRequest,
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
      `/Transaction/${id}`,
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "取引詳細の取得に失敗しました");
    }

    return response.data.data;
  },

  async create(request: CreateTransactionRequest): Promise<TransactionResult> {
    const response = await apiClient.post<ApiResponse<TransactionResult>>(
      "/Transaction",
      request,
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "取引の登録に失敗しました");
    }

    return response.data.data;
  },

  /**
   * 取引を更新
   */
  async update(
    id: string,
    request: UpdateTransactionRequest,
  ): Promise<TransactionResult> {
    const response = await apiClient.put<ApiResponse<TransactionResult>>(
      `/Transaction/${id}`,
      request,
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "取引の更新に失敗しました");
    }

    return response.data.data;
  },

  /**
   * 取引を削除
   */
  async delete(id: string): Promise<void> {
    const response = await apiClient.delete<
      ApiResponse<{ transactionId: string }>
    >(`/Transaction/${id}`);

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "取引の削除に失敗しました");
    }
  },
};
