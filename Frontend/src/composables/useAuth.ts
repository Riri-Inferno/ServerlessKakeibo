import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import {
  authRepository,
  type CurrentUser,
} from "../repositories/authRepository";
import { useAuthStore } from "../stores/authStore";

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

    try {
      // リポジトリ経由で取得
      return await authRepository.getCurrentUser(authStore.accessToken);
    } catch (error) {
      // 401 ならリフレッシュトークンで再試行
      if (error instanceof Error && error.message.includes("401")) {
        await authStore.refreshAccessToken();
        // 再度リクエスト
        return await authRepository.getCurrentUser(authStore.accessToken!);
      }
      throw error;
    }
  };

  /**
   * トークンをリフレッシュ
   */
  const refreshToken = async () => {
    try {
      await authStore.refreshAccessToken();
    } catch (error) {
      // リフレッシュ失敗 → ログアウト
      logout();
      throw error;
    }
  };

  return {
    // State
    isLoading,
    errorMessage,
    isAuthenticated,
    user,
    accessToken,

    // Actions
    logout,
    fetchCurrentUser,
    refreshToken,
  };
}
