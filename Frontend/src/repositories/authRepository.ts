/**
 * authRepository - 認証API呼び出し層
 *
 * バックエンドの /api/Auth エンドポイントとの通信を担当
 */

import apiClient from "../api/axios";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/**
 * APIレスポンスの共通型
 */
interface ApiResponse<T> {
  status: "Success" | "Fail";
  message: string | null;
  data: T;
}

/**
 * Google ログインのレスポンス型
 */
export interface LoginResult {
  accessToken: string;
  refreshToken: string;
  userId: string;
  displayName: string;
  pictureUrl: string;
}

/**
 * 現在のユーザー情報
 */
export interface CurrentUser {
  userId: string;
  displayName: string;
  email: string;
}

/**
 * 認証リポジトリ
 */
export const authRepository = {
  /**
   * Google ID Token を使ってログイン
   * @param idToken Google から取得した ID Token
   * @returns ログイン結果（トークン、ユーザー情報）
   */
  async loginWithGoogle(idToken: string): Promise<LoginResult> {
    const response = await fetch(`${API_BASE_URL}/api/Auth/google`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ idToken }),
    });

    if (!response.ok) {
      throw new Error(`HTTP Error: ${response.status}`);
    }

    const result: ApiResponse<LoginResult> = await response.json();

    if (result.status !== "Success") {
      throw new Error(result.message || "認証に失敗しました");
    }

    return result.data;
  },

  async refreshToken(refreshToken: string): Promise<LoginResult> {
    const response = await fetch(`${API_BASE_URL}/api/Auth/refresh`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ refreshToken }),
    });

    if (!response.ok) {
      throw new Error(`HTTP Error: ${response.status}`);
    }

    const result: ApiResponse<LoginResult> = await response.json();

    if (result.status !== "Success") {
      throw new Error(result.message || "トークンの更新に失敗しました");
    }

    return result.data;
  },

  /**
   * 現在のユーザー情報を取得
   * @param accessToken アクセストークン
   * @returns 現在のユーザー情報
   */
  async getCurrentUser(): Promise<CurrentUser> {
    const response = await apiClient.get<ApiResponse<CurrentUser>>(
      "/api/Auth/me"
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "ユーザー情報の取得に失敗しました"
      );
    }

    return response.data.data;
  },
};
