import apiClient from "../api/axios";
import { isDemoMode } from "../utils/env";
import { generateMockSettings } from "../mocks/helpers";
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
    // デモモード
    if (isDemoMode()) {
      await new Promise((resolve) => setTimeout(resolve, 200));
      return generateMockSettings();
    }

    // 実API
    const response =
      await apiClient.get<ApiResponse<UserSettings>>("/api/user/settings");

    if (response.data      .status !== "Success") {
      throw new Error(
        response.data.message || "ユーザー設定の取得に失敗しました"
      );
    }

    return response.data.data   ;
  },

  /**
   * ユーザー設定を更新
   */
  async updateUserSettings(
    request: UpdateUserSettingsRequest
  ): Promise<UserSettings> {
    // デモモード：更新不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードでは設定の変更はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.put<ApiResponse<UserSettings>>(
      "/api/user/settings",
      request
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "ユーザー設定の更新に失敗しました"
      );
    }

    return response.data.data;
  },

  /**
   * ユーザーの全取引データを削除
   */
  async deleteAllTransactions(): Promise<DeleteAllTransactionsResult> {
    // デモモード：削除不可
    if (isDemoMode()) {
      throw new Error(
        "デモモードでは取引データの削除はできません。実際のアカウントでお試しください。"
      );
    }

    // 実API
    const response = await apiClient.delete<
      ApiResponse<DeleteAllTransactionsResult>
    >("/api/UserData/transactions");

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "取引データの削除に失敗しました"
      );
    }

    return response.data.data;
  },
};
