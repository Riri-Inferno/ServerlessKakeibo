import { mockDemoUser } from "../mocks/data/demoUser";

/**
 * デモモード判定（環境変数ベース）
 * window.ENV.ENVIRONMENT が優先、なければ import.meta.env.VITE_ENVIRONMENT を使用
 */
export const isDemoMode = (): boolean => {
  const env =
    (window as any).ENV?.ENVIRONMENT ||
    import.meta.env.VITE_ENVIRONMENT ||
    "";
  return env === "demo";
};

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
