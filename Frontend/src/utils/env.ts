import { mockDemoUser } from "../mocks/data/demoUser";

/**
 * 環境変数の取得ヘルパ。
 *
 * window.ENV (Docker / k3s で entrypoint.sh が config.js から注入) を優先し、
 * 未定義なら import.meta.env (Vite build-time に焼き込み) にフォールバックする。
 * GitHub Pages デモは window.ENV が無いため build-time 値で動作する。
 */

export const getApiBaseUrl = (): string =>
  window.ENV?.API_BASE_URL ||
  import.meta.env.VITE_API_BASE_URL ||
  "http://localhost:8080";

export const getGoogleClientId = (): string =>
  window.ENV?.GOOGLE_CLIENT_ID ||
  import.meta.env.VITE_GOOGLE_CLIENT_ID ||
  "";

export const getGitHubClientId = (): string =>
  window.ENV?.GITHUB_CLIENT_ID ||
  import.meta.env.VITE_GITHUB_CLIENT_ID ||
  "";

export const getEnvironment = (): string =>
  window.ENV?.ENVIRONMENT ||
  import.meta.env.VITE_ENVIRONMENT ||
  "";

/**
 * デモモード判定（環境変数ベース）
 */
export const isDemoMode = (): boolean => getEnvironment() === "demo";

/**
 * デモユーザーでログインしているか判定（ユーザーIDベース）
 */
export const isDemoUser = (): boolean => {
  try {
    const userStr = localStorage.getItem("user");
    if (!userStr) return false;

    const user = JSON.parse(userStr);

    // デモユーザーのIDで判定
    return user.userId === mockDemoUser.userId;
  } catch (error) {
    console.error("ユーザー情報の取得に失敗:", error);
    return false;
  }
};

/**
 * 認証済みかどうか判定
 */
export const isAuthenticated = (): boolean => {
  const accessToken = localStorage.getItem("accessToken");
  return !!accessToken;
};
