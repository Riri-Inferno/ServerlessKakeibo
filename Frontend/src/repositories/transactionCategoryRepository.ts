import apiClient from "../api/axios";
import type { ApiResponse } from "../types/api";
import type {
  //   TransactionCategoryDto,
  TransactionCategoryResult,
  TransactionCategoryListResult,
  CreateTransactionCategoryRequest,
  UpdateTransactionCategoryRequest,
} from "../types/transactionCategory";

export const transactionCategoryRepository = {
  /**
   * 取引カテゴリ一覧を取得
   *
   * @param includeHidden 非表示カテゴリも含めるか
   * @returns カテゴリ一覧
   */
  async getCategories(
    includeHidden = false,
  ): Promise<TransactionCategoryListResult> {
    const response = await apiClient.get<
      ApiResponse<TransactionCategoryListResult>
    >("/api/categories/transaction", {
      params: { includeHidden },
    });

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引カテゴリの取得に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * カスタム取引カテゴリを作成
   *
   * @param request 作成リクエスト
   * @returns 作成結果
   */
  async createCategory(
    request: CreateTransactionCategoryRequest,
  ): Promise<TransactionCategoryResult> {
    const response = await apiClient.post<
      ApiResponse<TransactionCategoryResult>
    >("/api/categories/transaction", request);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引カテゴリの作成に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 取引カテゴリを更新
   *
   * @param id カテゴリID
   * @param request 更新リクエスト
   * @returns 更新結果
   */
  async updateCategory(
    id: string,
    request: UpdateTransactionCategoryRequest,
  ): Promise<TransactionCategoryResult> {
    const response = await apiClient.put<
      ApiResponse<TransactionCategoryResult>
    >(`/api/categories/transaction/${id}`, request);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引カテゴリの更新に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 取引カテゴリを削除（非表示化）
   *
   * @param id カテゴリID
   * @returns 削除結果
   */
  async deleteCategory(id: string): Promise<TransactionCategoryResult> {
    const response = await apiClient.delete<
      ApiResponse<TransactionCategoryResult>
    >(`/api/categories/transaction/${id}`);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引カテゴリの削除に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 取引カテゴリを復元
   *
   * @param id カテゴリID
   * @returns 復元結果
   */
  async restoreCategory(id: string): Promise<TransactionCategoryResult> {
    const response = await apiClient.post<
      ApiResponse<TransactionCategoryResult>
    >(`/api/categories/transaction/${id}/restore`);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引カテゴリの復元に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 取引カテゴリをマスタ設定に戻す
   *
   * @returns リセット結果
   */
  async resetToMaster(): Promise<TransactionCategoryListResult> {
    const response = await apiClient.post<
      ApiResponse<TransactionCategoryListResult>
    >("/api/categories/transaction/reset");

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "マスタ設定への復元に失敗しました",
      );
    }

    return response.data.data;
  },
};
