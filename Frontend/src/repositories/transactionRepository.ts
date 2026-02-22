import apiClient from "../api/axios";
import { isDemoMode } from "../utils/env";
import { mockTransactions } from "../mocks/data/transactions";
import { toTransactionSummaries } from "../mocks/helpers";
import type {
  TransactionSummary,
  TransactionDetail,
  PagedResult,
  GetTransactionsRequest,
  CreateTransactionRequest,
  UpdateTransactionRequest,
  TransactionResult,
  ReceiptImageUrlResult,
  ExportTransactionsRequest,
  TransactionExportResult,
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
    // デモモード：モックデータを返す
    if (isDemoMode()) {
      await new Promise((resolve) => setTimeout(resolve, 300)); // 遅延シミュレート

      let filtered = [...mockTransactions];

      // フィルタリング
      if (params.startDate) {
        filtered = filtered.filter((t) => t.transactionDate >= params.startDate!);
      }
      if (params.endDate) {
        filtered = filtered.filter((t) => t.transactionDate <= params.endDate!);
      }
      if (params.type) {
        filtered = filtered.filter((t) => t.type === params.type);
      }
      if (params.userTransactionCategoryId) {
        filtered = filtered.filter(
          (t) => t.userTransactionCategory?.id === params.userTransactionCategoryId
        );
      }

      // 日付降順ソート
      filtered.sort((a, b) => b.transactionDate.localeCompare(a.transactionDate));

      // ページング
      const page = params.page || 1;
      const pageSize = params.pageSize || 20;
      const totalCount = filtered.length;
      const totalPages = Math.ceil(totalCount / pageSize);
      const startIndex = (page - 1) * pageSize;
      const endIndex = startIndex + pageSize;

      return {
        items: toTransactionSummaries(filtered.slice(startIndex, endIndex)),
        totalCount,
        currentPage: page,
        pageSize,
        totalPages,
        hasPreviousPage: page > 1,
        hasNextPage: page < totalPages,
      };
    }

    // 実API呼び出し
    const response = await apiClient.get<
      ApiResponse<PagedResult<TransactionSummary>>
    >("/Transaction", { params });

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "取引一覧の取得に失敗しました");
    }

    return response.data.data;
  },

  async getDetail(id: string): Promise<TransactionDetail> {
    // デモモード：モックデータを返す
    if (isDemoMode()) {
      await new Promise((resolve) => setTimeout(resolve, 200));

      const transaction = mockTransactions.find((t) => t.id === id);
      if (!transaction) {
        throw new Error(`取引が見つかりません（ID: ${id}）`);
      }
      return transaction;
    }

    // 実API呼び出し
    const response = await apiClient.get<ApiResponse<TransactionDetail>>(
      `/Transaction/${id}`,
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "取引詳細の取得に失敗しました");
    }

    return response.data.data;
  },

  async create(request: CreateTransactionRequest): Promise<TransactionResult> {
    // デモモード：エラーを投げる
    if (isDemoMode()) {
      throw new Error(
        "デモモードでは取引の作成はできません。実際のアカウントでお試しください。"
      );
    }

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
    // デモモード：エラーを投げる
    if (isDemoMode()) {
      throw new Error(
        "デモモードでは取引の更新はできません。実際のアカウントでお試しください。"
      );
    }

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
    // デモモード：エラーを投げる
    if (isDemoMode()) {
      throw new Error(
        "デモモードでは取引の削除はできません。実際のアカウントでお試しください。"
      );
    }

    const response = await apiClient.delete<
      ApiResponse<{ transactionId: string }>
    >(`/Transaction/${id}`);

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "取引の削除に失敗しました");
    }
  },

  /**
   * レシート画像を添付
   *
   * @param id 取引ID
   * @param file アップロードするファイル
   * @returns 更新された取引情報（SourceUrlとReceiptAttachedAtを含む）
   */
  async attachReceipt(id: string, file: File): Promise<TransactionResult> {
    // デモモード：エラーを投げる
    if (isDemoMode()) {
      throw new Error(
        "デモモードではレシート画像の添付はできません。実際のアカウントでお試しください。"
      );
    }

    const formData = new FormData();
    formData.append("File", file);

    const response = await apiClient.patch<ApiResponse<TransactionResult>>(
      `/Transaction/${id}/receipt`,
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      },
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "レシート画像の添付に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * レシート画像の署名付きURLを取得
   *
   * @param id 取引ID
   * @returns 署名付きURL（1時間有効）
   */
  async getReceiptImageUrl(id: string): Promise<ReceiptImageUrlResult> {
    // デモモード：エラーを投げる
    if (isDemoMode()) {
      throw new Error(
        "デモモードではレシート画像の取得はできません。実際のアカウントでお試しください。"
      );
    }

    const response = await apiClient.get<ApiResponse<ReceiptImageUrlResult>>(
      `/Transaction/${id}/receipt-image-url`,
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "レシート画像URLの取得に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 取引をエクスポート（CSV + 画像）
   *
   * @param request エクスポート条件
   * @returns エクスポート結果（Base64エンコードされたZip）
   */
  async export(
    request: ExportTransactionsRequest,
  ): Promise<{ result: TransactionExportResult; warnings?: string }> {
    // デモモード：エラーを投げる
    if (isDemoMode()) {
      throw new Error(
        "デモモードではエクスポートはできません。実際のアカウントでお試しください。"
      );
    }

    const response = await apiClient.post<ApiResponse<TransactionExportResult>>(
      "/TransactionExport",
      request,
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "エクスポートに失敗しました");
    }

    return {
      result: response.data.data,
      warnings: response.data.message || undefined,
    };
  },
};
