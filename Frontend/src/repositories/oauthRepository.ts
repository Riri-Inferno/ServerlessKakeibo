import apiClient from "../api/axios";
import { getApiBaseUrl, isDemoMode } from "../utils/env";
import type {
  OAuthAuthorizeInfo,
  OAuthConnection,
  OAuthConsentRequest,
} from "../types/oauth";

interface ApiResponse<T> {
  status: "Success" | "Fail";
  message: string | null;
  data: T;
}

const DEMO_MODE_MESSAGE =
  "デモモードでは外部アプリとの連携はできません。実際のアカウントでお試しください。";

export const oauthRepository = {
  /**
   * 承認画面に表示するクライアントの情報を取得する
   */
  async getAuthorizeInfo(
    clientId: string,
    redirectUri: string,
    scope: string | null,
  ): Promise<OAuthAuthorizeInfo> {
    if (isDemoMode()) {
      throw new Error(DEMO_MODE_MESSAGE);
    }

    const response = await apiClient.get<ApiResponse<OAuthAuthorizeInfo>>(
      "/api/oauth/authorize-info",
      {
        params: {
          client_id: clientId,
          redirect_uri: redirectUri,
          ...(scope ? { scope } : {}),
        },
      },
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "連携するアプリの情報を取得できませんでした",
      );
    }

    return response.data.data;
  },

  /**
   * 連携を許可し、承認チケットを受け取る
   */
  async consent(request: OAuthConsentRequest): Promise<string> {
    if (isDemoMode()) {
      throw new Error(DEMO_MODE_MESSAGE);
    }

    const response = await apiClient.post<
      ApiResponse<{ consentTicket: string }>
    >("/api/oauth/consent", request);

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "連携の許可に失敗しました");
    }

    return response.data.data.consentTicket;
  },

  /**
   * 認可エンドポイント（backend の /oauth/authorize）に戻る URL を作る。
   * 認可リクエストのパラメータはそのまま付け、許可したならチケット、拒否したなら consent_denied を足す
   */
  buildAuthorizeUrl(
    query: Record<string, string>,
    result: { consentTicket: string } | { denied: true },
  ): string {
    const url = new URL("/oauth/authorize", getApiBaseUrl());
    for (const [key, value] of Object.entries(query)) {
      if (key === "consent_ticket" || key === "consent_denied") continue;
      url.searchParams.append(key, value);
    }
    if ("consentTicket" in result) {
      url.searchParams.append("consent_ticket", result.consentTicket);
    } else {
      url.searchParams.append("consent_denied", "1");
    }
    return url.toString();
  },

  /**
   * 連携中のアプリの一覧
   */
  async listConnections(): Promise<OAuthConnection[]> {
    if (isDemoMode()) {
      return [];
    }

    const response = await apiClient.get<ApiResponse<OAuthConnection[]>>(
      "/api/oauth/connections",
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "連携中のアプリの取得に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 連携を解除する
   */
  async revokeConnection(id: string): Promise<void> {
    if (isDemoMode()) {
      throw new Error(DEMO_MODE_MESSAGE);
    }

    const response = await apiClient.delete<ApiResponse<unknown>>(
      `/api/oauth/connections/${encodeURIComponent(id)}`,
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "連携の解除に失敗しました");
    }
  },
};
