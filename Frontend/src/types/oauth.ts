/**
 * OAuth 連携（Claude などの MCP クライアント）関連の型定義
 */

/** 承認画面に表示する、連携を求めているクライアントの情報 */
export interface OAuthAuthorizeInfo {
  clientId: string;
  clientName: string;
  clientUri: string | null;
  /** 認可コードを返す先のホスト名。なりすまし対策として承認画面にはっきり表示する */
  redirectHost: string;
  /** クライアントが求めているスコープ（read / write） */
  requestedScopes: string[];
}

/** 連携を許可するリクエスト */
export interface OAuthConsentRequest {
  clientId: string;
  redirectUri: string;
  codeChallenge: string;
  scopes: string[];
}

/** 連携中のアプリ */
export interface OAuthConnection {
  id: string;
  clientId: string | null;
  clientName: string;
  scopes: string[];
  createdAt: string | null;
  lastUsedAt: string | null;
}
