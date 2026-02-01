/**
 * APIレスポンスの共通型
 */
export interface ApiResponse<T> {
  status: "Success" | "Fail";
  message: string | null;
  data: T;
}

/**
 * ログイン結果（Google/GitHub 共通）
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
 * Google ログインリクエスト
 */
export interface GoogleLoginRequest {
  idToken: string;
}

/**
 * GitHub ログインリクエスト
 */
export interface GitHubLoginRequest {
  code: string;
  state?: string | null;
}

/**
 * トークン更新リクエスト
 */
export interface RefreshTokenRequest {
  refreshToken: string;
}
