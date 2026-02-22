import apiClient from "../api/axios";
import { isDemoMode } from "../utils/env";
import { generateIncomeItemCategories } from "../mocks/helpers";
import type { ApiResponse } from "../types/api";
import type {
  //   IncomeItemCategoryDto,
  IncomeItemCategoryResult,
  IncomeItemCategoryListResult,
  CreateIncomeItemCategoryRequest,
  UpdateIncomeItemCategoryRequest,
} from "../types/incomeItemCategory";

export const incomeItemCategoryRepository = {
  /**
   * 給与項目カテゴリ一覧を取得
   *
   * @param includeHidden 非表示カテゴリも含めるか
   * @returns カテゴリ一覧
   */
  async getCategories(
    includeHidden = false
  ): Promise<IncomeItemCategoryListResult> {
    // デモモード
    if (isDemoMode()) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return generateIncomeItemCategories(includeHidden);
    }

    // 実API
    const response = await apiClient.get<
      ApiResponse<IncomeItemCategoryListResult>
    >("/api/categories/income-item", {
      params: { includeHidden },
    });

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "給与項目カテゴリの取得に失敗しました"
      );
    }

    return response.data.data;
  },

  /**
   * カスタム給与項目カテゴリを作成
   *
   * @param request 作成リクエスト
   * @returns 作成結果
   */
  async createCategory(
    request: CreateIncomeItemCategoryRequest
  ): Promise<IncomeItemCategoryResult> {
    // デモモード：作成不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードではカテゴリの作成はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.post<
      ApiResponse<IncomeItemCategoryResult>
    >("/api/categories/income-item", request);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "給与項目カテゴリの作成に失敗しました"
      );
    }

    return response.data.data;
  },

  /**
   * 給与項目カテゴリを更新
   *
   * @param id カテゴリID
   * @param request 更新リクエスト
   * @returns 更新結果
   */
  async updateCategory(
    id: string,
    request: UpdateIncomeItemCategoryRequest
  ): Promise<IncomeItemCategoryResult> {
    // デモモード：更新不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードではカテゴリの更新はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.put<ApiResponse<IncomeItemCategoryResult>>(
      `/api/categories/income-item/${id}`,
      request
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "給与項目カテゴリの更新に失敗しました"
      );
    }

    return response.data.data;
  },

  /**
   * 給与項目カテゴリを削除（非表示化）
   *
   * @param id カテゴリID
   * @returns 削除結果
   */
  async deleteCategory(id: string): Promise<IncomeItemCategoryResult> {
    // デモモード：削除不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードではカテゴリの削除はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.delete<
      ApiResponse<IncomeItemCategoryResult>
    >(`/api/categories/income-item/${id}`);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "給与項目カテゴリの削除に失敗しました"
      );
    }

    return response.data.data;
  },

  /**
   * 給与項目カテゴリを復元
   *
   * @param id カテゴリID
   * @returns 復元結果
   */
  async restoreCategory(id: string): Promise<IncomeItemCategoryResult> {
    // デモモード：復元不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードではカテゴリの復元はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.post<
      ApiResponse<IncomeItemCategoryResult>
    >(`/api/categories/income-item/${id}/restore`);

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "給与項目カテゴリの復元に失敗しました"
      );
    }

    return response.data.data;
  },

  /**
   * 給与項目カテゴリをマスタ設定に戻す
   *
   * @returns リセット結果
   */
  async resetToMaster(): Promise<IncomeItemCategoryListResult> {
    // デモモード：リセット不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードではマスタ設定へのリセットはできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.post<
      ApiResponse<IncomeItemCategoryListResult>
    >("/api/categories/income-item/reset");

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "マスタ設定への復元に失敗しました"
      );
    }

    return response.data.data;
  },

  /**
   * 給与項目カテゴリの並び順を一括更新
   */
  async updateCategoryOrders(
    orders: Array<{ id: string; displayOrder: number }>
  ): Promise<IncomeItemCategoryListResult> {
    // デモモード：並び順変更不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードでは並び順の変更はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.put<
      ApiResponse<IncomeItemCategoryListResult>
    >("/api/categories/income-item/order", {
      orders,
    });

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "並び順の更新に失敗しました");
    }

    return response.data.data;
  },
};
