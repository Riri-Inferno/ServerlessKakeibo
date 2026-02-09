import apiClient from "../api/axios";
import type { ApiResponse } from "../types/api";
import type {
  //   ItemCategoryDto,
  ItemCategoryResult,
  ItemCategoryListResult,
  CreateItemCategoryRequest,
  UpdateItemCategoryRequest,
} from "../types/itemCategory";

export const itemCategoryRepository = {
  /**
   * 商品カテゴリ一覧を取得
   *
   * @param includeHidden 非表示カテゴリも含めるか
   * @returns カテゴリ一覧
   */
  async getCategories(includeHidden = false): Promise<ItemCategoryListResult> {
    const response = await apiClient.get<ApiResponse<ItemCategoryListResult>>(
      "/api/categories/item",
      {
        params: { includeHidden },
      },
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "商品カテゴリの取得に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * カスタム商品カテゴリを作成
   *
   * @param request 作成リクエスト
   * @returns 作成結果
   */
  async createCategory(
    request: CreateItemCategoryRequest,
  ): Promise<ItemCategoryResult> {
    const response = await apiClient.post<ApiResponse<ItemCategoryResult>>(
      "/api/categories/item",
      request,
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "商品カテゴリの作成に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 商品カテゴリを更新
   *
   * @param id カテゴリID
   * @param request 更新リクエスト
   * @returns 更新結果
   */
  async updateCategory(
    id: string,
    request: UpdateItemCategoryRequest,
  ): Promise<ItemCategoryResult> {
    const response = await apiClient.put<ApiResponse<ItemCategoryResult>>(
      `/api/categories/item/${id}`,
      request,
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "商品カテゴリの更新に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 商品カテゴリを削除（非表示化）
   *
   * @param id カテゴリID
   * @returns 削除結果
   */
  async deleteCategory(id: string): Promise<ItemCategoryResult> {
    const response = await apiClient.delete<ApiResponse<ItemCategoryResult>>(
      `/api/categories/item/${id}`,
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "商品カテゴリの削除に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 商品カテゴリを復元
   *
   * @param id カテゴリID
   * @returns 復元結果
   */
  async restoreCategory(id: string): Promise<ItemCategoryResult> {
    const response = await apiClient.post<ApiResponse<ItemCategoryResult>>(
      `/api/categories/item/${id}/restore`,
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "商品カテゴリの復元に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 商品カテゴリをマスタ設定に戻す
   *
   * @returns リセット結果
   */
  async resetToMaster(): Promise<ItemCategoryListResult> {
    const response = await apiClient.post<ApiResponse<ItemCategoryListResult>>(
      "/api/categories/item/reset",
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "マスタ設定への復元に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 商品カテゴリの並び順を一括更新
   */
  async updateCategoryOrders(
    orders: Array<{ id: string; displayOrder: number }>,
  ): Promise<ItemCategoryListResult> {
    const response = await apiClient.put<ApiResponse<ItemCategoryListResult>>(
      "/api/categories/item/order",
      {
        orders,
      },
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "並び順の更新に失敗しました");
    }

    return response.data.data;
  },
};
