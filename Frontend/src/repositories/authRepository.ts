/**
 * authRepository - 認証API呼び出し層
 *
 * バックエンドの /api/Auth エンドポイントとの通信を担当
 *
 * 設計方針：
 * - ログイン・リフレッシュ系APIは fetch を使用（axios インターセプターを避けるため）
 * - 認証後のAPIは apiClient (axios) を使用（自動トークン付与とリフレッシュ対応）
 */

import apiClient from "../api/axios";
import type {
  ApiResponse,
  LoginResult,
  CurrentUser,
  GoogleLoginRequest,
  GitHubLoginRequest,
  RefreshTokenRequest,
} from "../types/auth";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/**
 * fetch を使った共通POSTリクエスト（ログイン系API用）
 * axios のインターセプターを避けて循環参照を防ぐ
 */
async function fetchPost<T>(
  endpoint: string,
  body: object,
  errorMessage: string,
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    // エラーレスポンスをパース
    let message = `HTTP Error: ${response.status}`;
    try {
      const errorData: ApiResponse<any> = await response.json();
      if (errorData.message) {
        message = errorData.message;
      }
    } catch {
      // JSONパース失敗時はデフォルトメッセージ
    }

    console.error(`${errorMessage}:`, message);
    throw new Error(message);
  }

  const result: ApiResponse<T> = await response.json();

  if (result.status !== "Success") {
    throw new Error(result.message || errorMessage);
  }

  return result.data;
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
    return fetchPost<LoginResult>(
      "/api/Auth/google",
      { idToken } as GoogleLoginRequest,
      "Google認証に失敗しました",
    );
  },

  /**
   * GitHub 認証コードを使ってログイン
   * @param code GitHub から取得した認証コード
   * @param state CSRF対策用の state（オプション）
   * @returns ログイン結果（トークン、ユーザー情報）
   */
  async loginWithGitHub(code: string, state?: string): Promise<LoginResult> {
    return fetchPost<LoginResult>(
      "/api/Auth/github",
      {
        code,
        state: state || null,
      } as GitHubLoginRequest,
      "GitHub認証に失敗しました",
    );
  },

  /**
   * リフレッシュトークンを使ってアクセストークンを更新
   * @param refreshToken リフレッシュトークン
   * @returns 新しいログイン結果（トークン、ユーザー情報）
   */
  async refreshToken(refreshToken: string): Promise<LoginResult> {
    return fetchPost<LoginResult>(
      "/api/Auth/refresh",
      { refreshToken } as RefreshTokenRequest,
      "トークンの更新に失敗しました",
    );
  },

  /**
   * 現在のユーザー情報を取得
   * @returns 現在のユーザー情報
   */
  async getCurrentUser(): Promise<CurrentUser> {
    try {
      const response =
        await apiClient.get<ApiResponse<CurrentUser>>("/api/Auth/me");

      if (response.data.status !== "Success") {
        throw new Error(
          response.data.message || "ユーザー情報の取得に失敗しました",
        );
      }

      return response.data.data;
    } catch (error: any) {
      console.error("ユーザー情報取得エラー:", error);

      if (error.response?.data?.message) {
        throw new Error(error.response.data.message);
      }

      throw new Error(error.message || "ユーザー情報取得に失敗しました");
    }
  },
};
