import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import { authRepository } from "../repositories/authRepository";
import { settingsRepository } from "../repositories/settingsRepository";
import { useAuthStore } from "../stores/authStore";
import type { CurrentUser } from "../types/auth";

/**
 * useAuth - 認証処理の Composable
 */
export function useAuth() {
  const router = useRouter();
  const authStore = useAuthStore();

  const isLoading = ref(false);
  const errorMessage = ref("");

  // ストアから取得
  const isAuthenticated = computed(() => authStore.isAuthenticated);
  const user = computed(() => authStore.user);
  const accessToken = computed(() => authStore.accessToken);
  const effectiveDisplayName = computed(() => authStore.effectiveDisplayName);

  /**
   * Google でログイン
   */
  const loginWithGoogle = async (idToken: string) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      const userData = await authRepository.loginWithGoogle(idToken);
      authStore.setAuthData(userData);
      await fetchSettings();
      return userData;
    } catch (error) {
      console.error("Google ログインエラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "ログインに失敗しました";
      throw error;
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * GitHub でログイン
   */
  const loginWithGitHub = async (code: string, state?: string) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      const userData = await authRepository.loginWithGitHub(code, state);
      authStore.setAuthData(userData);
      await fetchSettings();
      return userData;
    } catch (error) {
      console.error("GitHub ログインエラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "GitHub 認証に失敗しました";
      throw error;
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * ログアウト
   */
  const logout = () => {
    authStore.logout();
    router.push({ name: "login" });
  };

  /**
   * 現在のユーザー情報を取得（/api/Auth/me）
   */
  const fetchCurrentUser = async (): Promise<CurrentUser> => {
    if (!authStore.accessToken) {
      throw new Error("認証されていません");
    }

    return await authRepository.getCurrentUser();
  };

  /**
   * 設定を取得してストアに保存
   */
  const fetchSettings = async () => {
    try {
      const settings = await settingsRepository.getUserSettings();
      authStore.setSettings(settings);
    } catch (error) {
      console.error("設定取得エラー:", error);
      // エラーでも処理は継続
    }
  };

  /**
   * GitHub OAuth の認証URLを生成
   */
  const getGitHubAuthUrl = () => {
    const GITHUB_CLIENT_ID = import.meta.env.VITE_GITHUB_CLIENT_ID;
    const REDIRECT_URI = `${window.location.origin}/auth/callback`;
    const state = generateRandomState();

    // state をセッションストレージに保存
    sessionStorage.setItem("github_oauth_state", state);

    const authUrl = new URL("https://github.com/login/oauth/authorize");
    authUrl.searchParams.append("client_id", GITHUB_CLIENT_ID);
    authUrl.searchParams.append("redirect_uri", REDIRECT_URI);
    authUrl.searchParams.append("scope", "read:user user:email");
    authUrl.searchParams.append("state", state);

    return authUrl.toString();
  };

  /**
   * GitHub ログインを開始（リダイレクト）
   */
  const startGitHubLogin = () => {
    window.location.href = getGitHubAuthUrl();
  };

  return {
    // State
    isLoading,
    errorMessage,
    isAuthenticated,
    user,
    accessToken,
    effectiveDisplayName,

    // Actions
    loginWithGoogle,
    loginWithGitHub,
    logout,
    fetchCurrentUser,
    fetchSettings,
    getGitHubAuthUrl,
    startGitHubLogin,
  };
}

/**
 * ランダムな state を生成（CSRF 対策）
 */
function generateRandomState(): string {
  return (
    Math.random().toString(36).substring(2, 15) +
    Math.random().toString(36).substring(2, 15)
  );
}
