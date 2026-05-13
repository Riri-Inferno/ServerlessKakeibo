/**
 * APIキー関連の型定義
 */

/**
 * 一覧用 APIキー DTO（平文キー・ハッシュは含まない）
 */
export interface ApiKeyDto {
  id: string;
  name: string;
  keyPrefix: string;
  scopes: string[];
  expiresAt: string | null;
  lastUsedAt: string | null;
  revokedAt: string | null;
  createdAt: string;
}

/**
 * APIキー発行リクエスト
 */
export interface CreateApiKeyRequest {
  name: string;
  scopes: string[];
  expiresAt: string | null;
}

/**
 * APIキー発行結果
 * key は **発行直後のみ** 平文で返ってくる
 */
export interface CreateApiKeyResult {
  id: string;
  name: string;
  key: string;
  keyPrefix: string;
  scopes: string[];
  expiresAt: string | null;
  createdAt: string;
}
