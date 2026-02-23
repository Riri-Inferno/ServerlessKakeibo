import apiClient from "../api/axios";
import { isDemoMode } from "../utils/env";
import { generateTransactionCategories } from "../mocks/helpers";
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
    includeHidden = false
  ): Promise<TransactionCategoryListResult> {
    // デモモード
    if (isDemoMode()) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return generateTransactionCategories(includeHidden);
    }

    // 実API
    const response = await apiClient.get<
      ApiResponse<TransactionCategoryListResult>
    >("/api/categories/transaction", {
      params: { includeHidden },
    });

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引カテゴリの取得に失敗しました"
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
    request: CreateTransactionCategoryRequest
  ): Promise<TransactionCategoryResult> {
    // デモモード：作成不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードではカテゴリの作成はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.post<
      ApiResponse<TransactionCategoryResult>
    >("/api/categories/transaction", request);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引カテゴリの作成に失敗しました"
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
    request: UpdateTransactionCategoryRequest
  ): Promise<TransactionCategoryResult> {
    // デモモード：更新不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードではカテゴリの更新はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.put<
      ApiResponse<TransactionCategoryResult>
    >(`/api/categories/transaction/${id}`, request);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引カテゴリの更新に失敗しました"
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
    // デモモード：削除不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードではカテゴリの削除はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.delete<
      ApiResponse<TransactionCategoryResult>
    >(`/api/categories/transaction/${id}`);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引カテゴリの削除に失敗しました"
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
    // デモモード：復元不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードではカテゴリの復元はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.post<
      ApiResponse<TransactionCategoryResult>
    >(`/api/categories/transaction/${id}/restore`);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引カテゴリの復元に失敗しました"
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
    // デモモード：リセット不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードではマスタ設定へのリセットはできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.post<
      ApiResponse<TransactionCategoryListResult>
    >("/api/categories/transaction/reset");

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "マスタ設定への復元に失敗しました"
      );
    }

    return response.data.data;
  },

  /**
   * カテゴリの並び順を一括更新
   */
  async updateCategoryOrders(
    orders: Array<{ id: string; displayOrder: number }>
  ): Promise<TransactionCategoryListResult> {
    // デモモード：並び順変更不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードでは並び順の変更はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.put<
      ApiResponse<TransactionCategoryListResult>
    >("/api/categories/transaction/order", {
      orders,
    });

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "並び順の更新に失敗しました");
    }

    return response.data.data;
  },
};
