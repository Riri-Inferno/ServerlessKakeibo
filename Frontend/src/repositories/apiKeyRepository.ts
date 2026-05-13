import apiClient from "../api/axios";
import { isDemoMode } from "../utils/env";
import type {
  ApiKeyDto,
  CreateApiKeyRequest,
  CreateApiKeyResult,
} from "../types/apiKey";

interface ApiResponse<T> {
  status: "Success" | "Fail";
  message: string | null;
  data: T;
}

const DEMO_MODE_MESSAGE =
  "デモモードではAPIキーの管理はできません。実際のアカウントでお試しください。";

export const apiKeyRepository = {
  /**
   * APIキーを発行する
   * レスポンスの `key` は発行直後のみ平文で返却される
   */
  async create(request: CreateApiKeyRequest): Promise<CreateApiKeyResult> {
    if (isDemoMode()) {
      throw new Error(DEMO_MODE_MESSAGE);
    }

    const response = await apiClient.post<ApiResponse<CreateApiKeyResult>>(
      "/api/api-keys",
      request,
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "APIキーの発行に失敗しました");
    }

    return response.data.data;
  },

  /**
   * 自分のAPIキー一覧を取得（失効済みも含む）
   */
  async list(): Promise<ApiKeyDto[]> {
    if (isDemoMode()) {
      return [];
    }

    const response =
      await apiClient.get<ApiResponse<ApiKeyDto[]>>("/api/api-keys");

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "APIキー一覧の取得に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * APIキーを失効させる（物理削除はしない）
   */
  async revoke(id: string): Promise<void> {
    if (isDemoMode()) {
      throw new Error(DEMO_MODE_MESSAGE);
    }

    const response = await apiClient.delete<ApiResponse<unknown>>(
      `/api/api-keys/${id}`,
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "APIキーの失効に失敗しました");
    }
  },
};
