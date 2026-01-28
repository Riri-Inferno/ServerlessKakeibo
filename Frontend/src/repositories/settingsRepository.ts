import apiClient from "../api/axios";
import type {
  UserSettings,
  UpdateUserSettingsRequest,
  DeleteAllTransactionsResult,
} from "../types/settings";

interface ApiResponse<T> {
  status: "Success" | "Fail";
  message: string | null;
  data: T;
}

export const settingsRepository = {
  /**
   * ユーザー設定を取得
   *
   * @returns ユーザー設定
   */
  async getUserSettings(): Promise<UserSettings> {
    const response =
      await apiClient.get<ApiResponse<UserSettings>>("/api/user/settings");

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "ユーザー設定の取得に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * ユーザー設定を更新
   *
   * @param request 更新リクエスト
   * @returns 更新後のユーザー設定
   */
  async updateUserSettings(
    request: UpdateUserSettingsRequest,
  ): Promise<UserSettings> {
    const response = await apiClient.put<ApiResponse<UserSettings>>(
      "/api/user/settings",
      request,
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "ユーザー設定の更新に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * ユーザーの全取引データを削除
   *
   * @returns 削除結果
   */
  async deleteAllTransactions(): Promise<DeleteAllTransactionsResult> {
    const response = await apiClient.delete<
      ApiResponse<DeleteAllTransactionsResult>
    >("/api/UserData/transactions");

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引データの削除に失敗しました",
      );
    }

    return response.data.data;
  },
};
